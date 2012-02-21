using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

using MyUtils;

namespace UnitTests.Executor
{
	class MyFixture
	{
		void HelloWorld()
		{
			XTrace.XTrace.Important("HelloWorld");
			File.WriteAllText(AgentTests.TempFlagFileName, "");
		}
	}

	class Calculator
	{
		void HelloWorld()
		{
			MessageBox.Show("HelloWorld");
		}

		int Combine_and_(int a, int b)
		{
			// MessageBox.Show("Combine_and_ {0} {1}".Arg(a,b));
			return a + b;
		}
	}
}
