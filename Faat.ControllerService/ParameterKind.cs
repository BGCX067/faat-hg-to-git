using System.Linq;
using System.Collections.Generic;
using System;

namespace Faat.ControllerService
{
	public enum ParameterKind
	{
		// String,
		Numeric,
		InfoSize,
		TimeSpan,
		Custom, // register dynamic parameter
		// InfoRate,
	}
}