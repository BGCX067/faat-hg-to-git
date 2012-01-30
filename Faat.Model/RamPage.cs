using System.Linq;
using System.Collections.Generic;
using System;

namespace Faat.Model
{
	public class RamPage : IPage
	{
		public RamPage()
		{
			_children = new SyncOppositeList<IPage>(this, x => x.Parents);
			_parents = new SyncOppositeList<IPage>(this, x => x.Children);
			_links = new SyncOppositeList<IPage>(this, x => x.Links);
		}

		readonly ICollection<IPage> _children;
		public ICollection<IPage> Children
		{
			get { return _children; }
		}

		readonly ICollection<IPage> _parents;
		public ICollection<IPage> Parents
		{
			get { return _parents; }
		}

		readonly ICollection<IPage> _links;
		public ICollection<IPage> Links
		{
			get { return _links; }
		}

		public string Identity { get; set; }

		public string Name { get; set; }
		public string Content { get; set; }
	}
}