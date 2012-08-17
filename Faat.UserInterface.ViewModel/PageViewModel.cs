using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Faat.Agent;
using Faat.Model;
using Faat.Storage;
using Faat.Storage.Remote;

using MyUtils;

namespace Faat.UserInterface.ViewModel
{
	/*@ fileinporject */
	class PageViewModel /*= ": DependencyObject  , INotifyPropertyChanged" */
	{
		/*# Mixin<INotifyPropertyChanged, ObservableObject> */

		readonly IPage _page;
		readonly PageViewModel _parent;
		// static readonly WeakValueCache<CacheKey, PageViewModel> _cacheByPageAndParent = new WeakValueCache<CacheKey, PageViewModel>();
		static readonly WeakValueCache<string, WeakValueCache<string, PageViewModel>> _cacheByPage = new WeakValueCache<string, WeakValueCache<string, PageViewModel>>();

		class CacheKey
		{
			readonly IPage _page;
			readonly IPage _parent;

			public CacheKey(IPage page, IPage parent)
			{
				_page = page;
				_parent = parent;
			}

			public bool Equals(CacheKey other)
			{
				if (ReferenceEquals(null, other))
				{
					return false;
				}
				if (ReferenceEquals(this, other))
				{
					return true;
				}
				return Equals(other._page, _page) && Equals(other._parent, _parent);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj))
				{
					return false;
				}
				if (ReferenceEquals(this, obj))
				{
					return true;
				}
				if (obj.GetType() != typeof(CacheKey))
				{
					return false;
				}
				return Equals((CacheKey)obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return ((_page != null ? _page.GetHashCode() : 0) * 397) ^ (_parent != null ? _parent.GetHashCode() : 0);
				}
			}
		}

		public bool IsRoot
		{
			get { return _parent == null; }
		}

		public static PageViewModel Create(IPage page, PageViewModel parent)
		{
			// var result = _cacheByPageAndParent[new CacheKey(page, parent), () => new PageViewModel(page, parent)];
			var byParentCollection = _cacheByPage[page.Identity, () => new WeakValueCache<string, PageViewModel>()];
			var result = byParentCollection[parent == null ? "RootInstance" : parent.Page.Identity, () => new PageViewModel(page, parent)];
			return result;
		}

		public string ContentTokenized
		{
			get
			{
				return XamlServices.Save(new PageParser().Parse(Page).PageTokens);
			}
		}

		PageViewModel _selectedParent;

		public PageViewModel SelectedParent
		{
			get { return _selectedParent; }
			set
			{
				_selectedParent = value;
				OnPropertyChanged("SelectedParent");
			}
		}

		public override string ToString()
		{
			return "PVM: " + ParentPage +"\\"+ Page;
		}

		public StringPage Page
		{
			get { return (StringPage)_page; }
		}

		PageViewModel(IPage page, PageViewModel parent)
		{
			_page = page;
			_parent = parent;
			var npc = _page as INotifyPropertyChanged;
			if (npc != null)
			{
				npc.PropertyChanged += PagePropertyChanged;
			}
		}

		void PagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.Is("Content"))
			{
				OnPropertyChanged("ContentTokenized");
			}
		}

		public bool IsCutted
		{
			get { return _bufferPage == this && _bufferCut; }
		}

		public bool IsCopied
		{
			get { return _bufferPage!=null && _bufferPage.Page == Page && !_bufferCut; }
		}

		public IEnumerable<PageViewModel> Children
		{
			get { return OE.Select(_page.Children, x=>Create(x, this)); }
		}

		public IPage ParentPage
		{
			get { return _parent == null ? null : _parent.Page; }
		}

		public PageViewModel ParentViewModel
		{
			get { return _parent; }
		}

		public RemoteStorage Storage
		{
			get { return (RemoteStorage)Page.Storage; }
		}

		#region Commands

		static PageViewModel _bufferPage;

		/// <summary>
		/// Cut behaviour instead of copy
		/// </summary>
		static bool _bufferCut;

		public bool CanCreateChildPage
		{
			get { return Page != Storage.GetRecycleBin(); }
		}

		public void CreateChildPage()
		{
			Page.Children.Add(new StringPage(Page.Storage) {Name = "New page"});
		}

		public void Cut()
		{
			Cut(this);
		}

		public bool CanPasteChild
		{
			get { return _bufferPage != null; }
		}

		public void PasteChild()
		{
			if (_bufferPage != null)
			{
				var buffer = _bufferPage;
				if (_bufferCut)
				{
					_bufferPage.Page.Parents.Remove(_bufferPage.ParentPage);
					Copy(null);
				}
				Page.Children.Add(buffer.Page);
			}
		}

		public static bool Cut(PageViewModel page)
		{
			return Copy(page, true);
		}

		public static bool Copy(PageViewModel page, bool cutBehaviour = false)
		{
			if (page != _bufferPage || cutBehaviour != _bufferCut)
			{
				var oldPage = _bufferPage;
				_bufferPage = page;
				_bufferCut = cutBehaviour;
				var newPage = _bufferPage;

				if (oldPage != null)
				{
					oldPage.OnPropertyChanged("IsCutted");
					oldPage.OnPropertyChanged("IsCopied");
					foreach (var variable in _cacheByPage[oldPage.Page.Identity].Entries.Select(x=>x.Value))
					{
						variable.OnPropertyChanged("IsCopied");
					}
				}
				if (newPage != null)
				{
					newPage.OnPropertyChanged("IsCutted");
					newPage.OnPropertyChanged("IsCopied");
					foreach (var variable in _cacheByPage[newPage.Page.Identity].Entries.Select(x => x.Value))
					{
						variable.OnPropertyChanged("IsCopied");
					}
				}
			}

			return true;
		}

		public bool CanRecycle
		{
			get
			{
				var rb = Storage.GetRecycleBin();
				return Page.Parents.All(x => x != rb) && !IsRootVersionExists;
			}
		}

		bool IsRootVersionExists
		{
			get { return _cacheByPage[Page.Identity].Entries.Any(x => x.Value.IsRoot); }
		}

		public void Recycle()
		{
			Page.Parents.Clear();
			Storage.GetPage(Const.RecycleBinPage).Children.Add(Page);
		}

		public bool CanRemove
		{
			get { return ParentPage != null && Page.Parents.Contains(ParentPage) && Page.Parents.Count > 1; }
		}

		public ExecutionResultState? ExecutionResult
		{
			get
			{
				ExecutionResultState state;
				Enum.TryParse(Storage.GetData(Const.GlobalPageResultSimplePrefix + Page.Identity), out state);
				return state;
			}
		}

		public bool HasExecutionResult
		{
			get
			{
				return ExecutionResult.HasValue && ExecutionResult != ExecutionResultState.Unknown;
			}
		}

		public void Remove()
		{
			Page.Parents.Remove(ParentPage);
		}

		public void Copy()
		{
			Copy(this);
		}

		public void Run()
		{
			var ae = new FaatAgentExecutor();
			var exr = ae.RunPage(Storage.Address, Page.Identity, null/*, Const.GlobalPageResultPrefix + Page.Identity*/);
			var state = exr.Page.ResultState;
			// MessageBox.Show(state == null ? "Page state is null" : state.ToString());
			// todo
		}

		#endregion

		public static void Subscribe(IStorage storage)
		{
			storage.DataChanged -= StorageDataChanged;
			storage.DataChanged += StorageDataChanged;
		}

		static void StorageDataChanged(object sender, DataChangedEventArgs e)
		{
			ForceRefresh(e.Identity);

			if (e.Identity.StartsWith(Const.GlobalPageResultSimplePrefix))
			{
				ForceRefresh(e.Identity.Substring(Const.GlobalPageResultSimplePrefix.Length));
			}
//			else if (e.Identity.StartsWith(Const.GlobalPageResultSimplePrefix))
//			{
//				ForceRefresh(e.Identity.Substring(Const.GlobalPageResultSimplePrefix.Length));
//			}
		}

		static void ForceRefresh(string identity)
		{
			var listOfViewModels = _cacheByPage[identity];
			if (listOfViewModels != null)
			{
				foreach (var page in listOfViewModels.Entries.Select(x => x.Value))
				{
					page.OnPropertyChanged(null); // page content is updated on server
				}
			}
		}
	}

	public static partial class Ext
	{
		public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> source)
		{
			if (source == null)
			{
				return Enumerable.Empty<T>();
			}
			return source;
		}
	}
}
