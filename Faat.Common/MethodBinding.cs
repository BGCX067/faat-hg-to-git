using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

using MyUtils;
using MyUtils.UI;

[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Faat")]

namespace Faat
{
	[MarkupExtensionReturnType(typeof(ICommand))]
	public class MethodBindingExtension : MarkupExtension, IValueConverter
	{
		const BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Public;

		public MethodBindingExtension()
		{
			
		}

		public MethodBindingExtension(string methodName)
		{
			MethodName = methodName;
		}

		[ConstructorArgument("methodName")]
		public string MethodName { get; set; }

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			// binding to current DataContext (path is empty)
			// converter parameter contains method name to call
			// converter will convert object to DelegateCommand with specified method invocation
			return new Binding
			{
				Converter = this,
				ConverterParameter = MethodName,
			}.ProvideValue(serviceProvider);
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				var type = value.GetType();
				var executeMethod =type.GetMethod(MethodName, _bindingFlags);
				if (executeMethod != null)
				{
					var act = executeMethod.Compile<Action<object>>(); // Action<object> because it is open-delegate. Method signature still void();
					Func<object, bool> canAct = null; // Action<object> because it is open-delegate. Method signature is bool();

					// possible cases:
					// bool CanMethodName() {}
					// bool get_CanMethodName() {}
					// bool CanMethodName { get {} } // Recomended
					var canExecuteMethod = type.GetMethod("Can" + MethodName, _bindingFlags) ?? type.GetMethod("get_Can" + MethodName, _bindingFlags);

					if (canExecuteMethod != null)
					{
						canAct = canExecuteMethod.Compile<Func<object, bool>>();
					}

					var cmd = new DelegateCommand(x => act(value), canAct == null ? default(Func<object, bool>) : x => canAct(value));
					return cmd;
				}
				else
				{
					PresentationTraceSources.DataBindingSource.TraceEvent(TraceEventType.Error, 0, "MethodBinding: Method '{0}' not found on data context '{1}'", MethodName, value);
					// can execute - false
					return new DelegateCommand(_ => { }, _ => false);
				}
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
