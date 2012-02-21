using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Faat.Storage
{
	public static class Ext
	{
		public static IPage GetRecycleBin(this IStorage storage)
		{
			if (storage == null)
			{
				throw new ArgumentNullException("storage");
			}
			return storage.GetPage(Const.RecycleBinPage);
		}

		public static IPage GetRoot(this IStorage storage)
		{
			if (storage == null)
			{
				throw new ArgumentNullException("storage");
			}
			return storage.GetPage(Const.RootPage);
		}
	}
}
