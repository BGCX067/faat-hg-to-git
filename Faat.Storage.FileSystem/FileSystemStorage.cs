﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Faat.Model;

using MySpring;

using MyUtils;

namespace Faat.Storage.FileSystem
{
	public class FileSystemStorage : IStorage, IDisposable
	{
		readonly DirectoryInfo _dir;

		[DefaultConstructor]
		public FileSystemStorage():this(new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Faat", "ServerRepo")))
		{
		}

		public FileSystemStorage(DirectoryInfo dir)
		{
			_dir = dir;
			Directory.CreateDirectory(dir.FullName);
		}

		string FileName(string identity)
		{
			//if (identity.Any(x => !char.IsLetterOrDigit(x)))
			//{
			//   throw new ArgumentException("Page identity can contain only letters and numbers", "identity");
			//}
			var path = Path.Combine(_dir.FullName, identity);
			return path;
		}

		readonly WeakValueCache<string, IPage> _objects = new WeakValueCache<string, IPage>();

		public IPage GetPage(string identity)
		{
			lock (_objects)
			{
				return _objects[identity, () => new StringPage(this, identity)];
			}
		}

		public string GetData(string identity)
		{
			var file = FileName(identity);
			if (File.Exists(file))
			{
				return File.ReadAllText(file);
			}
			return null;
		}

		readonly object _syncWrite = new object();

		public void SetData(string identity, string data)
		{
			lock (_syncWrite)
			{
				File.WriteAllText(FileName(identity), data);
			}
			OnDataChanged(identity);
		}

		public event EventHandler<DataChangedEventArgs> DataChanged;

		protected void OnDataChanged(string identity)
		{
			OnDataChanged(new DataChangedEventArgs {Identity = identity});
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
		}
	}
}