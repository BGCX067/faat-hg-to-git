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
	public class PageAtFileStorageTests_NoReopen : PageAtFileStorageTests
	{
		protected override void ReopenStorage() {}
	}

	[TestClass]
	public class PageAtFileStorageTestsReopen : PageAtFileStorageTests {}

	[TestClass]
	public abstract class PageAtFileStorageTests : PageAtStorageTests
	{
		readonly DirectoryInfo _dir = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\storage" + Guid.NewGuid().ToString("N"));

		protected override IStorage OpenStorage()
		{
			return new FileSystemStorage(_dir);
		}

		[TestMethod]
		public void Should_write_change_asap()
		{
			Page1.Name = "x2";

			using (var fss = new FileSystemStorage(_dir))
			{
				Assert.AreEqual("x2", fss.GetPage(Page1.Identity).Name);
			}

			Page1.Content = "x3";
			using (var fss = new FileSystemStorage(_dir))
			{
				Assert.AreEqual("x3\r\n", fss.GetPage(Page1.Identity).Content);
				Assert.AreEqual("x2", fss.GetPage(Page1.Identity).Name);
			}

			Page1.Content = "x3\r\nx4";
			using (var fss = new FileSystemStorage(_dir))
			{
				Assert.AreEqual("x3\r\nx4\r\n", fss.GetPage(Page1.Identity).Content);
				Assert.AreEqual("x2", fss.GetPage(Page1.Identity).Name);
			}
		}
	}
}
