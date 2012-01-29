using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Faat.Contract;

namespace Faat.UserInterface
{
	class PageBrowserViewModel : ObservableObject
	{
		Guid _identity;

		public string PageIdentity
		{
			get { return _identity.ToString("N"); }
			set
			{
				_identity = new Guid(value);
			}
		}

		public string PageName
		{
			
		}

		IPage Page
		{
			get
			{
				return 
			}
		}

		public PageBrowserViewModel()
		{
			
		}
	}
}
