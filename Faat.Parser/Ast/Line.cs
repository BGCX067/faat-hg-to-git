using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Windows;

using Faat.Model;

namespace Faat.Parser.Ast
{
	[DefaultProperty("TextLine")]
	[Serializable]
	public class Line
	{
		[DefaultValue(null)]
		public string String { get; set; }

		public int StartOffset { get; set; }
		public int EndOffset { get; set; }

		public int Length
		{
			get { return EndOffset - StartOffset; }
		}

		public override string ToString()
		{
			return "L: " + String;
		}

		public override int GetHashCode()
		{
			return String.GetHashCodeOrNull();
		}

		[DefaultValue(null)]
		public ExecutionResultState? ResultState { get; set; }
	}

	public static class Ext
	{
		public static int GetHashCodeOrNull(this string str)
		{
			return str == null ? 0 : str.GetHashCode();
		}
	}

}