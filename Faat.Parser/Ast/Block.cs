using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Markup;

[assembly: XmlnsDefinition("Faat1", "Faat.Parser.Ast")]

namespace Faat.Parser.Ast
{
	[DefaultProperty("Lines")]
	[ContentProperty("Lines")]
	[Serializable]
	public class Block : Line
	{
		public Block()
		{
			_lines.CollectionChanged += LinesCollectionChanged;
		}

		void LinesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			XTrace.XTrace.Verbose("Block Lines Changed", "{0}, {1}, new: [{2}]", Name, e.Action, (e.NewItems ?? new object[0]).Cast<object>().FirstOrDefault());
		}

		public string Name { get; set; }

		// todo remove observing and logging
		readonly ObservableCollection<Line> _lines = new ObservableCollection<Line>();
		public bool IsPageWide;

		[DefaultValue(null)]
		public string Param { get; set; }

		public IList<Line> Lines
		{
			get { return _lines; }
		}

		public override string ToString()
		{
			return "B: " + Name;
		}

		public override int GetHashCode()
		{
			int hash = 0;
			foreach (var item in Lines.Select(x => x.GetHashCode())
				.Concat(new[]
				{
					String.GetHashCodeOrNull(),
					Param.GetHashCodeOrNull()
				}))
			{
				unchecked
				{
					hash *= 397;
					hash ^= item.GetHashCode();
				}
			}
			return hash;
		}
	}

}
