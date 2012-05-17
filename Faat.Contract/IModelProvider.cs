using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Faat
{
	public interface IModelProvider
	{
		IPage GetPage(string pageId);
		void SetData(string identity, string data);
		string GetData(string identity);
	}
}
