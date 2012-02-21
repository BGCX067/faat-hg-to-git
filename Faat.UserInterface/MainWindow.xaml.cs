using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Faat.UserInterface
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();
			DataContext = Context = new MainWindowContext();

			ConnectInBackground();

			_focusManager.Register(navi);
			_focusManager.Register(page);

			Loaded += MainWindow_Loaded;
		}

		void ConnectInBackground()
		{
			Context.Connection.Connect();
		}

		void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			//((Storyboard)Resources["global_BackgroundAnimation"]).Begin();
//			var colors = new List<Color>();
//			for (int i = 0; ; i++)
//			{
//				try
//				{
//					colors.Add((Color)Resources["global_bg" + i]);
//				}
//				catch
//				{
//					break;
//				}
//			}
//
//			var story = new Storyboard {RepeatBehavior = RepeatBehavior.Forever};
//			var overall = default(TimeSpan);
//			var duration = TimeSpan.FromSeconds(3);
//			foreach (var color in colors)
//			{
//				Anim(story, overall, duration, color, "navi");
//				Anim(story, overall, duration, color, "page");
//				overall += duration;
//			}
//
//			story.Begin();
		}

//		static void Anim(Storyboard story, TimeSpan overall, TimeSpan duration, Color color, string element)
//		{
//			var anim = new ColorAnimation(color, duration) {BeginTime = overall};
//			anim.SetValue(Storyboard.TargetNameProperty, element);
//			anim.SetValue(Storyboard.TargetPropertyProperty, new DependencyPropertyConverter().ConvertFromString("(Control.Background).(LinearGradientBrush.GradientStops).[1].Color"));
//			story.Children.Add(anim);
//		}

		readonly MainWindowContext Context;

		readonly MyFocusManager _focusManager = new MyFocusManager();

		private void ToggleCurrentView_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			_focusManager.Toggle();
		}

	}

	public class MyFocusManager
	{
		readonly List<FrameworkElement> _elements = new List<FrameworkElement>();

		public void Register(FrameworkElement element)
		{
			_elements.Add(element);
		}

		public void Toggle()
		{
			var index = _elements.IndexOf(_elements.Find(x => x.IsKeyboardFocused || x.IsFocused || x.IsKeyboardFocusWithin));
			XTrace.XTrace.Verbose("Focus", "Current index {0}", index);
			index++;
			if (index >= _elements.Count)
			{
				index = 0;
			}
			var next = _elements[index];

			XTrace.XTrace.Verbose("Focus", "Set focus to {0}", next);
			next.Focus();
			Keyboard.Focus(next);
			XTrace.XTrace.Error(!next.IsKeyboardFocusWithin, "Focus", "Can not set keyboard focus - not IsKeyboardFocused");
		}
	}

	static class MyNavigationCommands
	{
		static RoutedUICommand _toggleCurrentView = new RoutedUICommand("ToggleCurrentView", "ToggleCurrentView", typeof(MyNavigationCommands), new InputGestureCollection {new KeyGesture(Key.F6)});
		public static RoutedUICommand ToggleCurrentView
		{
			get { return _toggleCurrentView; }
			set { _toggleCurrentView = value; }
		}
	}
}
