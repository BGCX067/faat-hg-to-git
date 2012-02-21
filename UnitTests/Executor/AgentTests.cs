using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Reflection;

using Faat.Agent;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using MyUtils;

namespace UnitTests.Executor
{
	[TestClass]
	public class AgentTests
	{
		public static string TempFlagFileName = Path.Combine(Path.GetTempPath(), "_ExecutorTest.tmp");

		readonly FaatAgent _sut = new FaatAgent();

		[TestMethod]
		public void Should_find_fixtures_and_invoke()
		{
			new XTrace.StructuredTextFileXTraceListener("faat_test", XTrace.RelativePath.TempFolder);
			File.Delete(TempFlagFileName);
			Assert.IsFalse(File.Exists(TempFlagFileName));

			_sut.RunPage(string.Format(@"
load {0}
using my fixture
hello world
", Assembly.GetExecutingAssembly().Location));
			Assert.IsTrue(File.Exists(TempFlagFileName));
		}
	}
}