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
							AspectHandleDisconnect(delegate
							{
								Storage = new RemoteStorage(true, ServerAddress);
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

		RemoteStorage _storage;

		public RemoteStorage Storage
		{
			get { return _storage; }
			private set
			{
				_storage = value;
				OnPropertyChanged("Storage");
			}
		}

		public bool CanConnect
		{
			get { return State == ConnectionState.Disconnected; }
		}

		void VerifyConnection()
		{
			AspectHandleDisconnect(delegate
			{
				lock (_sync)
				{
					var storage = Storage;
					var isCon = IsConnected = storage != null && storage.IsAlive;
					if (isCon)
					{
						storage.PropertyChanged -= StoragePropertyChanged;
						storage.PropertyChanged += StoragePropertyChanged;
					}
				}
			});
		}

		void AspectHandleDisconnect(Action act)
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

		void StoragePropertyChanged(object sender, PropertyChangedEventArgs e)
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