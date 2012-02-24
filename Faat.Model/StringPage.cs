using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Text;

using Faat.Storage;

using MyUtils;

namespace Faat.Model
{
	public class StringPage : ObservableObject, IPage
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
				if (!_debugInstantiated.TryGetValue(Storage, out map))
				{
					_debugInstantiated[Storage] = map = new Dictionary<string, IPage>();
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
//			if (storage == null)
//			{
//				throw new ArgumentNullException("storage");
//			}
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

			if (identity == null || !Load())
			{
				_name = "New Page";
			}
		}

		public bool Load()
		{
			return Load(Storage.GetData(Identity));
		}

		public bool Load(string pageData)
		{
			if (string.IsNullOrWhiteSpace(pageData))
			{
				return false;
			}
			int pos = 0;
			_name = ReadLine(ref pageData, ref pos);
			_childrenLine = ReadLine(ref pageData, ref pos);
			_parentsLine = ReadLine(ref pageData, ref pos);
			_linksLine = ReadLine(ref pageData, ref pos);
			_content = ReadToEnd(ref pageData, ref pos);
			OnPropertyChanged(null);
			return true;
		}

		static string WriteList(IEnumerable<IPage> pageCollection)
		{
			return string.Join("|", pageCollection.Select(x => x.Identity));
		}

		SyncOppositeList<IPage> ReadList(string line, Func<IPage, ICollection<IPage>> oppositeList, Action altered)
		{
			var pagesIds = (line??"").Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
			var pages = pagesIds.Select(x => Storage.GetPage(x)).ToArray();

			return new SyncOppositeList<IPage>(this, oppositeList, altered, pages);
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
				if (_name != value)
				{
					_name = value;
					OnPropertyChanged("Name");
					Write();
				}
			}
		}

		public string Content
		{
			get { return _content; }
			set
			{
				if (_content != value)
				{
					_content = value;
					XTrace.XTrace.Verbose("Page Content Changed");
					OnPropertyChanged("Content");
					Write();
				}
			}
		}

		public IStorage Storage
		{
			get { return _storage; }
		}

		void Write()
		{
			Storage.SetData(_identity, Serialize());
		}

		public string Serialize()
		{
			return SerializeStatic(Name, Content, _childrenLine, _parentsLine, _linksLine);
		}

		public static string SerializeStatic(string name, string content, string childrens = null, string parents = null, string links = null)
		{
			var sb = new StringBuilder();
			sb.AppendLine(name);
			sb.AppendLine(childrens);
			sb.AppendLine(parents);
			sb.AppendLine(links);
			sb.Append(content);
			if (content == null || !content.EndsWith(Environment.NewLine))
			{
				sb.AppendLine();
			}
			return sb.ToString();
		}

		public override string ToString()
		{
			return "P: " + Name + " #" + Identity.Take(5).Select(x => x.ToString(CultureInfo.InvariantCulture)).Join("");
		}
	}
}