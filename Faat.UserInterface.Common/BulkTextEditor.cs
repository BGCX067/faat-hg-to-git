using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

using ICSharpCode.AvalonEdit;

using XTrace;

[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Faat.UserInterface.Common")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Faat.UserInterface")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Faat")]

namespace Faat.UserInterface.Common
{
	public class BulkTextEditor : TextEditor, INotifyPropertyChanged
	{
		public static DependencyProperty BulkTextProperty = DependencyProperty.Register("BulkText", typeof(string), typeof(BulkTextEditor), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, BulkTextChanged, null, true, UpdateSourceTrigger.Explicit));

		public string BulkText
		{
			get { return (string)GetValue(BulkTextProperty); }
			set { SetValue(BulkTextProperty, value); }
		}

		static readonly XTraceSource Trace = new XTraceSource("BulkTextEditor");

		// BulkText => Text
		static void BulkTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			Trace.Verbose("BulkTextChanged");
			var ed = (BulkTextEditor)dependencyObject;
			var txt = (string)e.NewValue;
			if (!string.Equals(txt, ed.Text, StringComparison.Ordinal))
			{
				Trace.Verbose("BulkTextChanged copy string to Text");
				// TODO replace to modifying document for undo stack support
				ed.Text = txt;
			}
		}

		// Text => BulkText
		protected override void OnTextChanged(EventArgs e)
		{
			Trace.Verbose("TextChanged copy string to BulkText");
			base.OnTextChanged(e);
			if (!string.Equals(BulkText, Text, StringComparison.Ordinal))
			{
				BulkText = Text;
				OnPropertyChanged("Text");
			}
		}

		protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			var be = GetBindingExpression(BulkTextProperty);
			if (be != null)
			{
				Trace.Verbose("PreviewLostKeyboardFocus UpdateSource");
				be.UpdateSource();
			}
			base.OnPreviewLostKeyboardFocus(e);
		}

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
	}
}