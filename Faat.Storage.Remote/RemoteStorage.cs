using System;
using System.Collections.Generic;
using System.Text;

using Faat.Client.FaatServiceReference;
using Faat.Model;

using MyUtils;

namespace Faat.Storage.Remote
{
	public class RemoteStorage : IStorage
	{
		readonly FaatServiceClient _client;

		public RemoteStorage(FaatServiceClient client)
		{
			_client = client;
		}

		readonly WeakValueCache<string, IPage> _pages = new WeakValueCache<string, IPage>();

		public IPage GetPage(string identity)
		{
			lock (_pages)
			{
				return _pages[identity, () => new StringPage(this, _client.GetPage(identity))];
			}
		}

		public string GetPageData(string identity)
		{
			return _client.GetPage(identity);
		}

		public void SetPageData(string identity, string pageData)
		{
			_client.SetPage(identity, pageData);
		}
	}
}
