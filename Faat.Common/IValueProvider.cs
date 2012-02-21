using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Faat
{
	public interface IValueProvider : INotifyPropertyChanged
	{
		object Value { get; }
	}

	public interface IValueProvider<out T> : IValueProvider
	{
		new T Value { get; }
	}

	public class ValueProvider<T> : IValueProvider<T>
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
		}

		T _value;
		public T Value
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
