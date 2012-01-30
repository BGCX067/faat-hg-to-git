using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;

using Faat.Storage.FileSystem;

namespace Faat.Service.Console
{
	class Program
	{
		static void Main()
		{
			using(var host = new ServiceHost(new FaatService(new FileSystemStorage()), new Uri("http://localhost/Faat")))
			{
				host.Open();
				System.Console.WriteLine("Ready.");
				System.Console.ReadLine();
			}
		}
	}
}
