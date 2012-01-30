using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;

using Faat.Core;

namespace Faat.Host
{
	public class FilePage : StringPage
	{
		readonly string _fileName;
		Cache<StreamReader> _stream;

		/// <summary>
		/// Create new page
		/// </summary>
		/// <param name="directory"></param>
		/// <param name="pageName"></param>
		public static FilePage Create(string directory, string pageName)
		{
			string path;
			return Create(directory, pageName, out path);
		}

		/// <summary>
		/// Create new page
		/// </summary>
		/// <param name="directory"></param>
		/// <param name="pageName"></param>
		/// <param name="path"> </param>
		public static FilePage Create(string directory, string pageName, out string path)
		{
			var file = Path.Combine(directory, Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(directory);
			File.WriteAllText(file, pageName);
			path = file;
			return new FilePage(file);
		}

		public FilePage(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException("filePath");
			}
			if (!Path.IsPathRooted(filePath))
			{
				throw new ArgumentException("Path is not rooted", "filePath");
			}
			Load(File.ReadAllText(filePath));
		}
	}
}