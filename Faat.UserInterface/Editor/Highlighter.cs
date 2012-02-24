using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

using Faat.Model;
using Faat.Parser.Ast;

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using Faat.Parser;

using XTrace;

namespace Faat.UserInterface.Editor
{
	public class Highlighter : DocumentColorizingTransformer
	{
		readonly static Brush _line = new SolidColorBrush(Colors.Black);
		readonly static Brush _block = new SolidColorBrush(Colors.Blue);

		readonly PageParserState _pageParserState;

		public Highlighter(PageParserState pageParser)
		{
			_pageParserState = pageParser;
		}

		IEnumerable<Line> GetAstLines(int pos)
		{
			var page = _pageParserState.ExecutedOrSimple;
			if (page == null)
			{
				return null;
			}
			return AllLinesCacheLast(page).Where(x => x.StartOffset <= pos && pos < x.EndOffset);
		}

		Line[] _prevAst;
		Block _prevAstPage;

		IEnumerable<Line> AllLinesCacheLast(Block block)
		{
			if(_prevAstPage != block)
			{
				_prevAstPage = block;
				_prevAst = AllLines(block).ToArray();
			}
			return _prevAst;
		}

		IEnumerable<Line> AllLines(Block block)
		{
			if (block == null)
			{
				throw new ArgumentNullException("block");
			}
			yield return block;
			foreach (var line in block.Lines)
			{
				var subBlock = line as Block;
				if (subBlock != null)
				{
					foreach (var subLine in AllLines(subBlock))
					{
						yield return subLine;
					}
				}
				else if(line != null)
				{
					yield return line;
				}
			}
		}

		static readonly XTraceSource Trace = new XTraceSource("Highlighter");

		protected override void ColorizeLine(DocumentLine line)
		{
			int lineStartOffset = line.Offset;
			string text = CurrentContext.Document.GetText(line);
			Trace.Verbose("ColorizeLine: " + text + " [" + lineStartOffset);

			var astLines = GetAstLines(lineStartOffset);

			if(astLines == null || !astLines.Any())
			{
				return;
			}

			foreach (var astLine in astLines)
			{
				if (astLine.GetType() == typeof(Line))
				{
					ChangeLinePart(lineStartOffset, line.EndOffset, element => element.TextRunProperties.SetForegroundBrush(_line));
				}
				else if (astLine.GetType() == typeof(Block))
				{
					ChangeLinePart(lineStartOffset, line.EndOffset, element => element.TextRunProperties.SetForegroundBrush(_block));
				}

				var brush = GetBrushBuState(astLine.ResultState);
				if (brush != null)
				{
					ChangeLinePart(lineStartOffset, line.EndOffset, element => element.TextRunProperties.SetBackgroundBrush(brush));
				}
			}
		}

		Brush GetBrushBuState(ExecutionResultState? state)
		{
			switch (state)
			{
				case null:
					return null;
				default:
					switch (state.Value)
					{
						case ExecutionResultState.Passed:
							return _passed;
						case ExecutionResultState.Failed:
							return _failed;
						case ExecutionResultState.Warning:
						case ExecutionResultState.Exception:
						case ExecutionResultState.Timeout:
						case ExecutionResultState.BadTest:
						case ExecutionResultState.Unknown:
							return _error;
						default:
							throw new ArgumentOutOfRangeException();
					}
			}
		}

		// standart
		static readonly Brush _passedSimple = new LinearGradientBrush(Color.FromArgb(0x44, 0x77, 0xFF, 0x77), Color.FromArgb(0x44, 0x00, 0x77, 0x00), new Point(0, 0), new Point(0, 1));

		// standart convertors
		static readonly Brush _passed = new LinearGradientBrush((C<Color>)"#4477FF77", (C<Color>)"#44007700", (C<Point>)"0 0", (C<Point>)"0 1");
		static readonly Brush _failed = new LinearGradientBrush((C<Color>)"#44FF7777", (C<Color>)"#44770000", (C<Point>)"0 0", (C<Point>)"0 1");
		static readonly Brush _error = new LinearGradientBrush((C<Color>)"#4477FFFF", (C<Color>)"#44007777", (C<Point>)"0 0", (C<Point>)"0 1");

