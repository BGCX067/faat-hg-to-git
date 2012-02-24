using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using MyUtils;

namespace Faat
{
	public interface IValueProvider : INotifyPropertyChanged
	{
		object Value { get; set; }
	}

	public interface IValueProvider<T> : IValueProvider
	{
		new T Value { get; set; }
	}

	public static class ValueProvider
	{
		public static IValueProvider<T> Create<T>(T value)
		{
			return new ValueProvider<T> { Value = value };
		}
		public static IValueProvider<T> Create<T>(Func<T> getter)
		{
			return new DelegateValueProvider<T>(getter, null);
		}
		public static IValueProvider<T> Create<T>(Func<T> getter, Action<T> setter)
		{
			return new DelegateValueProvider<T>(getter, setter);
		}
		public static IValueProvider<T> Create<T>(Func<T> getter, Action<T> setter, INotifyPropertyChanged npc, params string[] properyNames)
		{
			return new DelegateValueProvider<T>(getter, setter, npc, properyNames);
		}
	}

	public class DelegateValueProvider<T> : BaseValueProvider<T>
	{
		readonly Func<T> _getter;
		readonly Action<T> _setter;
		readonly INotifyPropertyChanged _observable;
		readonly string[] _propertiesToObserve;

		public DelegateValueProvider(Func<T> getter, Action<T> setter = null, INotifyPropertyChanged observable = null, params string[] propertiesToObserve)
		{
			if (getter == null)
			{
				throw new ArgumentNullException("getter");
			}
			_getter = getter;
			_setter = setter;
			_observable = observable;
			_propertiesToObserve = propertiesToObserve;
			if (_observable != null)
			{
				if (_propertiesToObserve == null || _propertiesToObserve.Length <= 0)
				{
					_observable.PropertyChanged += ObservableAnyPropertyChanged;
				}
				else if (_propertiesToObserve.Length == 1)
				{
					_observable.PropertyChanged += ObservableSpecifiedSinglePropertyChanged;
				}
				else
				{
					_observable.PropertyChanged += ObservableSpecifiedListPropertyChanged;
				}
			}
		}

		void ObservableAnyPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			Notify();
		}

		void ObservableSpecifiedSinglePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.Is(_propertiesToObserve[0]))
			{
				Notify();
			}
		}

		void ObservableSpecifiedListPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_propertiesToObserve.Any(e.Is))
			{
				Notify();
			}
		}

		public override T Value
		{
			get { return _getter(); }
			set
			{
				if (_setter != null)
				{
					_setter(value);
				}
				else
				{
					throw new NotSupportedException("Value is readonly");
				}
			}
		}

		public void Notify()
		{
			OnPropertyChanged("Value");
		}
	}

	public abstract class BaseValueProvider<T> : IValueProvider<T>
	{
		
		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			OnPropertyChanged(new PropertyChangedEventArgs(name));
		}

		protected void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		#endregion

		object IValueProvider.Value
		{
			get { return Value; }
			set { Value = (T)value; }
		}

		public abstract T Value { get; set; }
	}

	public class ValueProvider<T> : BaseValueProvider<T>
	{

		T _value;
		public override T Value
		{
			get { return _value; }
			set
			{
				if (!Equals(_value, value))
				{
					_value = value;
					OnPropertyChanged("Value");
				}
			}
		}
	}
}
