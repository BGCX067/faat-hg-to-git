using System.Linq;
using System.Collections.Generic;
using System;

using MySpring;

namespace Faat.ControllerService
{
	[DefaultImpl(typeof(FaatStatistics))]
	public interface IFaatStatisticsWriter : IFaatStatistics
	{
		void NumericSum(string parameterName, decimal value, ParameterKind kind = ParameterKind.Numeric);
		void NumericMax(string parameterName, decimal value, ParameterKind kind = ParameterKind.Numeric);
		void NumericAverrage(string parameterName, decimal value, ParameterKind kind = ParameterKind.Numeric);
		void RegisterDynamicParameter(string parameterName, Func<IFaatStatistics, string> factory);
	}
}