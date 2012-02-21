using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Faat.Parser;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Parser
{
	[TestClass]
	public class TokenizeByLine
	{
		readonly PageParser _sut = new PageParser();

		[TestMethod]
		public void Should_do_something()
		{
			var sample = @"line1
line2

line4";
			var result = _sut.TokenizeByLine(sample).ToArray();

			Assert.AreEqual(4, result.Length);
			Assert.AreEqual("line1", result[0].String);
			Assert.AreEqual("line2", result[1].String);
			Assert.AreEqual("", result[2].String);
			Assert.AreEqual("line4", result[3].String);
		}
	}
}
