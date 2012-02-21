using System.Linq;
using System.Collections.Generic;
using System;

namespace Faat.ControllerService
{
	public static class FaatStatisticsWriterExt
	{
		public static void NumericIncrement(this IFaatStatisticsWriter writer, string parameterName)
		{
			writer.NumericSum(parameterName, 1);
		}
	}
}