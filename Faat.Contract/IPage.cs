using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Faat.Core
{
	public interface IPage
	{
		ICollection<IPage> Children { get; }
		ICollection<IPage> Parents { get; }
		ICollection<IPage> Links { get; }

		string Name { get; set; }
		string Content { get; set; }
	}
}
