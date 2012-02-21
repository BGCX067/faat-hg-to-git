using System.ComponentModel;
using System.Collections.Generic;
using System;
using System.Windows;

using MyUtils.UI;

namespace Faat
{
	public abstract class ObservableObject : DispatchableObservable //INotifyPropertyChanged
	{
//		public event PropertyChangedEventHandler PropertyChanged;
//
//		protected void OnPropertyChanged(string name)
//		{
//			var handler = PropertyChanged;
//			if (handler != null)
//			{
//				Disp
//				handler(this, new PropertyChangedEventArgs(name));
//			}
//		}
	}
}