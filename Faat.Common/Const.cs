using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Faat
{
	public static class Const
	{
		/// <summary>
		/// Root page identity. Clients should use it as index.htm in webserver
		/// </summary>
		public static string RootPage = new Guid("{DC4D51EF-EA4F-4D13-B1E7-BC162683CB37}").ToString("N");

		/// <summary>
		/// Dynamic page for current performance counters
		/// </summary>
		public static string StatisticsPage = "sys.statistics";

		/// <summary>
		/// Page for linking pages that is going to be removed
		/// </summary>
		public static string RecycleBinPage = new Guid("{AAA0B7F5-B621-412B-AF8B-9C2EEF647C1D}").ToString("N");

		/// <summary>
		/// Prefix for page identifier by witch page execution result is stored (Block page xaml with parsed and stated elements)
		/// </summary>
		public static string GlobalPageResultGraphPrefix = "ResultGraph_";

		/// <summary>
		/// Prefix for page identifier by witch page execution result is stored (ExecutionResultState enum value)
		/// </summary>
		public static string GlobalPageResultSimplePrefix = "ResultSimple_";
	}

}
