using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Faat.UserInterface
{
	class StyleSwitcher : ObservableObject
	{
		static readonly StyleSwitcher _default = new StyleSwitcher();

		public static StyleSwitcher Default
		{
			get { return _default; }
		}

		Style _currentStyle;

		public Style CurrentStyle
		{
			get { return _currentStyle; }
			set
			{
				_currentStyle = value;
				OnPropertyChanged("CurrentStyle");
			}
		}
	}
}