		// LinearGradientBrush convertor
		static readonly Brush _passed2 = (C<LinearGradientBrush>)"0 1 #4477FF77 #44007700";
		static readonly Brush _passed3 = (C<LinearGradientBrush>)"0 0 0 1 #4477FF77 #44007700";
		static readonly Brush _passed4 = (C<LinearGradientBrush>)"0,0 0 1 #4477FF77 #44007700";
		static readonly Brush _passed5 = (C<LinearGradientBrush>)"0 0 0,1 #4477FF77 #44007700";
		static readonly Brush _passed6 = (C<LinearGradientBrush>)"0,0 0,1 #4477FF77 #44007700";
	}

//	static class Cvts
//	{
//		static readonly Dictionary<Type, TypeConverter> _converters = new Dictionary<Type, TypeConverter>();
//
//		public static TypeConverter GetConverter<T>()
//		{
//			TypeConverter cvt;
//			if (!_converters.TryGetValue(typeof(T), out cvt))
//			{
//				_converters[typeof(T)] = cvt = TypeDescriptor.GetConverter(typeof(T));
//			}
//		}
//	}

	public static class C
	{
		public static C<T> Get<T>(string str)
		{
			return str;
		}
		
	}

	/// <summary>
	/// Implicit invariant string to type convertor
	/// </summary>
	public struct C<T>
	{
		readonly T _value;

		public C(T value)
		{
			_value = value;
		}

		public static implicit operator T(C<T> cvt)
		{
			return cvt._value;
		}

		public static implicit operator C<T>(string from)
		{
			Register.Reg();
			var cvtr = TypeDescriptor.GetConverter(typeof(T));
			var value = (T)cvtr.ConvertFromInvariantString(from);
			return new C<T>(value);
		}
	}

	static class Register
	{
		static Register()
		{
			TypeDescriptor.AddProvider(LinearGradientBrushProvider.Default, typeof(LinearGradientBrush));
		}

		public static void Reg()
		{
		}
	}

	public class LinearGradientBrushProvider : TypeDescriptionProvider
	{
		public static LinearGradientBrushProvider Default = new LinearGradientBrushProvider();

		public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
		{
			if (objectType == typeof(LinearGradientBrush))
			{
				return LinearGradientBrushDescriptor.Default;
			}
			return base.GetTypeDescriptor(objectType, instance);
		}
	} 

	public class LinearGradientBrushDescriptor : CustomTypeDescriptor
	{
		public static LinearGradientBrushDescriptor Default = new LinearGradientBrushDescriptor();

		public override TypeConverter GetConverter()
		{
			return LinearGradientBrushConverter.Default;
		}
	}

	public class LinearGradientBrushConverter : TypeConverter
	{
		public static LinearGradientBrushConverter Default = new LinearGradientBrushConverter();

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
			{
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var brush = new LinearGradientBrush {EndPoint = (C<Point>)"1 1"};

			var str = (string)value;
			var parts_ = str.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

			var parts = new List<string>();
			var stops = new List<string>();

			for (int i = 0; i < parts_.Length; i++)
			{
				if (parts_[i].StartsWith("#"))
				{
					stops.Add(parts_[i]);
				}
				else
				{
					parts.Add(parts_[i]);
				}
			}

			for (int i = 0; i < stops.Count;i++ )
			{
				brush.GradientStops.Add(new GradientStop((C<Color>)stops[i], i / (double)(stops.Count - 1)));
			}

			bool epInited = false;

			string prevPointPart = null;
			for (int i = 0; i < parts.Count; i++)
			{
				if (!IsPoint(parts[i]))
				{
					if (prevPointPart != null)
					{
						var point = (C<Point>)(prevPointPart + " " + parts[i]);
						prevPointPart = null;
						if (epInited)
						{
							brush.StartPoint = point;
						}
						else
						{
							brush.EndPoint = point;
							epInited = true;
						}
					}
					else
					{
						prevPointPart = parts[i];
					}
				}
				else
				{
					if (prevPointPart != null)
					{
						throw new Exception("Point parts does not matched");
					}
					var point = (C<Point>)parts[i];
					if (epInited)
					{
						brush.StartPoint = point;
					}
					else
					{
						brush.EndPoint = point;
						epInited = true;
					}

				}
			}
			if (prevPointPart != null)
			{
				throw new Exception("Point parts does not matched");
			}
			return brush;
			
		}

		[DebuggerStepThrough]
		bool IsPoint(string s)
		{
			try
			{
				Point.Parse(s);
				return true;
			}catch
			{
				return false;
			}
		}
	}
}


