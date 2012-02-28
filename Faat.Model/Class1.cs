using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Faat.Model
{
	public enum ExecutionResultState
	{
		Unknown, // result is not initialized
		Passed,
		Failed,
		Warning,
		Exception,
		Timeout,
		BadTest,
		ActionUnknown, // script action not found
	}
}
