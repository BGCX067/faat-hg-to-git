using System.Collections.Generic;
using System;
using System.Linq;

using Faat;
using Faat.Model;
using Faat.Storage;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
	[TestClass]
	public abstract class PageAtStorageTests
	{
		static int _num;

		protected string GetNewPageName()
		{
			return "New Page " + ++_num;
		}

		protected IStorage _storage;

		protected abstract IStorage OpenStorage();

		protected virtual void ReopenStorage()
		{
			var disp = _storage as IDisposable;
			if (disp != null)
			{
				disp.Dispose();
			}
			var newStorage = OpenStorage();
			Assert.AreNotSame(_storage, newStorage);
			_storage = newStorage;

			for (int i = 0; i < _pages.Count; i++)
			{
				_pages[i] = _storage.GetPage(_pages[i].Identity);
			}
		}

		readonly List<IPage> _pages = new List<IPage>();

		protected IPage Page0 { get { return Page(0); } }
		protected IPage Page1 { get { return Page(1); } }
		protected IPage Page2 { get { return Page(2); } }
		protected IPage Page3 { get { return Page(3); } }

		protected IPage Page(int i)
		{
			while (_pages.Count < i + 1)
			{
				var p = CreatePage();
				p.Name = "AutoPage " + _pages.Count;
				_pages.Add(p);
			}
			return _pages[i];
		}

		IPage CreatePage()
		{
			return new StringPage(_storage);
		}

		[TestInitialize]
		public void Init()
		{
			_storage = OpenStorage();
		}

		[TestCleanup]
		public void Clean()
		{
			foreach (var page in _pages)
			{
				var paged = page as IDisposable;
				if (paged != null)
				{
					paged.Dispose();
				}
			}
		}

		[TestMethod]
		public void Should_have_page_with_properties()
		{
			Assert.IsNotNull(Page1);
			Assert.IsTrue(!string.IsNullOrEmpty(Page1.Name));
		}

		[TestMethod]
		public void Should_have_page_with_settable_properties()
		{
			Page1.Name = "test";
			Assert.AreEqual("test", Page1.Name);
			Page1.Name = "test2";
			Assert.AreEqual("test2", Page1.Name);

			Page1.Content = "testC";
			Assert.AreEqual("test2", Page1.Name);
			Assert.AreEqual("testC", Page1.Content);

			Page1.Content = "testC2";
			Assert.AreEqual("test2", Page1.Name);
			Assert.AreEqual("testC2", Page1.Content);

		}

		[TestMethod]
		public void Should_not_depend_on_setting_order()
		{
			Page1.Content = "testC2\r\n";
			Page1.Name = "test2";

			ReopenStorage();

			Assert.AreEqual("testC2\r\n", Page1.Content);
			Assert.AreEqual("test2", Page1.Name);
		}

		[TestMethod]
		public void Should_create_subpages()
		{
			Page1.Children.Add(Page2);
			Page1.Children.Add(Page3);

			ReopenStorage();

			Assert.IsTrue(Page1.Children.Contains(Page2));
			Assert.IsTrue(Page1.Children.Contains(Page3));
			Assert.IsTrue(Page2.Parents.Contains(Page1));
			Assert.IsTrue(Page3.Parents.Contains(Page1));
		}

		[TestMethod]
		public void Should_allow_multiple_parents()
		{
			Page2.Children.Add(Page1);
			Page3.Children.Add(Page1);

			ReopenStorage();

			Assert.IsTrue(Page1.Parents.Contains(Page2));
			Assert.IsTrue(Page1.Parents.Contains(Page3));
			Assert.IsTrue(Page2.Children.Contains(Page1));
			Assert.IsTrue(Page3.Children.Contains(Page1));
		}

		[TestMethod]
		public void Should_create_linked_pages()
		{
			Page1.Links.Add(Page2);

			ReopenStorage();

			Assert.AreEqual(Page2, Page1.Links.Single());
			Assert.AreEqual(Page1, Page2.Links.Single());
		}


		[TestMethod]
		public void Should_return_page_by_unknown_id()
		{
			var page1 = _storage.GetPage(Guid.NewGuid().ToString("N"));
			Assert.AreEqual("New Page", page1.Name);

			var page2 = _storage.GetPage(Guid.NewGuid().ToString("N"));
			Assert.AreEqual("New Page", page1.Name);

			var page3 = _storage.GetPage(page2.Identity);
			Assert.AreSame(page2, page3);
		}

		[TestMethod]
		public void Should_write_change_asap()
		{
			Page1.Name = "x2";

			ReopenStorage();

			Assert.AreEqual("x2", Page1.Name);

			Page1.Content = "x3";

			ReopenStorage();

			Assert.AreEqual("x3\r\n", Page1.Content);
			Assert.AreEqual("x2", Page1.Name);

			Page1.Content = "x3\r\nx4";
			ReopenStorage();

			Assert.AreEqual("x3\r\nx4\r\n", Page1.Content);
			Assert.AreEqual("x2", Page1.Name);
		}


	}
}