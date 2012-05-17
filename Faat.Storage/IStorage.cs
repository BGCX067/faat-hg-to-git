using System;
using System.Runtime.Serialization;

using MySpring;

namespace Faat.Storage
{
	public interface IStorage
	{
		// IPage GetPage(string identity);
		string GetData(string identity);
		GetDataResult GetData2(string identity);

		void SetData(string identity, string data);
		void SetData(string identity, MetaData metaData, string data);

		event EventHandler<DataChangedEventArgs> DataChanged;
	}

	[DataContract]
	public class MetaData
	{

	}

	[DataContract]
	public class GetDataResult
	{
		public string Data { get; set; }
		public MetaData Meta { get; set; }
	}



	public class DataChangedEventArgs : EventArgs
	{
		public string Identity { get; set; }
	}
}
