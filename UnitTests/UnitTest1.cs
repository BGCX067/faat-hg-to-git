using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using Faat.Contract;
using Faat.Core;
using Faat.Host;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
	[TestClass]
	public class FilePageTests : PageTests
	{
		string _fileName;

		protected override IPage CreatePage()
		{
			return FilePage.Create(Directory.GetCurrentDirectory(), GetNewPageName(), out _fileName);
		}

		[TestMethod]
		public void Should_write_change_asap()
		{
			_sut.Name = "x2";
			using (var page = new FilePage(_fileName))
			{
				Assert.AreEqual("x2", page.Name);
			}

			_sut.Content = "x3";
			using (var page = new FilePage(_fileName))
			{
				Assert.AreEqual("x3\r\n", page.Content);
				Assert.AreEqual("x2", page.Name);
			}

			_sut.Content = "x3\r\nx4";
			using (var page = new FilePage(_fileName))
			{
				Assert.AreEqual("x3\r\nx4\r\n", page.Content);
				Assert.AreEqual("x2", page.Name);
			}
		}
	}

	[TestClass]
	public class RamPageTests : PageTests
	{
		protected override IPage CreatePage()
		{
			return new RamPage {Name = GetNewPageName()};
		}
	}

	[TestClass]
	public abstract class PageTests
	{
		static int _num;

		protected string GetNewPageName()
		{
			return "New Page " + ++_num;
		}

		protected IPage _sut;

		protected abstract IPage CreatePage();

		[TestInitialize]
		public void Init()
		{
			_sut = CreatePage();
		}

		[TestCleanup]
		public void Clean()
		{
			var page = _sut as IDisposable;
			if (page != null)
			{
				page.Dispose();
			}
		}

		[TestMethod]
		public void Should_have_page_with_properties()
		{
			Assert.IsNotNull(_sut);
			Assert.IsTrue(!string.IsNullOrEmpty(_sut.Name));
		}

		[TestMethod]
		public void Should_have_page_with_settable_properties()
		{
			_sut.Name = "test";
			Assert.AreEqual("test", _sut.Name);
			_sut.Name = "test2";
			Assert.AreEqual("test2", _sut.Name);

			_sut.Content = "testC";
			Assert.AreEqual("test2", _sut.Name);
			Assert.AreEqual("testC", _sut.Content);

			_sut.Content = "testC2";
			Assert.AreEqual("test2", _sut.Name);
			Assert.AreEqual("testC2", _sut.Content);

		}

		[TestMethod]
		public void Should_not_depend_on_setting_order()
		{
			_sut.Content = "testC2";
			_sut.Name = "test2";

			Assert.AreEqual("testC2", _sut.Content);
			Assert.AreEqual("test2", _sut.Name);
		}

		[TestMethod]
		public void Should_create_subpages()
		{
			var sp1 = CreatePage();
			var sp2 = CreatePage();
			_sut.Children.Add(sp1);
			_sut.Children.Add(sp2);

			Assert.IsTrue(_sut.Children.Contains(sp1));
			Assert.IsTrue(_sut.Children.Contains(sp2));
			Assert.IsTrue(sp1.Parents.Contains(_sut));
			Assert.IsTrue(sp2.Parents.Contains(_sut));
		}

		[TestMethod]
		public void Should_allow_multiple_parents()
		{
			var sp1 = CreatePage();
			var sp2 = CreatePage();
			sp1.Children.Add(_sut);
			sp2.Children.Add(_sut);

			Assert.IsTrue(_sut.Parents.Contains(sp1));
			Assert.IsTrue(_sut.Parents.Contains(sp2));
			Assert.IsTrue(sp1.Children.Contains(_sut));
			Assert.IsTrue(sp2.Children.Contains(_sut));
		}

		[TestMethod]
		public void Should_create_linked_pages()
		{
			
			Assert.Inconclusive();
		}


	}

	[TestClass]
	public class PageFormatTests
	{

		[TestMethod]
		public void Should_do_something()
		{
			
			Assert.Inconclusive();
		}

		[TestMethod]
		public void Should_inline_pages()
		{

			Assert.Inconclusive();
		}

	}
}
