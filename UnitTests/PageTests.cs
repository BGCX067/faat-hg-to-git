using System.Collections.Generic;
using System;

using Faat;
using Faat.Model;
using Faat.Storage;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
	[TestClass]
	public abstract class PageTests
	{
		static int _num;

		protected string GetNewPageName()
		{
			return "New Page " + ++_num;
		}

		protected IPage _page;
		protected IStorage _storage;

		protected abstract IStorage OpenStorage();

		[TestInitialize]
		public void Init()
		{
			_storage = OpenStorage();
			_page = new StringPage(_storage);
		}

		[TestCleanup]
		public void Clean()
		{
			var page = _page as IDisposable;
			if (page != null)
			{
				page.Dispose();
			}
		}

		[TestMethod]
		public void Should_have_page_with_properties()
		{
			Assert.IsNotNull(_page);
			Assert.IsTrue(!string.IsNullOrEmpty(_page.Name));
		}

		[TestMethod]
		public void Should_have_page_with_settable_properties()
		{
			_page.Name = "test";
			Assert.AreEqual("test", _page.Name);
			_page.Name = "test2";
			Assert.AreEqual("test2", _page.Name);

			_page.Content = "testC";
			Assert.AreEqual("test2", _page.Name);
			Assert.AreEqual("testC", _page.Content);

			_page.Content = "testC2";
			Assert.AreEqual("test2", _page.Name);
			Assert.AreEqual("testC2", _page.Content);

		}

		[TestMethod]
		public void Should_not_depend_on_setting_order()
		{
			_page.Content = "testC2";
			_page.Name = "test2";

			Assert.AreEqual("testC2", _page.Content);
			Assert.AreEqual("test2", _page.Name);
		}

		[TestMethod]
		public void Should_create_subpages()
		{
			var sp1 = CreatePage();
			var sp2 = CreatePage();
			_page.Children.Add(sp1);
			_page.Children.Add(sp2);

			Assert.IsTrue(_page.Children.Contains(sp1));
			Assert.IsTrue(_page.Children.Contains(sp2));
			Assert.IsTrue(sp1.Parents.Contains(_page));
			Assert.IsTrue(sp2.Parents.Contains(_page));
		}

		protected IPage CreatePage()
		{
			return new StringPage(_storage);
		}

		[TestMethod]
		public void Should_allow_multiple_parents()
		{
			var sp1 = CreatePage();
			var sp2 = CreatePage();
			sp1.Children.Add(_page);
			sp2.Children.Add(_page);

			Assert.IsTrue(_page.Parents.Contains(sp1));
			Assert.IsTrue(_page.Parents.Contains(sp2));
			Assert.IsTrue(sp1.Children.Contains(_page));
			Assert.IsTrue(sp2.Children.Contains(_page));
		}

		[TestMethod]
		public void Should_create_linked_pages()
		{
			
			Assert.Inconclusive();
		}


	}
}