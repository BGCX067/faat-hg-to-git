using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;

using Faat.Contract;
using Faat.Core;

namespace Faat.Host
{
	public class RamPage : IPage
	{
		public RamPage()
		{
			_children = new SyncOppositeList<IPage>(this, x => x.Parents);
			_parents = new SyncOppositeList<IPage>(this, x => x.Children);
			_links = new SyncOppositeList<IPage>(this, x => x.Links);
		}

		readonly ICollection<IPage> _children;
		public ICollection<IPage> Children
		{
			get { return _children; }
		}

		readonly ICollection<IPage> _parents;
		public ICollection<IPage> Parents
		{
			get { return _parents; }
		}

		readonly ICollection<IPage> _links;
		public ICollection<IPage> Links
		{
			get { return _links; }
		}

		public string Identity { get; set; }

		public string Name { get; set; }
		public string Content { get; set; }
	}

	public class SyncOppositeList<T> : ICollection<T>
	{
		readonly HashSet<T> _list = new HashSet<T>();

		readonly T _owner;
		readonly Func<T, ICollection<T>> _oppositeList;
		readonly Action _altered;

		public SyncOppositeList(T owner, Func<T, ICollection<T>> oppositeList, Action altered = null)
		{
			_owner = owner;
			_oppositeList = oppositeList;
			_altered = altered ?? delegate { };
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		void AddAspect(T item)
		{
			_oppositeList(item).Add(_owner);
			_altered();
		}

		void RemoveAspect(T item)
		{
			_oppositeList(item).Remove(_owner);
			_altered();
		}

		public void Add(T item)
		{
			var ok = _list.Add(item);
			if (ok)
			{
				AddAspect(item);
			}
		}

		public void Clear()
		{
			foreach (var item in _list.ToArray())
			{
				Remove(item);
			}
		}

		public bool Contains(T item)
		{
			return _list.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(T item)
		{
			var ok = _list.Remove(item);
			if (ok)
			{
				RemoveAspect(item);
			}
			return ok;
		}

		public int Count
		{
			get { return _list.Count; }
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

	}
}