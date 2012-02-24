using System.Linq;
using System.Collections.Generic;
using System;

using MyUtils;

namespace Faat.Agent
{
	public class FaatAgentExecutor
	{
		public ExecutionResult RunPage(string serverAddress, string pageIdentifier, string resultDataIdentifier)
		{
			using (var dom = new SeparateDomain())
			{
				return (ExecutionResult)dom.Execute<FaatAgent, object>(x => x.RunPage(serverAddress, pageIdentifier, resultDataIdentifier));
			}
		}
	}
}