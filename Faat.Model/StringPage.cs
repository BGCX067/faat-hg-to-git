using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Text;

using Faat.Storage;

namespace Faat.Model
{
	public class StringPage : IPage
	{
		readonly IStorage _storage;
		readonly string _identity;

		string _name;
		string _content;

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

#if DEBUG
		static readonly Dictionary<IStorage, Dictionary<string, IPage>> _debugInstantiated = new Dictionary<IStorage, Dictionary<string, IPage>>();

		Dictionary<string, IPage> DebugInstMapFromMap
		{
			get
			{
				Dictionary<string, IPage> map;
				if (!_debugInstantiated.TryGetValue(_storage, out map))
				{
					_debugInstantiated[_storage] = map = new Dictionary<string, IPage>();
				}
				return map;
			}
		}

#endif

		[Conditional("DEBUG")]
		void DebugRegister(string identity)
		{
			DebugInstMapFromMap.Add(identity, this);
		}

		public StringPage(IStorage storage, string identity = null)
		{
			if (storage == null)
			{
				throw new ArgumentNullException("storage");
			}
			_storage = storage;
			_identity = identity ?? Guid.NewGuid().ToString("N");

			DebugRegister(_identity);

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

			if (identity != null)
			{
				Load();
			}
			else
			{
				_name = "New Page";
			}
		}

		public void Load()
		{
			Load(_storage.GetPageData(Identity));
		}

		public void Load(string pageData)
		{
			int pos = 0;
			_name = ReadLine(ref pageData, ref pos);
			_childrenLine = ReadLine(ref pageData, ref pos);
			_parentsLine = ReadLine(ref pageData, ref pos);
			_linksLine = ReadLine(ref pageData, ref pos);
			_content = ReadToEnd(ref pageData, ref pos);
		}

		static string WriteList(IEnumerable<IPage> pageCollection)
		{
			return string.Join("|", pageCollection.Select(x => x.Identity));
		}

		SyncOppositeList<IPage> ReadList(string line, Func<IPage, ICollection<IPage>> oppositeList, Action altered)
		{
			var list = new SyncOppositeList<IPage>(this, oppositeList, altered);
			var pages = (line??"").Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
			foreach (var page in pages)
			{
				list.Add(_storage.GetPage(page));
			}
			return list;
		}

		string _childrenLine;
		readonly Cache<SyncOppositeList<IPage>> _children;
		public ICollection<IPage> Children
		{
			get { return _children.Value; }
		}

		string _parentsLine;
		readonly Cache<SyncOppositeList<IPage>> _parents;
		public ICollection<IPage> Parents
		{
			get { return _parents.Value; }
		}

		string _linksLine;
		readonly Cache<SyncOppositeList<IPage>> _links;
		public ICollection<IPage> Links
		{
			get
			{
				return _links.Value;
			}
		}

		public string Identity
		{
			get { return _identity; }
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
			get { return _content; }
			set
			{
				_content = value;
				Write();
			}
		}

		void Write()
		{
			_storage.SetPageData(_identity, Serialize());
		}

		public string Serialize()
		{
			var sb = new StringBuilder();
			sb.AppendLine(Name);
			sb.AppendLine(_childrenLine);
			sb.AppendLine(_parentsLine);
			sb.AppendLine(_linksLine);
			sb.AppendLine(Content);
			return sb.ToString();
		}

		public override string ToString()
		{
			return Name;
		}
	}
}