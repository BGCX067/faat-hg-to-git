using System.Linq;
using System.Collections.Generic;
using System;

namespace Faat.Parser
{
	public class TextLine
	{
		public string String;
		public int StartOffset;
		public int EndOffset;

		public TextLine()
		{

		}

		public TextLine(string s, int startOffset, int endOffset)
		{
			String = s;
			StartOffset = startOffset;
			EndOffset = endOffset;
		}

		public int Length
		{
			get { return EndOffset - StartOffset; }
		}

		public static implicit operator string(TextLine line)
		{
			return line.String;
		}
	}
}