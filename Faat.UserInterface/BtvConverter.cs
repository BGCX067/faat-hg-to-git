using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Faat.UserInterface
{
	public class BtbNegateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var val = (bool)value;
			return !val;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	public class BtvConverter : IValueConverter
	{
		public bool IsInverted { get; set; }
		public bool IsHide { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var val = (bool)value;
			if (IsInverted)
			{
				val = !val;
			}
			return val ? Visibility.Visible : (IsHide ? Visibility.Hidden : Visibility.Collapsed);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
