using System.Linq;
using System.Collections.Generic;
using System;

using Faat.Parser.Ast;

namespace Faat.Parser
{
	public class ParsingReport
	{
		public Block PageTokens;
		public List<ParsingError> ParsingErrors = new List<ParsingError>();
		public int Hash { get; set; }
	}
}