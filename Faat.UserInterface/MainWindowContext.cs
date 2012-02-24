using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using Faat.Storage;

using MySpring;

using MyUtils;


namespace Faat.UserInterface
{
	public enum ConnectionState
	{
		Unknown,
		Connecting,
		Connected,
		Disconnected,
	}

	public static partial class Ext
	{
		public static T MostInner<T>(this T item, Func<T, T> innerAccessor) where T : class
		{
			while (true)
			{
				var inner = innerAccessor(item);
				if (inner == null)
				{
					return item;
				}
				item = inner;
			}
		}
	}

	class MainWindowContext : ObservableObject
	{
		readonly SpringContainer SpringContainer = SpringContainer.Root;

		public MainWindowContext()
		{
			_connection = new ConnectionViewModel();
			_connection.PropertyChanged += ConnectionPropertyChanged;
		}

		void ConnectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.Is("IsConnected"))
			{
				if (Connection.IsConnected)
				{
					_currentPage = PageViewModel.Create(RootPage, null);
					PageViewModel.Subscribe(SpringContainer.Get<IStorage>());
				}
				OnPropertyChanged(null);
			}
		}

		readonly ConnectionViewModel _connection;
		public ConnectionViewModel Connection
		{
			get { return _connection; }
		}

		PageViewModel _currentPage;

		public PageViewModel CurrentPage
		{
			get { return _currentPage; }
			set
			{
				if (_currentPage != value)
				{
					_currentPage = value;
					OnPropertyChanged("CurrentPage");
				}
			}
		}

		public PageViewModel[] RootPages
		{
			get
			{
				if (!Connection.IsConnected)
				{
					return null;
				}
				return new[]
				{
					PageViewModel.Create(RootPage, null),
					PageViewModel.Create(RecycleBinPage, null),
					PageViewModel.Create(StatisticsPage, null),
				};
			}
		}

		public IPage RootPage
		{
			get
			{
				if (!Connection.IsConnected)
				{
					return null;
				}
				return SpringContainer.Get<IStorage>().GetPage(Const.RootPage);
			}
		}

		public IPage StatisticsPage
		{
			get
			{
				if (!Connection.IsConnected)
				{
					return null;
				}
				return SpringContainer.Get<IStorage>().GetPage(Const.StatisticsPage);
			}
		}

		public IPage RecycleBinPage
		{
			get
			{
				if (!Connection.IsConnected)
				{
					return null;
				}
				var page = SpringContainer.Get<IStorage>().GetPage(Const.RecycleBinPage);
				page.Name = "Recycle Bin";
				return page;
			}
		}

	}
}
