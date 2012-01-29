using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;

using Faat.Contract;
using Faat.Core;

namespace Faat.Host
{
	public class FilePage : StringPage
	{
		readonly string _fileName;
		Cache<StreamReader> _stream;

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
				throw new ArgumentException("Path is not rooted", "filePath");
			}
			Refresh(File.ReadAllText(filePath));
		}
	}

	public abstract class StringPage : IDisposable, IPage
	{
		string _name;
		readonly Cache<string> _content;

		string ReadLine(ref string str, ref int position)
		{
			var old = position;
			position = str.IndexOf(Environment.NewLine, position, StringComparison.Ordinal) + Environment.NewLine.Length;
			return str.Substring(old, position - old).Trim();
		}

		string ReadToEnd(ref string str, ref int position)
		{
			var end = str.Substring(position);
			position = str.Length;
			return end;
		}


		public void Refresh(string content)
		{
			int pos = 0;
			_name = ReadLine(ref content, ref pos) ?? "Noname";
			_childrenLine = ReadLine(ref content, ref pos) ?? "";
			_parentsLine = ReadLine(ref content, ref pos) ?? "";
			_linksLine = ReadLine(ref content, ref pos) ?? "";
			_children = Cache.New(() => ReadList(_childrenLine, x => x.Parents, () =>
			{
				_childrenLine = WriteList(_children.Value);
				Write();
			}));
			_parents = Cache.New(() => ReadList(_parentsLine, x => x.Children, () =>
			{
				_parentsLine = WriteList(_parents.Value);
				Write();
			}));
			_links = Cache.New(() => ReadList(_linksLine, x => x.Links, () =>
			{
				_linksLine = WriteList(_links.Value);
				Write();
			}));

			_content = Cache.New(() => _stream.Value.ReadToEnd());
		}

		static string WriteList(IEnumerable<IPage> pageCollection)
		{
			return string.Join("|", pageCollection.Select(x => x.Identity));
		}

		SyncOppositeList<IPage> ReadList(string line, Func<IPage, ICollection<IPage>> oppositeList, Action altered)
		{
			var list = new SyncOppositeList<IPage>(this, oppositeList, altered);
			var pages = line.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
			var path = Path.GetDirectoryName(_fileName);
			if (string.IsNullOrEmpty(path))
			{
				throw new InvalidOperationException("Path does not exists");
			}
			foreach (var page in pages)
			{
				list.Add(new FilePage(Path.Combine(path, page)));
			}
			return list;
		}

		void InitStream()
		{
			_stream = new Cache<StreamReader>(() => new StreamReader(File.Open(_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)));
		}

		string _childrenLine;
		Cache<SyncOppositeList<IPage>> _children;
		public ICollection<IPage> Children
		{
			get { return _children.Value; }
		}

		string _parentsLine;
		Cache<SyncOppositeList<IPage>> _parents;
		public ICollection<IPage> Parents
		{
			get { return _parents.Value; }
		}

		string _linksLine;
		Cache<SyncOppositeList<IPage>> _links;
		public ICollection<IPage> Links
		{
			get
			{
				return _links.Value;
			}
		}

		public string Identity
		{
			get { return Path.GetFileName(_fileName); }
			set
			{
				throw new NotSupportedException();
			}
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
				sw.WriteLine(_childrenLine);
				sw.WriteLine(_parentsLine);
				sw.WriteLine(_linksLine);
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

// ReSharper disable UnusedParameter.Local
		void Dispose(bool managed)
// ReSharper restore UnusedParameter.Local
		{
			CloseFile();
		}
	}
}