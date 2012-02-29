using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;

using Faat.Storage;
using Faat.Storage.Remote;
using Faat.UserInterface.Properties;

using MySpring;

using MyUtils;
using MyUtils.UI;

namespace Faat.UserInterface
{
	class ConnectionViewModel : DispatchableObservable
	{
		readonly SpringContainer SpringContainer = SpringContainer.Root;

		ConnectionState _state = ConnectionState.Disconnected;

		public ConnectionState State
		{
			get { return _state; }
			private set
			{
				if (_state != value)
				{
					_state = value;
					OnPropertyChanged(null);
					var d = DefaultDispatcher;
					if (d != null)
					{
						d.BeginInvoke((Action) CommandManager.InvalidateRequerySuggested);
					}
				}
			}
		}

		public bool IsNotConnected
		{
			get { return _state != ConnectionState.Connected; }
		}

		public bool IsConnecting
		{
			get { return _state == ConnectionState.Connecting; }
		}

		public bool IsNotConnecting
		{
			get
			{
				return _state != ConnectionState.Connecting;
			}
		}

		public bool IsConnected
		{
			get
			{
				return _state == ConnectionState.Connected;
			}
			set { State = value ? ConnectionState.Connected : ConnectionState.Disconnected; }
		}

		readonly object _sync = new object();

		public void Connect()
		{
			ThreadPool.QueueUserWorkItem(delegate
			{
				try
				{
					lock (_sync)
					{
						if (!IsConnected)
						{
							State = ConnectionState.Connecting;
							HandleDisconnect(delegate
							{
								SpringContainer.Add<IStorage>(new RemoteStorage(true, ServerAddress));
							});
							VerifyConnection();
						}
					}
				}
				catch (Exception ex)
				{
					XTrace.XTrace.Exception(ex);
				}
			});
		}

		public bool CanConnect
		{
			get { return State == ConnectionState.Disconnected; }
		}

		void VerifyConnection()
		{
			HandleDisconnect(delegate
			{
				lock (_sync)
				{
					var storage = SpringContainer.Get<IStorage>() as RemoteStorage;
					IsConnected = storage != null && storage.IsAlive;
					if (IsConnected)
					{
						storage.PropertyChanged -= storage_PropertyChanged;
						storage.PropertyChanged += storage_PropertyChanged;
					}
				}
			});
		}

		void HandleDisconnect(Action act)
		{
			lock (_sync)
			{
				try
				{
					act();
					ConnectionError = null;
				}
				catch (Exception ex)
				{
					XTrace.XTrace.Exception(ex);
					IsConnected = false;
					ConnectionError = ex.MostInner(x => x.InnerException).Message;
				}
			}
		}

		void storage_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.Is("IsConnected"))
			{
				ThreadPool.QueueUserWorkItem(delegate { VerifyConnection(); });
			}
		}

		public string ServerAddress
		{
			get { return Settings.Default.LastServerAddress.IsNullOrWhitespaces() ? null : Settings.Default.LastServerAddress; } // null allows app config usage
			set
			{
				if (Settings.Default.LastServerAddress != value)
				{
					Settings.Default.LastServerAddress = value;
					Settings.Default.Save();
					OnPropertyChanged("ServerAddress");
				}
			}
		}

		string _connectionError;
		public string ConnectionError
		{
			get { return _connectionError; }
			set
			{
				if (_connectionError != value)
				{
					_connectionError = value;
					OnPropertyChanged("ConnectionError");
				}
			}
		}
	}
}