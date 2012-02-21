using System.Diagnostics;
using System.Linq;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;

using Faat.Model;
using Faat.Storage;

using MySpring;

using XTrace;
using System.Text;

namespace Faat.ControllerService
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
	public class FaatService : IFaatService
	{
		readonly IStorage _storage;
		readonly IFaatStatisticsWriter _statistics;

		// DEMO (non-working service for WCF default host)
		public FaatService()
		{
			
		}

		[DefaultConstructor]
		public FaatService(IStorage storage, IFaatStatisticsWriter statistics)
		{
			_storage = storage;
			_statistics = statistics;
			_storage.DataChanged += StorageDataChanged;

			_systemPages.Add(Const.StatisticsPage, () =>
			{
				var sb = new StringBuilder();
				foreach (var paramName in _statistics.ParameterNames)
				{
					sb.AppendFormat("{0,30}\t-\t{1}", paramName, _statistics.GetParameterValue(paramName));
					sb.AppendLine();
				}
				return StringPage.SerializeStatic("Server Statistics", sb.ToString());
			});
		}

		readonly Dictionary<string, Func<string>> _systemPages = new Dictionary<string, Func<string>>();

		void StorageDataChanged(object sender, DataChangedEventArgs e)
		{
			ThreadPool.QueueUserWorkItem(delegate
			{
				IFaatServiceCallback[] callbacks;
				lock (_callbacks)
				{
					callbacks = _callbacks.ToArray();
				}
				foreach (var callback in callbacks)
				{
					if (((ICommunicationObject)callback).State == CommunicationState.Opened)
					{
						ThreadPool.QueueUserWorkItem(x =>
						{
							var sw = Stopwatch.StartNew();
							var localCallBack = (IFaatServiceCallback)x;
							try
							{
								localCallBack.OnDataChanged(e.Identity);
							}
							catch (Exception ex)
							{
								XTrace.XTrace.Exception(ex);
								lock (_callbacks)
								{
									_callbacks.Remove(localCallBack);
								}
							}
							sw.Stop();
							_statistics.NumericAverrage("CallBack AverageTime", sw.ElapsedTicks, ParameterKind.TimeSpan);
						}, callback);
					}
					else
					{
						lock (_callbacks)
						{
							_callbacks.Remove(callback);
						}
					}
				}
			});
		}

		static readonly XTraceSource Trace = new XTraceSource("Faat ControllerService WCF");

		public string GetData(string identity)
		{
			Trace.Verbose("WCF GetData", "page={0}, session={1}", identity, OperationContext.Current.SessionId);

			string data;
			Func<string> func;
			if (_systemPages.TryGetValue(identity, out func))
			{
				data = func();
			}
			else
			{
				data = _storage.GetData(identity);
			}

			_statistics.NumericIncrement("GetData Count");
			if (data != null)
			{
				_statistics.NumericAverrage("GetData AverrageSize", data.Length, ParameterKind.InfoSize);
			}
			return data;
		}

		public void SetData(string identity, string data)
		{
			Trace.Verbose("WCF SetData", "page={0}, session={1}, data={2}", identity, OperationContext.Current.SessionId, data);
			_statistics.NumericIncrement("SetData Count");
			if (data != null)
			{
				_statistics.NumericAverrage("SetData AverrageSize", data.Length, ParameterKind.InfoSize);
			}
			_storage.SetData(identity, data);
		}

		readonly HashSet<IFaatServiceCallback> _callbacks = new HashSet<IFaatServiceCallback>();

		public void Subscribe()
		{
			var callback = OperationContext.Current.GetCallbackChannel<IFaatServiceCallback>();
			if (callback != null)
			{
				ThreadPool.QueueUserWorkItem(x =>
				{
					var localCallBack = (IFaatServiceCallback)x;
					var msg = Guid.NewGuid().ToString();
					if (localCallBack.PingClient(msg) == msg)
					{
						if (!_callbacks.Contains(localCallBack))
						{
							lock (_callbacks)
							{
								// second check is implied
								_callbacks.Add(localCallBack);
							}
						}
					}
				}, callback);
			}
		}

		public string PingServer(string message)
		{
			return message;
		}

	}
}