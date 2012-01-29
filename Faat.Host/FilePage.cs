using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;

using Faat.Common;
using Faat.Core;

namespace Faat.Host
{
	public class FilePage : IDisposable, IPage
	{
		readonly string _fileName;
		Cache<StreamReader> _stream;

		string _name;
		readonly Cache<string> _content;

		/// <summary>
		/// Create new page
		/// </summary>
		/// <param name="directory"></param>
		/// <param name="pageName"></param>
		public static FilePage Create(string directory, string pageName)
		{
			string path;
			return Create(directory, pageName, out path);
		}

		/// <summary>
		/// Create new page
		/// </summary>
		/// <param name="directory"></param>
		/// <param name="pageName"></param>
		/// <param name="path"> </param>
		public static FilePage Create(string directory, string pageName, out string path)
		{
			var file = Path.Combine(directory, Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(directory);
			File.WriteAllText(file, pageName);
			path = file;
			return new FilePage(file);
		}

		public FilePage(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException("filePath");
			}
			if (!Path.IsPathRooted(filePath))
			{
				throw new ArgumentException("Path is not rooted","filePath");
			}
			_fileName = filePath;

			InitStream();
			_name = _stream.Value.ReadLine();
			_content = new Cache<string>(() => _stream.Value.ReadToEnd());
		}

		void InitStream()
		{
			_stream = new Cache<StreamReader>(() => new StreamReader(File.Open(_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)));
		}

		Lazy<SyncOppositeList<IPage>> _children;
		public ICollection<IPage> Children
		{
			get { throw new NotImplementedException(); }
		}

		Lazy<SyncOppositeList<IPage>> _parents;
		public ICollection<IPage> Parents
		{
			get { throw new NotImplementedException(); }
		}

		public ICollection<IPage> Links
		{
			get { throw new NotImplementedException(); }
		}

		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				Write();
			}
		}

		public string Content
		{
			get { return _content.Value; }
			set
			{
				_content.Value = value;
				Write();
			}
		}

		void Write()
		{
			var name = Name;
			var content = Content;
			CloseFile();
			using(var sw = new StreamWriter(_fileName))
			{
				sw.WriteLine(name);
				sw.WriteLine(content);
			}
			InitStream();
		}

		void CloseFile()
		{
			var cachedStream = _stream;
			if (cachedStream !=null && cachedStream.IsValueCreated)
			{
				var disposableStream = cachedStream.Value as IDisposable;
				if (disposableStream != null)
				{
					disposableStream.Dispose();
				}
			}
			_stream = null;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		~FilePage()
		{
			GC.SuppressFinalize(this);
			Dispose(false);
		}

		void Dispose(bool managed)
		{
			CloseFile();
		}
	}
}