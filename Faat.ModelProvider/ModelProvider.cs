using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Faat.Model;
using Faat.Storage;

using MyUtils;

namespace Faat.ModelProvider
{
	public class ModelProvider: IModelProvider
	{
		readonly IStorage _storage;

		public ModelProvider(IStorage storage)
		{
			if (storage == null)
			{
				throw new ArgumentNullException("storage");
			}
			_storage = storage;
		}

		readonly WeakValueCache<string, IPage> _objects = new WeakValueCache<string, IPage>();

		public IPage GetPage(string identity)
		{
			lock (_objects)
			{
				return _objects[identity, () => new StringPage(this, identity)];
			}
		}

		void IModelProvider.SetData(string identity, string data)
		{
			_storage.SetData(identity, data);
		}

		string IModelProvider.GetData(string identity)
		{
			return _storage.GetData(identity);
		}
	}
}
