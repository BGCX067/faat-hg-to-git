using System;
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
		static string GetDefaultPath()
		{
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Faat", "ServerRepo");
		}

		readonly DirectoryInfo _dir;

		[DefaultConstructor]
		public FileSystemStorage(string path = null)
			: this(new DirectoryInfo(path.IsNullOrWhitespaces() ? GetDefaultPath() : path))
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


		public string GetData(string identity)
		{
			var file = FileName(identity);
			if (File.Exists(file))
			{
				return File.ReadAllText(file);
			}
			return null;
		}

		public GetDataResult GetData2(string identity)
		{
			return new GetDataResult
			{
				Data =  GetData(identity),
			};
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

		public void SetData(string identity, MetaData metaData, string data)
		{
			SetData(identity, data);
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
