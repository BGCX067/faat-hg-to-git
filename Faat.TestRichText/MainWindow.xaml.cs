using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Faat.TestRichText
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			rtb.AppendText(@"
		public MainWindow()
		{
			InitializeComponent();
"/*.Replace("\r\n","\r")*/);
			// var q = rtb.SelectionCharOffset
		}

		private void rtb_PreviewKeyDown(object sender, KeyEventArgs e)
		{
//			var rtb = (RichTextBox)sender;
//       if (e.Key == Key.Enter)
//       {
//         var newPointer = rtb.Selection.Start.InsertLineBreak();
//			rtb.Selection.Select(newPointer, newPointer);
//         e.Handled = true;
//       }
		}
	}
}
