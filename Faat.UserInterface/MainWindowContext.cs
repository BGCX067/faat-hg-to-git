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
		#region Composition

		readonly ConnectionViewModel _connection;

		public ConnectionViewModel Connection
		{
			get { return _connection; }
		}

		#endregion

		#region Services

		SpringContainer _modelContainer;

		PageViewModelCache PageViewModelCache
		{
			get { return _modelContainer.Get<PageViewModelCache>(); }
		}

		IStorage Storage
		{
			get { return _modelContainer.Get<IStorage>(); }
		}

		IModelProvider ModelProvider
		{
			get { return _modelContainer.Get<IModelProvider>(); }
		}

		#endregion

		public MainWindowContext()
		{
			_connection = new ConnectionViewModel();
			_connection.PropertyChanged += ConnectionPropertyChanged;
		}

		bool _isConnectedLast;

		void ConnectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.Is("IsConnected"))
			{
				var now = Connection.IsConnected;
				if (!_isConnectedLast && now)
				{
					// Connected - construct model container
					_modelContainer = new SpringContainer();
					_modelContainer.Add<IStorage>(Connection.Storage);
					_modelContainer.Add<PageViewModelCache, PageViewModelCache>();
					PageViewModel.Subscribe(Storage);
					_currentPage = PageViewModelCache.Get(RootPage, null);
					OnPropertyChanged(null);
				}
				else if (_isConnectedLast && !now)
				{
					// Disconnected
					_modelContainer = null;
					OnPropertyChanged(null);
				}
				_isConnectedLast = now;
			}
		}

		#region Pages

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
					PageViewModelCache.Get(RootPage, null),
					PageViewModelCache.Get(RecycleBinPage, null),
					PageViewModelCache.Get(StatisticsPage, null),
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
				return ModelProvider.GetPage(Const.RootPage);
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
				return ModelProvider.GetPage(Const.StatisticsPage);
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
				var page = ModelProvider.GetPage(Const.RecycleBinPage);
				page.Name = "Recycle Bin";
				return page;
			}
		}

		#endregion



	}
}
