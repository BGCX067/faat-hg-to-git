using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Faat.Storage
{
	public static class Ext
	{
		public static IPage GetRecycleBin(this IModelProvider modelProvider)
		{
			if (modelProvider == null)
			{
				throw new ArgumentNullException("modelProvider");
			}
			return modelProvider.GetPage(Const.RecycleBinPage);
		}

		public static IPage GetRoot(this IModelProvider modelProvider)
		{
			if (modelProvider == null)
			{
				throw new ArgumentNullException("modelProvider");
			}
			return modelProvider.GetPage(Const.RootPage);
		}
	}
}
