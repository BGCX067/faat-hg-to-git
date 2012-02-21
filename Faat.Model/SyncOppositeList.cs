using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System;

using XTrace;

namespace Faat.Model
{
	public class SyncOppositeList<T> : ICollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
	{
		readonly HashSet<T> _list = new HashSet<T>();
		readonly ObservableCollection<T> _observable;

		readonly T _owner;
		readonly Func<T, ICollection<T>> _oppositeList;
		readonly Action _altered;

		public SyncOppositeList(T owner, Func<T, ICollection<T>> oppositeList, Action altered = null, params T[] items)
		{
			_owner = owner;
			_oppositeList = oppositeList;
			_altered = altered ?? delegate { };

			foreach (var item in items)
			{
				_list.Add(item);
			}
			_observable = new ObservableCollection<T>(items);

			_observable.CollectionChanged += ObservableCollectionChanged;
			(_observable as INotifyPropertyChanged).PropertyChanged += ObservableCollectionPropertyChanged;
		}

		void ObservableCollectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(e);
		}

		void ObservableCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			OnCollectionChanged(e);
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
			_observable.Add(item);
			_altered();
			_oppositeList(item).Add(_owner);
		}

		void RemoveAspect(T item)
		{
			_observable.Remove(item);
			_altered();
			_oppositeList(item).Remove(_owner);
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
			_list.CopyTo(array, arrayIndex);
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

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		static readonly XTraceSource Trace = new XTraceSource("SyncOppositeList");

		protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			var handler = CollectionChanged;
			if (handler != null)
			{
				Trace.Verbose("NotifyCollection", "action {2}, owner {0}, list hash {1:X}, index n{3} o{4}, items {5}", _owner, GetHashCode(), e.Action, e.NewStartingIndex, e.OldStartingIndex, (e.NewItems ?? e.OldItems).Cast<object>().FirstOrDefault());
				handler(this, e);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		protected void OnPropertyChanged(string name)
		{
			OnPropertyChanged(new PropertyChangedEventArgs(name));
		}
	}
}