using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;

using XTrace;

namespace Faat.UserInterface
{
	public partial class App
	{
		public static bool NeedRestart = true;
		public static bool UsePerformanceMode;

		protected override void OnStartup(StartupEventArgs e)
		{
			UsePerformanceMode = Environment.GetCommandLineArgs().Any(x => x.ToLowerInvariant().TrimStart('/', '-') == "fast");

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
			Resources.MergedDictionaries.Clear();

			if (UsePerformanceMode)
			{
				Resources.MergedDictionaries.Add((ResourceDictionary)LoadComponent(new Uri("Styles/PerformanceStyles.xaml", UriKind.Relative)));
			}
			else
			{
				Resources.MergedDictionaries.Add((ResourceDictionary)LoadComponent(new Uri("Styles/AppearenceStyles.xaml", UriKind.Relative)));
			}
		}
	}
}
