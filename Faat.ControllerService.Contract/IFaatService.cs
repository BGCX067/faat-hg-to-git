using System.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;

using MySpring;

namespace Faat.ControllerService
{
	[ServiceContract(CallbackContract=typeof(IFaatServiceCallback))]
	[DefaultImpl("Faat.ControllerService.FaatService, Faat.ControllerService")]
	public interface IFaatService
	{
		[OperationContract]
		string GetData(string identity);

		[OperationContract]
		void SetData(string identity, string data);

		[OperationContract]
		[OneWay]
		void Subscribe();

		/// <summary>
		/// Just check a connection. Valid method should return same value as input.
		/// </summary>
		/// <param name="message"> </param>
		/// <returns></returns>
		[OperationContract]
		string PingServer(string message);
	}

	[ServiceContract]
	public interface IFaatServiceCallback
	{
		[OperationContract]
		[OneWay]
		void OnDataChanged(string identity);

		[OperationContract]
		string PingClient(string message);
	}

}
