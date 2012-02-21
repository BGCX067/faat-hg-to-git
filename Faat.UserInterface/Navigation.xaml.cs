using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using Faat.Model;
using Faat.Storage;

using MySpring;

namespace Faat.UserInterface
{
	/// <summary>
	/// Interaction logic for Navigation.xaml
	/// </summary>
	public partial class Navigation
	{
		public Navigation()
		{
			InitializeComponent();
		}

//		private void MenuItemAddChildPageClick(object sender, RoutedEventArgs e)
//		{
//			var fe = (FrameworkElement)sender;
//			var page = (PageViewModel)fe.DataContext;
//			var storage = SpringContainer.Root.Get<IStorage>();
//			page.Page.Children.Add(new StringPage(storage) {Name = "New page"});
//		}

		private void TreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			// var tv = (TreeView)sender;
			var ctx = (MainWindowContext)DataContext;
			var pageViewModel = e.NewValue as PageViewModel;
			if (pageViewModel != null)
			{
				ctx.CurrentPage = pageViewModel;
			}
		}

//		void MenuItem_Cut_Click(object sender, RoutedEventArgs e)
//		{
//			var fe = (FrameworkElement)sender;
//			var page = (PageViewModel)fe.DataContext;
//			page.Cut();
//		}
//
//		void MenuItem_PasteChild_Click(object sender, RoutedEventArgs e)
//		{
//			var fe = (FrameworkElement)sender;
//			var page = (PageViewModel)fe.DataContext;
//			page.PasteChild();
//		}

		private void TreeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			var htr = VisualTreeHelper.HitTest(tv, e.GetPosition(tv));
			var element = (FrameworkElement)htr.VisualHit;

			if (element != null)
			{
				var currentPage = element.DataContext as PageViewModel;
				if (currentPage != null)
				{
					var tvi1 = GetVisualParent<TreeViewItem>(element);

					Dispatcher.BeginInvoke(new Action<TreeViewItem>(x =>
					{
						x.IsSelected = true;
					}), DispatcherPriority.ApplicationIdle, tvi1);

//					if (tvi1 != null)
//					{
//						var currentPage1 = (PageViewModel)tvi1.DataContext;
//
//						Debug.Assert(currentPage == currentPage1);
//
//						var tvi2 = GetVisualParent<TreeViewItem>(tvi1);
//						if (tvi2 != null)
//						{
//							var currentPage2 = (PageViewModel)tvi2.DataContext;
//
//							Debug.Assert(currentPage1 != currentPage2);
//
//							currentPage1.SelectedParent = currentPage2;
//						}
//					}
				}
			}
		}

		T GetVisualParent<T>(DependencyObject current) where T : DependencyObject
		{
			if (current == null)
			{
				return null;
			}
			while ((current = GetParent(current)) != null)
			{
				var t = current as T;
				if (t != null)
				{
					return t;
				}
			}
			return null;
		}

		DependencyObject GetParent(DependencyObject current)
		{
			return VisualTreeHelper.GetParent(current);
		}

	}
}
