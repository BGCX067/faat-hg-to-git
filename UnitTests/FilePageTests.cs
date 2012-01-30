using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using Faat.Storage;
using Faat.Storage.FileSystem;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
	[TestClass]
	public class FilePageTests : PageTests
	{
		DirectoryInfo _dir;

		protected override IStorage OpenStorage()
		{
			return new FileSystemStorage(_dir = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\storage"+Guid.NewGuid().ToString("N")));
		}

		[TestMethod]
		public void Should_write_change_asap()
		{
			_page.Name = "x2";

			using (var fss = new FileSystemStorage(_dir))
			{
				Assert.AreEqual("x2", fss.GetPage(_page.Identity).Name);
			}

			_page.Content = "x3";
			using (var fss = new FileSystemStorage(_dir))
			{
				Assert.AreEqual("x3\r\n", fss.GetPage(_page.Identity).Content);
				Assert.AreEqual("x2", fss.GetPage(_page.Identity).Name);
			}

			_page.Content = "x3\r\nx4";
			using (var fss = new FileSystemStorage(_dir))
			{
				Assert.AreEqual("x3\r\nx4\r\n", fss.GetPage(_page.Identity).Content);
				Assert.AreEqual("x2", fss.GetPage(_page.Identity).Name);
			}
		}
	}
}
