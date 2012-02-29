
using System;
using System.Windows;

using Obtics.Collections;

using XTrace;

namespace Faat.UserInterface
{
	partial class App
	{
		public static bool NeedRestart = true;
		public static bool UsePerformanceMode;

		[STAThread]
		static void Main()
		{
			UsePerformanceMode = Environment.GetCommandLineArgs().Any(x => x.ToLowerInvariant().TrimStart('/', '-') == "fast") != null;

			var app = new App();
			app.InitializeComponent();
			app.Run();
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			new StructuredTextFileXTraceListener("Faat UI", RelativePath.TempFolder);
			XTrace.XTrace.Information("Started");

			while (NeedRestart)
			{
				NeedRestart = false;
				MaintainResources();
				new MainWindow().ShowDialog();
			}

			Shutdown();
		}

		void MaintainResources()
		{
			if (UsePerformanceMode)
			{
				Resources.MergedDictionaries.Add((ResourceDictionary) LoadComponent(new Uri("Styles/PerformanceStyles.xaml", UriKind.Relative)));
			}
			else
			{
				Resources.MergedDictionaries.Add((ResourceDictionary) LoadComponent(new Uri("Styles/AppearenceStyles.xaml", UriKind.Relative)));
			}
		}
	}
}

