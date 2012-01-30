using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using Faat.Storage;

namespace Faat.Service
{
	[ServiceBehavior(InstanceContextMode=InstanceContextMode.Single)]
	public class FaatService : IFaatService
	{
		readonly IStorage _storage;

		// DEMO
		public FaatService()
		{
			
		}

		public FaatService(IStorage storage)
		{
			_storage = storage;
		}

		public string GetPage(string identity)
		{
			return _storage.GetPageData(identity);
		}

		public void SetPage(string identity, string pageData)
		{
			_storage.SetPageData(identity, pageData);
		}
	}
}
