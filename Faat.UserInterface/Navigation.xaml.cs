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

using MyUtils;

using XTrace;

namespace Faat.UserInterface
{
	/// <summary>
	/// Interaction logic for Navigation.xaml
	/// </summary>
	public partial class Navigation
	{
		public static DependencyProperty IsDropTragetProperty = DependencyProperty.RegisterAttached("IsDropTraget", typeof(bool), typeof(Navigation));

		public bool IsDropTraget
		{
			get { return (bool)GetValue(IsDropTragetProperty); }
			set { SetValue(IsDropTragetProperty, value); }
		}

		PageViewModel DropTarget
		{
			get { return _dropTarget; }
			set
			{
				if (_dropTarget != value)
				{
					var old = _dropTarget;
					_dropTarget = value;

					if (old != null)
					{
						old.SetValue(IsDropTragetProperty, false);
					}
					if (value != null)
					{
						value.SetValue(IsDropTragetProperty, true);
					}
				}
			}
		}

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

		private void Drag_DragOver(object sender, DragEventArgs e)
		{
			NavDrag.Verbose("dnd", e.Effects + " " + e.Data);
			var element = sender as FrameworkElement;

			var eff = DragDropEffects.None;

			if (element != null)
			{
				var currentPage = element.DataContext as PageViewModel;
				if (currentPage != null)
				{
					DropTarget = currentPage;
					NavDrag.Verbose("PotentialDropTarget", DropTarget);
					eff = GetDropEffects(_draggingModel, DropTarget, e);
				}
			}

			e.Effects = eff;

			NavDrag.Verbose("ResultEffect", e.Effects);
			e.Handled = true;
		}

		DragDropEffects GetDropEffects(PageViewModel source, PageViewModel target, DragEventArgs e)
		{
			if (source == null)
			{
				return DragDropEffects.None;
			}
			if (target == null)
			{
				return DragDropEffects.None;
			}
			if (source == target)
			{
				return DragDropEffects.None;
			}

			if (e.KeyStates.HasFlag(DragDropKeyStates.AltKey))
			{
				return DragDropEffects.Move;
			}
			return DragDropEffects.Copy;
		}

		private void st_Drop(object sender, DragEventArgs e)
		{
			NavDrag.Verbose("dnd", e.Effects + " " + e.Data);
			// MessageBox.Show("{2}\r\n{0}\r\nto\r\n{1}".Arg(_draggingModel, _dropTarget, e.Effects));
			var eff = GetDropEffects(_draggingModel, DropTarget, e);
			if (eff == DragDropEffects.Copy)
			{
				_draggingModel.Copy();
				DropTarget.PasteChild();
			}
			else if (eff == DragDropEffects.Move)
			{
				if (DropTarget != null)
				{
					_draggingModel.Cut();
					DropTarget.PasteChild();
				}
			}
			DropTarget = null;
			// erease clipboard
			PageViewModel.Copy(null);
		}

		static readonly XTraceSource NavDrag = new XTraceSource("NavDrag");

		Point _startPoint;
		bool _isDragging;
		PageViewModel _draggingModel;
		PageViewModel _dropTarget;

		private void Drag_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var element = sender as FrameworkElement;

			if (element != null)
			{
				var currentPage = element.DataContext as PageViewModel;
				if (currentPage != null)
				{
					_startPoint = e.GetPosition(null);
					_draggingModel = currentPage;
					NavDrag.Verbose("DragSource", _draggingModel);
				}
			}
		}

		private void Drag_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed && !_isDragging)
			{
				var position = e.GetPosition(null);

				if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
					 Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
				{
					_isDragging = true;
					NavDrag.Verbose("StartDragging");
					var obj = _draggingModel;
					var data = new DataObject(obj.GetType(), obj);
					DragDrop.DoDragDrop(tv, data, DragDropEffects.Copy | DragDropEffects.Move);
					NavDrag.Verbose("EndDragging");
					_isDragging = false;
				}
			}   
		}

//		private void Drag_GiveFeedback(object sender, GiveFeedbackEventArgs e)
//		{
//			//e.UseDefaultCursors = true;
//			if (e.Effects.HasFlag(DragDropEffects.Copy))
//			{
//				Mouse.SetCursor(Cursors.Cross);
//			}
//			else if (e.Effects.HasFlag(DragDropEffects.Move))
//			{
//				Mouse.SetCursor(Cursors.Pen);
//			}
//			else
//			{
//				Mouse.SetCursor(Cursors.No);
//			}
//			e.Handled = true;
//		}

	}
}
