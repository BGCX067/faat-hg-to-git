using System.Linq;
using System;
using System.Collections.Generic;
using System.ServiceModel;

using Faat.Storage;
using Faat.Storage.FileSystem;

using MySpring;

namespace Faat.ControllerService.Console
{
	static class EntryPoint
	{
		static void Main()
		{
			new XTrace.ConsoleXTraceListener();
			new XTrace.StructuredTextFileXTraceListener("Faat Server", XTrace.RelativePath.TempFolder);

			var spring = SpringContainer.Root;
			spring.Add<IStorage, FileSystemStorage>();
			using (var host = new ServiceHost(spring.Get<IFaatService>(), new[]
			{
				new Uri("http://localhost/Faat"),
				// new Uri("http://localhost/Design_Time_Addresses/FaatTest/"),
				new Uri("net.tcp://localhost:4444/Faat"),
			}))
			{
				host.Open();
				System.Console.WriteLine("Ready. Press any key to shutdown.");
				System.Console.ReadKey();
			}
		}
	}
}
