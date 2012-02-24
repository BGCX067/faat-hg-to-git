using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Faat.Parser;

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

using MyUtils;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;

namespace Faat.UserInterface
{
	/// <summary>
	/// Interaction logic for PageBrowser.xaml
	/// </summary>
	public partial class PageBrowser
	{
		public PageBrowser()
		{
			InitializeComponent();
			//DataContext = new CurrentPage();

			//HighlightingManager.Instance.RegisterHighlighting("Faab", new[] {".faab"}, new FaabHighlighting());

			_xmlPreview.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".xml");
			_editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".faab");

			_foldingManager = FoldingManager.Install(_xmlPreview.TextArea);

			_xmlPreview.DataContextChanged -= ChangeDataContext;
			_xmlPreview.DataContextChanged += ChangeDataContext;
			_xmlPreview.TextChanged += EditorTextChanged;
			_editor.DataContextChanged -= ChangeDataContext;
			_editor.DataContextChanged += ChangeDataContext;
			_editor.TextChanged += EditorTextChanged;

			_xmlPreview.Attached().FoldingStrategy = _xmlFoldingStrategy;

		}

		readonly XmlFoldingStrategy _xmlFoldingStrategy = new XmlFoldingStrategy();
		readonly FoldingManager _foldingManager;

		void ChangeDataContext(object sender, DependencyPropertyChangedEventArgs e)
		{
			var ed = (TextEditor)sender;
			ed.Text = (string)ed.DataContext;
		}

		//[DebuggerStepThrough]
		private void EditorTextChanged(object sender, EventArgs e)
		{
			var ed = (TextEditor)sender;
			try
			{
				XmlFoldingStrategy st = ed.Attached().FoldingStrategy;
				if (st != null)
				{
					st.UpdateFoldings(_foldingManager, ed.Document);
				}
			}
			catch (Exception ex)
			{
				XTrace.XTrace.Exception(ex);
			}
		}
	}

	class FaabHighlightingDefinition : IHighlightingDefinition
	{
		public FaabHighlightingDefinition()
		{
			_lineRuleSet = new HighlightingRuleSet()
			{
				Name = "LineRuleSet",
			};
			_lineRuleSet.Spans.Add(new HighlightingSpan
			{
				
			});
		}

		HighlightingRuleSet _lineRuleSet;

		public HighlightingRuleSet GetNamedRuleSet(string name)
		{
			return _lineRuleSet;
		}

		public HighlightingColor GetNamedColor(string name)
		{
			HighlightingColor color;
			return _namedColors.TryGetValue(name, out color) ? color : null;
		}

		public string Name
		{
			get { return "Faab"; }
		}

		public HighlightingRuleSet MainRuleSet
		{
			get { return _lineRuleSet; }
		}

		readonly Dictionary<string, HighlightingColor> _namedColors = new Dictionary<string, HighlightingColor>
		{
			{
				"Line",
				new HighlightingColor {
					Background = new MyHighlightingBrush(Colors.Red),
					Name = "Line",
				}
			},
		};

		public IEnumerable<HighlightingColor> NamedHighlightingColors
		{
			get { return _namedColors.Values; }
		}
	}

	class MyHighlightingBrush : HighlightingBrush
	{
		readonly Color _color;

		public MyHighlightingBrush(Color color)
		{
			_color = color;
		}

		public override Brush GetBrush(ITextRunConstructionContext context)
		{
			return new SolidColorBrush(_color);
		}
	}

	class FaatFoldingStrategy : AbstractFoldingStrategy
	{
		readonly PageParser _parser = new PageParser();

		public override IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
		{
			try
			{
				var markers = new List<NewFolding>();
				var result = _parser.Tokenize(document.Text);

				firstErrorOffset = -1;
				return markers;
			}
			catch (Exception ex)
			{
				XTrace.XTrace.Exception(ex);
				firstErrorOffset = 0;
				return Enumerable.Empty<NewFolding>();
			}
		}
	}
}
