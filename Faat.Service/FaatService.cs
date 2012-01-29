using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Faat.Service
{
	// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
	public class FaatService : IFaatService
	{
		readonly string _directory;

		public FaatService()
		{
			_directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Faat", "ServerRepo");
		}

		string FileName(string identity)
		{
			if (identity.Any(x => !char.IsLetterOrDigit(x)))
			{
				throw new ArgumentException("Page identity can contain only letters and numbers", "identity");
			}
			var path = Path.Combine(_directory, identity);
			return path;
		}

		public string GetPage(string identity)
		{
			var file = FileName(identity);
			if (File.Exists(file))
			{
				return File.ReadAllText(file);
			}
			return null;
		}

		public void SetPage(string identity, string content)
		{
			// check sequence
			File.WriteAllText(FileName(identity), content);
		}
	}
}
