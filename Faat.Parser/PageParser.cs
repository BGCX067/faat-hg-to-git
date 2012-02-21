using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Faat.Parser.Ast;

using MyUtils;

using XTrace;

namespace Faat.Parser
{
	public class PageParser : IPageParser
	{
		public ParsingReport Parse(IPage page)
		{
			return Tokenize(page.Content);
		}

		static readonly XTraceSource Trace = new XTraceSource("Parser");

		public ParsingReport Tokenize(string content)
		{
			return Tokenize(TokenizeByLine(content));
		}

		public IEnumerable<TextLine> TokenizeByLine(string content)
		{
			if (string.IsNullOrWhiteSpace(content))
			{
				return Enumerable.Empty<TextLine>();
			}
			// tokenize by line
			var lines = new List<TextLine>();
			for (int i = 0; i < content.Length && i > -1;)
			{
				var start = i;
				if (i < content.Length)
				{
					i = content.IndexOf('\r', i);
				}
				else
				{
					i = -1;
				}
				if (i == -1)
				{
					var line = new TextLine(content.Substring(start), start, content.Length);
					lines.Add(line);
				}
				else
				{
					var line = new TextLine(content.Substring(start, i - start), start, i);
					lines.Add(line);
					if (content[i + 1] == '\n')
					{
						i++;
					}
					i++;
				}
			}
			return lines;
		}


		ParsingReport Tokenize(IEnumerable<TextLine> lines)
		{
			try
			{

				var ctx = new ParsingContext();
				var page = ctx.CurrentBlock;

				foreach (var lineToken in lines)
				{
					var line = lineToken.String;
					Trace.Verbose("Parser", "TextLine {0}", line);
					bool ignore = false;

					#region Tabulation

					var untabedLine = line.TrimStart();
					var tabs = line.TakeWhile(x => x == '\t').Count();
					Trace.Verbose("Parser", "Tabulation {0}", tabs);

					// comment
					var commentPos = untabedLine.IndexOf('`');
					if (commentPos >= 0)
					{
						untabedLine = untabedLine.Substring(0, commentPos).TrimEnd();
					}

//					if (string.IsNullOrWhiteSpace(untabedLine)) // continue
//					{
//						Trace.Verbose("Parser", "Tabulation - continue on empty line");
//					}
					//else
					if (ctx.CurrentTabulating == tabs) // continue
					{
						Trace.Verbose("Parser", "Tabulation - continue on same tabulation");
					}
					else if (ctx.CurrentTabulating > tabs) // closing
					{
						Trace.Verbose("Parser", "Tabulation - closing");
						while (ctx.CurrentTabulating > tabs)
						{
							ctx.CurrentBlock = ctx.BlockStack.Pop();
							ctx.CurrentTabulating--;
						}
					}
					else if (ctx.CurrentTabulating == tabs - 1) // open one. previous line is block
					{
						Trace.Verbose("Parser", "Tabulation - open one");
						if (!ctx.PreviousBlockCreatedByColumn)
						{
							if (string.IsNullOrWhiteSpace(ctx.PreviousUntabbedLine))
							{
								// немогу создать блок по предыдущей строке, т.к. предыдущая строка была пустая
								throw new ParsingException("Немогу создать блок по предыдущей строке, т.к. предыдущая строка была пустая");
							}
							var parent = ctx.CurrentBlock;
							ctx.BlockStack.Push(ctx.CurrentBlock);
							ctx.CurrentBlock = new Block
							{
								Name = ctx.PreviousUntabbedLine,
							};
							if (ctx.PreviousLineInserted)
							{
								parent.Lines.RemoveAt(parent.Lines.Count - 1);
							}
							parent.Lines.Add(ctx.CurrentBlock);
						}
					}
					else if (ctx.CurrentTabulating < tabs - 1) // error
					{
						throw new ParsingException("Tabulated more than previous line at {0}\r\n{1}\r\n{2}".Arg(ctx.LineNumber, ctx.PreviousLine, line));
					}
					else
					{
						throw new ParsingException("Fatal - unreacheble point expected");
					}

					#endregion

					#region Semicolon

					var currentBlockCreatedByColumn = false;

					var columnAtEnd = untabedLine.LastIndexOf(':');
					var good = columnAtEnd > 0 && ((columnAtEnd == untabedLine.Length - 1) || (columnAtEnd < untabedLine.Length - 1 && untabedLine[columnAtEnd + 1] == ' '));
					if (good)
					{
						Trace.Verbose("Parser", "Column - open one");
						ctx.BlockStack.Push(ctx.CurrentBlock);

						var param = untabedLine.Substring(columnAtEnd + 1).Trim();

						ctx.CurrentBlock = new Block
						{
							Name = untabedLine.Substring(0, columnAtEnd),
							Param = string.IsNullOrWhiteSpace(param) ? null : param,
						};
						ctx.BlockStack.Peek().Lines.Add(ctx.CurrentBlock);
						ignore = true;
						currentBlockCreatedByColumn = true;
					}
					else if (string.IsNullOrWhiteSpace(untabedLine)) // close all blocks on a page or start pagewide block
					{
						if (ctx.PreviousBlockCreatedByColumn) // start pagewide block
						{
							ctx.CurrentBlock.IsPageWide = true; // this block is ended only at the end of a page
						}
						else
						{
							while (ctx.BlockStack.Count > 0 && !ctx.CurrentBlock.IsPageWide)
							{
								ctx.CurrentBlock = ctx.BlockStack.Pop();
							}
							ctx.CurrentTabulating = 0;
						}
						ignore = true;
					}

					#endregion

					if (!ignore)
					{
						ctx.CurrentBlock.Lines.Add(new Line
						{
							String = untabedLine,
							StartOffset = lineToken.StartOffset,
							EndOffset = lineToken.EndOffset,
						});
					}
					ctx.PreviousLineInserted = !ignore;

					ctx.CurrentTabulating = tabs;
					ctx.PreviousLine = line;
					ctx.PreviousUntabbedLine = untabedLine;
					ctx.LineNumber++;
					ctx.PreviousBlockCreatedByColumn = currentBlockCreatedByColumn;
				}

				return new ParsingReport
				{
					PageTokens = page,
					Hash = CalculateHashForPage(page),
				};
			}
			catch (ParsingException ex)
			{
				Trace.Exception(ex);
				var result = new ParsingReport();
				result.ParsingErrors.Add(new ParsingError {ErrorDescription = ex.Message});
				return result;
			}
			catch (Exception ex)
			{
				Trace.Exception(ex);
				throw;
			}
		}

		public int CalculateHashForPage(Block page)
		{
			return page.GetHashCode();
		}

		class ParsingContext
		{
			public readonly Stack<Block> BlockStack = new Stack<Block>();
			public Block CurrentBlock = new Block { Name = "Page" };
			public int CurrentTabulating = 0; // текущий отступ
			// bool currentTabulatingBlockOpenedByPreviousColumnLine = false;
			public string PreviousLine = null;
			public string PreviousUntabbedLine = null;
			public int LineNumber;
			public bool PreviousBlockCreatedByColumn;
			public bool PreviousLineInserted;
		}

	}
}
