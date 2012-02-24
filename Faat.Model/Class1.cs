using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Faat.Model
{
	public enum ExecutionResultState
	{
		Unknown,
		Passed,
		Failed,
		Warning,
		Exception,
		Timeout,
		BadTest,
	}
}
