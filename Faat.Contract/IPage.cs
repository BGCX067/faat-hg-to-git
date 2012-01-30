using System;
using System.Collections.Generic;

namespace Faat
{
	public interface IPage
	{
		ICollection<IPage> Children { get; }
		ICollection<IPage> Parents { get; }
		ICollection<IPage> Links { get; }

		string Identity { get; }

		string Name { get; set; }
		string Content { get; set; }
	}
}
