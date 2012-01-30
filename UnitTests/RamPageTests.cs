using System.Collections.Generic;
using System;

using Faat.Storage;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
	[TestClass]
	public class RamPageTests : PageTests
	{
		protected override IStorage OpenStorage()
		{
			return null;
		}
	}
}