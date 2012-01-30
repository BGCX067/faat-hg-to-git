using System;
using System.Collections.Generic;
using System.Text;

using Faat.Client.FaatServiceReference;
using Faat.Storage;
using Faat.Storage.Remote;

namespace Faat.UserInterface
{
	class PageBrowserViewModel : ObservableObject
	{
		string _identity = default(Guid).ToString("N");

		public string PageIdentity
		{
			get { return _identity; }
			set
			{
				_identity = value;
				OnPropertyChanged("PageIdentity");
			}
		}

		public string PageName
		{
			get
			{
				return Page.Name;
			}
			set
			{
				Page.Name = value;
				OnPropertyChanged("PageName");
			}
		}

		IPage Page
		{
			get
			{
				return _storage.GetPage(PageIdentity);
			}
		}

		readonly IStorage _storage;

		public PageBrowserViewModel()
		{
			_storage = new RemoteStorage(new FaatServiceClient());
		}

		void NavigateTo(string identity)
		{
			_identity = identity;
			OnPropertyChanged(null);
		}
	}
}
