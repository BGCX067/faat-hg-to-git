using System;
using System.Collections.Generic;
using System.Linq;

namespace Faat.Contract
{
	public interface IPage
	{
		ICollection<IPage> Children { get; }
		ICollection<IPage> Parents { get; }
		ICollection<IPage> Links { get; }

		string Identity { get; set; }
		string Name { get; set; }
		string Content { get; set; }
	}
}
