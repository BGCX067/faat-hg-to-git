using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xaml;
using System.Xml;

using Faat.Parser;
using Faat.Parser.Ast;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Parser
{
	[TestClass]
	public class BasicTokensParsing
	{
		readonly PageParser _sut = new PageParser();

		[TestMethod]
		public void Should_10_parse_simple_block()
		{
			var result = _sut.Tokenize(@"

my scenario 1:
push asd
push asdasd

");
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.PageTokens);
			Assert.IsNotNull(result.PageTokens.Lines);
			Assert.AreEqual(1, result.PageTokens.Lines.Count());
			var block = (Block)result.PageTokens.Lines[0];
			Assert.IsNotNull(block);

			Assert.AreEqual(2, block.Lines.Count());
			Assert.AreEqual("push asd", block.Lines[0].String);
			Assert.AreEqual("push asdasd", block.Lines[1].String);
		}

		[TestMethod]
		public void Should_11_parse_two_blocks_on_page()
		{
			var result = _sut.Tokenize(@"

my scenario 1:
push asd
push asdasd

my scenario 2:
push asd2
push asdasd2


");
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.PageTokens);
			Assert.IsNotNull(result.PageTokens.Lines);
			Assert.AreEqual(2, result.PageTokens.Lines.Count());
			var block = (Block)result.PageTokens.Lines[0];
			Assert.IsNotNull(block);

			Assert.AreEqual(2, block.Lines.Count());
			Assert.AreEqual("push asd", block.Lines[0].String);
			Assert.AreEqual("push asdasd", block.Lines[1].String);

			block = (Block)result.PageTokens.Lines[1];
			Assert.IsNotNull(block);

			Assert.AreEqual(2, block.Lines.Count());
			Assert.AreEqual("push asd2", block.Lines[0].String);
			Assert.AreEqual("push asdasd2", block.Lines[1].String);

		}

		[TestMethod]
		public void Should_12_parse_two_blocks_on_page_inline()
		{
			var result = _sut.Tokenize(@"

my scenario 1:
push asd
push asdasd
my scenario 2:
	push asd2
	push asdasd2

");
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.PageTokens);
			Assert.IsNotNull(result.PageTokens.Lines);
			Assert.AreEqual(1, result.PageTokens.Lines.Count());
			var block = (Block)result.PageTokens.Lines[0];
			Assert.IsNotNull(block);

			Assert.AreEqual(3, block.Lines.Count());
			Assert.AreEqual("push asd", block.Lines[0].String);
			Assert.AreEqual("push asdasd", block.Lines[1].String);

			block = (Block)block.Lines[2];
			Assert.IsNotNull(block);

			Assert.AreEqual(2, block.Lines.Count());
			Assert.AreEqual("push asd2", block.Lines[0].String);
			Assert.AreEqual("push asdasd2", block.Lines[1].String);

		}


		[TestMethod]
		public void Should_20_parse_tabulated_blocks()
		{
			var result = _sut.Tokenize(@"

my scenario 1:
	push asd
	push asdasd

my scenario 2:
push asd2
push asdasd2


");
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.PageTokens);
			Assert.IsNotNull(result.PageTokens.Lines);
			Assert.AreEqual(2, result.PageTokens.Lines.Count());
			var block = (Block)result.PageTokens.Lines[0];
			Assert.IsNotNull(block);

			Assert.AreEqual(2, block.Lines.Count());
			Assert.AreEqual("push asd", block.Lines[0].String);
			Assert.AreEqual("push asdasd", block.Lines[1].String);

			block = (Block)result.PageTokens.Lines[1];
			Assert.IsNotNull(block);

			Assert.AreEqual(2, block.Lines.Count());
			Assert.AreEqual("push asd2", block.Lines[0].String);
			Assert.AreEqual("push asdasd2", block.Lines[1].String);

		}

		[TestMethod]
		public void Should_22_parse_tabulated_blocks_inline()
		{
			var result = _sut.Tokenize(@"

my scenario 1:
	push asd
	push asdasd
my scenario 2:
push asd2
push asdasd2


");
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.PageTokens);
			Assert.IsNotNull(result.PageTokens.Lines);
			Assert.AreEqual(2, result.PageTokens.Lines.Count());
			var block = (Block)result.PageTokens.Lines[0];
			Assert.IsNotNull(block);

			Assert.AreEqual(2, block.Lines.Count());
			Assert.AreEqual("push asd", block.Lines[0].String);
			Assert.AreEqual("push asdasd", block.Lines[1].String);

			block = (Block)result.PageTokens.Lines[1];
			Assert.IsNotNull(block);

			Assert.AreEqual(2, block.Lines.Count());
			Assert.AreEqual("push asd2", block.Lines[0].String);
			Assert.AreEqual("push asdasd2", block.Lines[1].String);

		}

		[TestMethod]
		public void Should_25_parse_tabulation()
		{
			ExpectedTokinization(@"

a
	aa
	ab
b
	ba
		baa
		bab
	bb
c
d
	da
	db
e
f

", @"<Block Name=""Page"" xmlns=""Faat1"">
  <Block Name=""a"">
    <TextLine TextLine=""aa"" />
    <TextLine TextLine=""ab"" />
  </Block>
  <Block Name=""b"">
    <Block Name=""ba"">
      <TextLine TextLine=""baa"" />
      <TextLine TextLine=""bab"" />
    </Block>
    <TextLine TextLine=""bb"" />
  </Block>
  <TextLine TextLine=""c"" />
  <Block Name=""d"">
    <TextLine TextLine=""da"" />
    <TextLine TextLine=""db"" />
  </Block>
  <TextLine TextLine=""e"" />
  <TextLine TextLine=""f"" />
</Block>");


		}

		[TestMethod]
		public void Should_25_parse_tabulationwith_columns()
		{
			ExpectedTokinization(@"

`tab-scoped
Share:
	MyScenario1:
		line
		line
		line
	MyScenario2:
		line
		line
		line
		MySubScenario2:
			line
			line
			line

`nontabulated
MyScenarioAfter:
line
line
line

", @"<Block Name=""Page"" xmlns=""Faat1"">
  <Block Name=""Share"">
    <Block Name=""MyScenario1"">
      <TextLine TextLine=""line"" />
      <TextLine TextLine=""line"" />
      <TextLine TextLine=""line"" />
    </Block>
    <Block Name=""MyScenario2"">
      <TextLine TextLine=""line"" />
      <TextLine TextLine=""line"" />
      <TextLine TextLine=""line"" />
      <Block Name=""MySubScenario2"">
        <TextLine TextLine=""line"" />
        <TextLine TextLine=""line"" />
        <TextLine TextLine=""line"" />
      </Block>
    </Block>
  </Block>
  <Block Name=""MyScenarioAfter"">
    <TextLine TextLine=""line"" />
    <TextLine TextLine=""line"" />
    <TextLine TextLine=""line"" />
  </Block>
</Block>");


		}

		void ExpectedTokinization(string source, string expectedXaml)
		{
			var result = _sut.Tokenize(source);

			var actualXaml = XamlServices.Save(result.PageTokens);

			Assert.AreEqual(expectedXaml, actualXaml);
		}

		string XamlWrite(object objectGraph)
		{
			var sw = new StringWriter(CultureInfo.CurrentCulture);
			var settings = new XmlWriterSettings
			{
				Indent = true,
				IndentChars = "\t",
				OmitXmlDeclaration = true,
			};
			using (var writer = XmlWriter.Create(sw, settings))
			{
				XamlServices.Save(writer, objectGraph);
			}
			return sw.ToString();
		}

		[TestMethod]
		public void Should_30_parse_page_wide_blocks()
		{
			ExpectedTokinization(@"


Note:
Some common actions can be shared - declared for future usage

Сложить 1 2 = 3
Сложить 1 3 = 4

Note:
Аналог сценариев FitNesse

Share:

Сложить А Б:
нажать А
нажать +
нажать Б
экран

Умножить A B:
нажать А
нажать *
нажать Б
экран

Note:
Последнее возвращенное значение является одновременно возвращенным из зашаренного сценария


", @"<Block Name=""Page"" xmlns=""Faat1"">
  <Block Name=""Note"">
    <TextLine TextLine=""Some common actions can be shared - declared for future usage"" />
  </Block>
  <TextLine TextLine=""Сложить 1 2 = 3"" />
  <TextLine TextLine=""Сложить 1 3 = 4"" />
  <Block Name=""Note"">
    <TextLine TextLine=""Аналог сценариев FitNesse"" />
  </Block>
  <Block Name=""Share"">
    <Block Name=""Сложить А Б"">
      <TextLine TextLine=""нажать А"" />
      <TextLine TextLine=""нажать +"" />
      <TextLine TextLine=""нажать Б"" />
      <TextLine TextLine=""экран"" />
    </Block>
    <Block Name=""Умножить A B"">
      <TextLine TextLine=""нажать А"" />
      <TextLine TextLine=""нажать *"" />
      <TextLine TextLine=""нажать Б"" />
      <TextLine TextLine=""экран"" />
    </Block>
    <Block Name=""Note"">
      <TextLine TextLine=""Последнее возвращенное значение является одновременно возвращенным из зашаренного сценария"" />
    </Block>
  </Block>
</Block>");
		}
	}
}
