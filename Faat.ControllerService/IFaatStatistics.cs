using System.Linq;
using System.Collections.Generic;
using System;

namespace Faat.ControllerService
{
	public interface IFaatStatistics
	{
		IEnumerable<string> ParameterNames { get; }
		string GetParameterValue(string parameterName);
	}
}