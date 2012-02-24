using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xaml;

using Faat.Client.FaatServiceReference;
using Faat.Model;
using Faat.Parser;
using Faat.Parser.Ast;
using Faat.Storage.Remote;

using MyUtils;

using XTrace;

namespace Faat.Agent
{
	[Serializable]
	public class  ExecutionResult
	{
		public Exception Exception;
		public Block Page;

		public string Serialize()
		{
			return XamlServices.Save(Page);
		}
	}

	public class FaatAgent : MarshalByRefObject
	{
		public ExecutionResult RunPage(string serverAddress, string pageIdentifier, string resultDataIdentifier)
		{
			try
			{
				new StructuredTextFileXTraceListener("FaatAgent", XTrace.RelativePath.TempFolder);
				using (var storage = new RemoteStorage(false, serverAddress))
				{
					return RunPage(storage, pageIdentifier, resultDataIdentifier);
				}
			}
			catch (Exception ex)
			{
				return new ExecutionResult {Exception = ex};
			}
		}

		public ExecutionResult RunPage(RemoteStorage storage, string pageIdentifier, string resultDataIdentifier)
		{
			var result = RunPage(storage.GetPage(pageIdentifier).Content);
/*
			if (resultDataIdentifier != null)
			{
				storage.SetData(resultDataIdentifier, result.Serialize());
			}
*/

			storage.SetData(Const.GlobalPageResultGraphPrefix + pageIdentifier, result.Serialize());
			if (result.Page != null)
			{
				storage.SetData(Const.GlobalPageResultSimplePrefix + pageIdentifier, result.Page.ResultState.ToString());
			}

			return result;
		}

		readonly PageParser _parser = new PageParser();

		class ExecutionContext
		{
			public readonly ExecutionResult Result = new ExecutionResult();
			public object LastOutput;
			public ExecutionResultState? LastResult;
		}

		ExecutionContext Context = new ExecutionContext();

		public ExecutionResult RunPage(string pageBody)
		{
			Context = new ExecutionContext();
			var page = _parser.Tokenize(pageBody).PageTokens;
			if (page == null)
			{
				return null;
			}
			RunBlock(page);

			Context.Result.Page = page;

			return Context.Result;

//			var value = new ValueProvider<ExecutionResult>();
//
//			ThreadPool.QueueUserWorkItem(x=>
//			{
//				var localPageBody = (string)x;
//				try
//				{
//					
//				}catch(Exception ex)
//				{
//					XTrace.XTrace.Exception(ex);
//				}
//			}, pageBody);
		}

		void RunBlock(Block block)
		{
			RunLine(block);
			foreach (var line in block.Lines)
			{
				RunLine(line);
			}
			block.ResultState = block.Lines
				.Where(x=>x.ResultState != null)
				.Select(x=>x.ResultState.Value)
				.OrderByDescending(x => x, StateResultantPriorityComparer.Instance)
				.FirstOrDefault();
		}

		class StateResultantPriorityComparer : IComparer<ExecutionResultState>
		{
			public static readonly StateResultantPriorityComparer Instance = new StateResultantPriorityComparer();

			StateResultantPriorityComparer()
			{
				
			}

			readonly Dictionary<ExecutionResultState, int> _priorities = new Dictionary<ExecutionResultState, int>
			{
				{ExecutionResultState.BadTest, 110},
				{ExecutionResultState.Exception, 100},
				{ExecutionResultState.Failed, 90},
				{ExecutionResultState.Warning, 80},
				{ExecutionResultState.Timeout, 10},
				{ExecutionResultState.Unknown, 0},
				{ExecutionResultState.Passed, -100},
			};

			public int Compare(ExecutionResultState x, ExecutionResultState y)
			{
				var xPriority = _priorities[x];
				var yPriority = _priorities[y];
				return xPriority.CompareTo(yPriority);
			}
		}

		void RunLine(Line line)
		{
			if (string.IsNullOrWhiteSpace(line.String))
			{
				return;
			}
			foreach (var variant in _variants)
			{
				var match = variant.Key.Match(line.String);
				if (match.Success)
				{
					try
					{
						line.ResultState = ExecutionResultState.Unknown;
						Context.LastResult = null;
						PerformRun(line, match, variant);
						line.ResultState = Context.LastResult ?? ExecutionResultState.Unknown;
					}
					catch
					{
						line.ResultState = ExecutionResultState.Exception;
					}
					return;
				}
			}
			XTrace.XTrace.Error("No handlers", line.String);

		}

		static void PerformRun(Line line, Match match, KeyValuePair<Regex, Action<string>> variant)
		{
			if (match.Groups.Count > 1)
			{
				// TODO eliminate SPECIAL SYMBOL!! 
				variant.Value(match.Groups.OfType<Group>().Skip(1).Select(x => x.Value).Join("|"));
			}
			else
			{
				var rest = variant.Key.Replace(line.String, "");
				variant.Value(rest);
			}
		}

