using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.Text;
using System.Threading;

using Faat.Client.FaatServiceReference;
using Faat.Model;

using MyUtils;
using MyUtils.UI;

namespace Faat.Storage.Remote
{
	public class RemoteStorage : DispatchableObservable, IStorage, IDisposable
	{
		[CallbackBehavior(UseSynchronizationContext = false)]
		class CallBack : IFaatServiceCallback
		{
			readonly RemoteStorage _parent;

			public CallBack(RemoteStorage parent)
			{
				_parent = parent;
			}

			public void OnDataChanged(string identity)
			{
				ThreadPool.QueueUserWorkItem(x =>
				{
					try
					{
						var localIdentity = (string)x;
						_parent.ReloadPage(localIdentity);
						_parent.OnDataChanged(localIdentity);
					}
					catch (Exception ex)
					{
						XTrace.XTrace.Exception(ex);
					}
				}, identity);
			}

			public string PingClient(string message)
			{
				return message;
			}

		}

		void ReloadPage(string localIdentity)
		{
			var page = _pages[localIdentity] as StringPage;
			if (page != null)
			{
				page.Load();
			}
		}

		readonly bool _notifiable;
		readonly string _address;
		readonly WeakValueCache<string, IPage> _pages = new WeakValueCache<string, IPage>();
		FaatServiceClient _client;
		InstanceContext _callback;

		public RemoteStorage(bool notifiable = true, string address = null)
		{
			_notifiable = notifiable;
			_address = address;
			Connect();
		}

		public string Address
		{
			get { return _address ?? _client.Endpoint.Address.ToString(); }
		}

		public IPage GetPage(string identity)
		{
			lock (_pages)
			{
				return _pages[identity, () => new StringPage(this, identity)];
			}
		}

		public bool IsAlive
		{
			[DebuggerStepThrough]
			get
			{
				try
				{
					var msg = Guid.NewGuid().ToString();
					return _client.PingServer(msg) == msg;
				}
				catch (Exception ex)
				{
					XTrace.XTrace.Exception(ex);
					return false;
				}
			}
		}

		public string GetData(string identity)
		{
			string result = null;
			AutoReconnect(delegate
			{
				result = _client.GetData(identity);
			});
			return result;
		}

		public void SetData(string identity, string data)
		{
			AutoReconnect(delegate
			{
				_client.SetData(identity, data);
			});
		}

		void AutoReconnect(Action act)
		{
			const int count = 3;
			for (int i = 0; i < count; i++)
			{
				try
				{
					if (!_connected)
					{
						Connect();
					}
					act();
					break;
				}
				catch (CommunicationException ex)
				{
					IsConnected = false;
					XTrace.XTrace.Exception(ex, "AutoReconnect", "Try {0}", i);
					if (i < count)
					{
						if (!_connected)
						{
							Thread.Sleep(2000);
						}
					}
					else
					{
						throw;
					}
				}
			}
		}

		bool _connected;

		public bool IsConnected
		{
			get
			{
				return _connected;
			}
			private set
			{
				if (_connected != value)
				{
					_connected = value;
					OnPropertyChanged("IsConnected");
				}
			}
		}

		void Connect()
		{
			IsConnected = false;
			var cli = _client as IDisposable;
			if (cli != null)
			{
				try
				{
					cli.Dispose();
				}catch {}
			}
			_client = null;

			_callback = new InstanceContext(new CallBack(this));
			_client = _address == null ? new FaatServiceClient(_callback) : new FaatServiceClient(_callback, "*", _address);

			var msg = Guid.NewGuid().ToString();
			if (_client.PingServer(msg) != msg)
			{
				throw new Exception("Ping Failed");
			}

			if (_notifiable)
			{
				_client.Subscribe();
			}

			IsConnected = true;
		}

		public event EventHandler<DataChangedEventArgs> DataChanged;

		protected void OnDataChanged(string identity)
		{
			OnDataChanged(new DataChangedEventArgs { Identity = identity });
		}

		protected void OnDataChanged(DataChangedEventArgs e)
		{
			var handler = DataChanged;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		public void Dispose()
		{
			DataChanged = null;
			var c = _client as IDisposable;
			if (c != null)
			{
				c.Dispose();
			}
		}
	}
}
