using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Faat.Service
{
	[ServiceContract]
	public interface IFaatService
	{
		[OperationContract]
		string GetPage(string identity);

		[OperationContract]
		void SetPage(string identity, string content);
	}

}
