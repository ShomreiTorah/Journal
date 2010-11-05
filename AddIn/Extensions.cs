using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;
using System.Runtime.InteropServices;

namespace ShomreiTorah.Journal.AddIn {
	static class AddInExtensions {
		public static DocumentWindow Window(this IRibbonControl control) {
			if (control.Context == null) {
				if (Globals.ThisAddIn.Application.Windows.Count == 0)
					return null;
				try {
					return Globals.ThisAddIn.Application.ActiveWindow;
				} catch (COMException) { return null; }	//There is no active window
			}
			return (DocumentWindow)control.Context;
		}

		public static JournalPresentation Journal(this IRibbonControl control) {
			var window = control.Window();
			if (window == null || window.Presentation == null) return null;
			return Globals.ThisAddIn.GetJournal(window.Presentation);
		}

		public static AdShape CurrentAd(this IRibbonControl control) {
			var jp = control.Journal();
			if (jp == null) return null;

			var window = control.Window();
			var slide = (Slide)window.View.Slide;
			if (slide.AdType() == null) return null;
			if (slide.AdType().AdsPerPage == 1)
				return jp.GetAd(slide.Shapes.Placeholders[1]);

			if (window.Selection.Type != PpSelectionType.ppSelectionShapes
			 && window.Selection.Type != PpSelectionType.ppSelectionText)
				return null;
			return jp.GetAd(window.Selection.ShapeRange[1]);

		}
	}
}
