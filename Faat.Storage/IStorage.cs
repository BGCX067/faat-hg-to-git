using System;

using MySpring;

namespace Faat.Storage
{
	public interface IStorage
	{
		IPage GetPage(string identity);
		string GetData(string identity);
		void SetData(string identity, string data);

		event EventHandler<DataChangedEventArgs> DataChanged;
	}

	public class DataChangedEventArgs : EventArgs
	{
		public string Identity { get; set; }
	}
}
