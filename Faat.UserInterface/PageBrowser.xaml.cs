using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Faat.Parser;
using Faat.Storage;
using Faat.UserInterface.Common;
using Faat.UserInterface.Editor;

using MySpring;

using MyUtils;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;

using XTrace;

namespace Faat.UserInterface
{
	/// <summary>
	/// Interaction logic for PageBrowser.xaml
	/// </summary>
	public partial class PageBrowser
	{
		PageViewModel PageViewModel
		{
			get { return (PageViewModel)DataContext; }
		}

		public PageBrowser()
		{
			InitializeComponent();

			_xmlPreview.DataContextChanged -= ChangeDataContext;
			_xmlPreview.DataContextChanged += ChangeDataContext;
			_xmlPreview.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".xml");
			_xmlPreview.TextChanged += EditorTextChanged;
			_xmlPreview.Attached().FoldingStrategy = new XmlFoldingStrategy();
			_xmlPreview.Attached().FoldingManager = FoldingManager.Install(_xmlPreview.TextArea);

			_editor.Attached().PageParserState = new PageParserState(
				ValueProvider.Create(() => _editor.Text/*, null, _editor, "Text"*/),
				ValueProvider.Create(() =>
				{
					var pageVm = PageViewModel;
					if(pageVm!=null)
					{
						return PageViewModel.Storage.GetData(Const.GlobalPageResultGraphPrefix + PageViewModel.Page.Identity);
					}
					return null;
				}));
			_editor.TextChanged += EditorTextChanged;
			_editor.TextArea.TextView.LineTransformers.Add(new Highlighter(_editor.Attached().PageParserState));
			_editor.Options.AllowScrollBelowDocument = true;
			_editor.Options.CutCopyWholeLine = true;
			_editor.Options.EnableHyperlinks = true;
			_editor.Options.IndentationSize = 2;
			_editor.Options.ShowTabs = true;
			_editor.WordWrap = true;
		}

		static readonly XTraceSource Trace = new XTraceSource("PageBrowser");

		void ChangeDataContext(object sender, DependencyPropertyChangedEventArgs e)
		{
			var ed = sender as TextEditor;
			if (ed != null)
			{
				ed.Text = (ed.DataContext as string) ?? "";
			}
		}

		private static void EditorTextChanged(object sender, EventArgs e)
		{
			var ed = sender as TextEditor;
			if (ed != null)
			{
				XmlFoldingStrategy fs = ed.Attached().FoldingStrategy;
				FoldingManager fm = ed.Attached().FoldingManager;
				if (fs != null && fm != null)
				{
					fs.UpdateFoldings(fm, ed.Document);
				}
			}
		}

	}
}