		readonly Dictionary<Regex, Action<string>> _variants;

		const RegexOptions RO = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;

		public FaatAgent()
		{
			_variants = new Dictionary<Regex, Action<string>>
			{
				{new Regex(@"^using\s", RO), Using},
				{new Regex(@"^load\s", RO), Load},
				{new Regex(@"^is\s", RO), Is},
			};
		}

		/// <summary>
		/// Check last output
		/// </summary>
		/// <param name="expected"></param>
		void Is(string expected)
		{
			var last = Context.LastOutput;
			var lastFormatable =last as IFormattable;

			string dislay;
			if (lastFormatable !=null)
			{
				dislay = lastFormatable.ToString(null, CultureInfo.InvariantCulture);
			}
			else if (last != null)
			{
				dislay = last.ToString();
			}
			else
			{
				// TODO Magic Symbol
				dislay = "(null)";
			}

			if (string.Equals(expected, dislay, StringComparison.InvariantCultureIgnoreCase))
			{
				Context.LastResult = ExecutionResultState.Passed;
			}
			else
			{
				Context.LastResult = ExecutionResultState.Failed;
			}
		}

		/// <summary>
		/// Load library into app domain
		/// </summary>
		/// <param name="str"></param>
		void Load(string str)
		{
			XTrace.XTrace.Information("Load", str);
			if (File.Exists(str))
			{
				var asm = AppDomain.CurrentDomain.Load(File.ReadAllBytes(str));
				_assemblies.Add(asm);
				Context.LastResult = ExecutionResultState.Passed;
			}
			else
			{
				XTrace.XTrace.Error("Load - file not found", str);
				Context.LastResult = ExecutionResultState.Exception; // TODO stoping exception
			}
		}

		readonly List<Assembly> _assemblies = new List<Assembly>();
		readonly List<Type> _fixtures = new List<Type>();

		/// <summary>
		/// Import Fixture by class name
		/// </summary>
		/// <param name="str"></param>
		void Using(string str)
		{
			XTrace.XTrace.Information("Using", str);
			str = str.Replace(" ", "");

			var clazz = _assemblies.SelectMany(x =>
			{
				try
				{
					return x.GetTypes();
				}
				catch (Exception ex)
				{
					XTrace.XTrace.Exception(ex);
					return Enumerable.Empty<Type>();
				}
			}).Single(x => string.Equals(x.Name, str, StringComparison.InvariantCultureIgnoreCase));
			_fixtures.Add(clazz);

			foreach (var methodInfo in clazz.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
			{
				if (methodInfo.DeclaringType == typeof(object))
				{
					continue;
				}
				var info = methodInfo;
				_variants.Add(new Regex(RegexByMethodName(methodInfo.Name), RO), x =>
				{
					var parameters = info.GetParameters();
					var arguments = new object[parameters.Length];
					var argumentsString = x.Split('|');
					if (arguments.Length != argumentsString.Length)
					{
						throw new Exception("Argument count missmatch");
					}
					for (int i = 0; i < arguments.Length; i++)
					{
						var arg = argumentsString[i].ConvertTo(parameters[i].ParameterType);
						arguments[i] = arg;
					}
					var result = info.Invoke(info.IsStatic ? null : InstanceOf(info.DeclaringType), arguments);

					Context.LastOutput = result;

					if (info.ReturnType == typeof(void))
					{
						Context.LastResult = ExecutionResultState.Passed;
					}else if (info.ReturnType == typeof(bool))
					{
						Context.LastResult = (bool)result ? ExecutionResultState.Passed : ExecutionResultState.Failed;
					}
					else if (info.ReturnType == typeof(bool?))
					{
						switch ((bool?)result)
						{
							case true:
								Context.LastResult = ExecutionResultState.Passed;
								break;
							case false:
								Context.LastResult = ExecutionResultState.Failed;
								break;
							case null:
								Context.LastResult = ExecutionResultState.Unknown;
								break;
						}
					}
					else
					{
						Context.LastResult = ExecutionResultState.Passed;
						// TODO verify by additional syntax
					}

				});

				Context.LastResult = ExecutionResultState.Passed; // mark only when anything imported
			}
		}

		object InstanceOf(Type type)
		{
			return _fixtureInstanties[type, () => Activator.CreateInstance(type)];
		}

		readonly WeakValueCache<Type, object> _fixtureInstanties = new WeakValueCache<Type, object>();

		string RegexByMethodName(string methodName)
		{
			var rx = new StringBuilder();
			int paramIndex = 0;
			for (int i = 0; i < methodName.Length; i++)
			{
				var c = methodName[i];
				if(char.IsUpper(c))
				{
					rx.Append(@"\s*");
					rx.Append(c);
				}
				else if (c == '_')
				{
					rx.Append(string.Format(@"(?'{0}'.+)", "P" + paramIndex++));
				}
				else
				{
					rx.Append(c);
				}
			}
			return rx.ToString();
		}
	}
}
