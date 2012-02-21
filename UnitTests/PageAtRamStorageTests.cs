using System.Collections.Generic;
using System;

using Faat.Storage;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
	[TestClass]
	public class PageAtRamStorageTests : PageAtStorageTests
	{
		protected override IStorage OpenStorage()
		{
			return null;
		}
	}
}