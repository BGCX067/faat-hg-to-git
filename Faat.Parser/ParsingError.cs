using System.Linq;
using System.Collections.Generic;
using System;

namespace Faat.Parser
{
	public class ParsingError
	{
		public string ErrorDescription;
		public int FromLine;
		public int FromColumn;
		public int ToLine;
		public int ToColumn;
	}
}