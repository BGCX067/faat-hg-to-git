using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;

using XTrace;

namespace Faat.UserInterface
{
	public partial class App
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			new StructuredTextFileXTraceListener("Faat UI", RelativePath.TempFolder);
			XTrace.XTrace.Information("Started");
			base.OnStartup(e);
		}
	}
}
