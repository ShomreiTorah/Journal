using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;
using ShomreiTorah.Common;
using ShomreiTorah.Data;
using ShomreiTorah.Journal.Properties;
using ShomreiTorah.WinForms;

namespace ShomreiTorah.Journal.AddIn {
	[ComVisible(true)]
	public class JournalRibbon : IRibbonExtensibility {
		private IRibbonUI ribbon;

		#region IRibbonExtensibility
		public string GetCustomUI(string RibbonID) {
			return GetResourceText("ShomreiTorah.Journal.AddIn.JournalRibbon.xml");
		}
		private static string GetResourceText(string resourceName) {
			using (var stream = typeof(JournalRibbon).Assembly.GetManifestResourceStream(resourceName))
			using (var reader = new StreamReader(stream))
				return reader.ReadToEnd();
		}
		#endregion

		public void OnLoad(IRibbonUI ribbonUI) {
			this.ribbon = ribbonUI;
			Globals.ThisAddIn.Application.WindowSelectionChange += delegate { ribbon.Invalidate(); };
			Globals.ThisAddIn.Application.WindowDeactivate += delegate { ribbon.Invalidate(); };

			StatsManager.Changed += delegate { ribbonUI.Invalidate(); };
		}
		public Bitmap LoadImage(string name) {
			return (Bitmap)Resources.ResourceManager.GetObject(name);
		}

		#region Boolean Callbacks
		public bool IsPresentation(IRibbonControl control) {
			var window = control.Window();
			return window != null && window.Presentation != null;
		}
		public bool IsJournal(IRibbonControl control) { return control.Journal() != null; }
		public bool IsAdSelected(IRibbonControl control) { return control.CurrentAd() != null; }
		#endregion

		#region Stats callbacks
		public string GetTotalPledged(IRibbonControl control) {
			if (!Program.WasInitialized) return "(N/A)";
			var journal = control.Journal();
			if (journal == null) return "(N/A)";
			return Program.Current.Statistics[journal.Year].TotalPledged.ToString("c", CultureInfo.CurrentCulture);
		}
		public string GetTotalPaid(IRibbonControl control) {
			if (!Program.WasInitialized) return "(N/A)";
			var journal = control.Journal();
			if (journal == null) return "(N/A)";
			return Program.Current.Statistics[journal.Year].TotalPaid.ToString("c", CultureInfo.CurrentCulture);
		}
		public string GetAdCount(IRibbonControl control) {
			var journal = control.Journal();
			return journal == null ? "(N/A)" : journal.Ads.Count.ToString(CultureInfo.CurrentCulture);
		}
		public string GetFamilySeats(IRibbonControl control) {
			if (!Program.WasInitialized) return "(N/A)";
			var journal = control.Journal();
			if (journal == null) return "(N/A)";
			return Program.Current.Statistics[journal.Year].FamilySeats.ToString("n0", CultureInfo.CurrentCulture);
		}
		public string GetMensSeats(IRibbonControl control) {
			if (!Program.WasInitialized) return "(N/A)";
			var journal = control.Journal();
			if (journal == null) return "(N/A)";
			return Program.Current.Statistics[journal.Year].MensSeats.ToString("n0", CultureInfo.CurrentCulture);
		}
		public string GetWomensSeats(IRibbonControl control) {
			if (!Program.WasInitialized) return "(N/A)";
			var journal = control.Journal();
			if (journal == null) return "(N/A)";
			return Program.Current.Statistics[journal.Year].WomensSeats.ToString("n0", CultureInfo.CurrentCulture);
		}
		public string GetMensSeatsCaption(IRibbonControl control) {
			if (!Program.WasInitialized) return "Men";
			return MelaveMalkaSeat.MensSeatsCaption;
		}
		public string GetWomensSeatsCaption(IRibbonControl control) {
			if (!Program.WasInitialized) return "Women";
			return MelaveMalkaSeat.WomensSeatsCaption;
		}
		#endregion

		public void ShowProperties(IRibbonControl control) {
			var window = control.Window();
			Globals.ThisAddIn.ShowProperties(window.Presentation);
		}
		public void ShowDetailPane(IRibbonControl control) {
			var window = control.Window();
			var jp = control.Journal();
			// CustomTaskPanes cannot be reused; I need to create a new one.
			Globals.ThisAddIn.CustomTaskPanes.Add(new AdPane(jp), "Ad Details", jp.Presentation.Windows[1]).Visible = true;
		}
		public void ShowWarningsForm(IRibbonControl control) {
			new Forms.WarningsForm(control.Journal()).Show(Globals.ThisAddIn.Application.Window());
		}
		public void ShowCharts(IRibbonControl control) { new Forms.ChartsForm(control.Journal().Year).Show(Globals.ThisAddIn.Application.Window()); }
		public void ShowGrid(IRibbonControl control) { new Forms.AdsGridForm(control.Journal()).Show(Globals.ThisAddIn.Application.Window()); }

		public void SaveDB(IRibbonControl control) { Program.Current.SaveDatabase(); }
		public void RefreshDB(IRibbonControl control) {
			SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
			Program.Current.RefreshDatabase();
		}

		public void SavePdfTypes(IRibbonControl control) {
			using (var dialog = new FolderBrowserDialog {
				Description = "Export PDFs by ad type",
				ShowNewFolderButton = true
			}) {
				if (dialog.ShowDialog(new ArbitraryWindow((IntPtr)control.Window().HWND)) == DialogResult.Cancel)
					return;
				var ranges = new List<PageRange>();
				var presentation = control.Journal().Presentation;
				for (int i = 1; i <= presentation.Slides.Count; i++) {
					var currentType = presentation.Slides[i].CustomLayout.Name;
					if (ranges.Count == 0 || ranges.Last().Type != currentType)
						ranges.Add(new PageRange { Type = currentType, Start = i, End = i });
					else
						ranges.Last().End = i;
				}

				for (int i = 0; i < ranges.Count; i++) {
					var range = ranges[i];
					presentation.ExportAsFixedFormat(
						Path.Combine(dialog.SelectedPath, $"{i:00} - {range.Type} "
														+ (range.Start == range.End ? $"(Page {range.Start}).pdf" : $"(Pages {range.Start} - {range.End}).pdf")),
						PpFixedFormatType.ppFixedFormatTypePDF, PpFixedFormatIntent.ppFixedFormatIntentPrint,
						PrintRange: presentation.PrintOptions.Ranges.Add(range.Start, range.End),
						RangeType: PpPrintRangeType.ppPrintSlideRange);
				}
			}
		}
		class PageRange {
			public string Type { get; set; }
			public int Start { get; set; }
			public int End { get; set; }
		}

		public void SavePdf(IRibbonControl control) {
			using (var dialog = new SaveFileDialog {
				Filter = "PDF Files (*.pdf)|*.pdf",
				FileName = Path.ChangeExtension(control.Journal().Presentation.FullName, ".pdf"),
				Title = "Export PDF"
			}) {
				if (dialog.ShowDialog(new ArbitraryWindow((IntPtr)control.Window().HWND)) == DialogResult.Cancel)
					return;
				control.Journal().Presentation.ExportAsFixedFormat(dialog.FileName,
					PpFixedFormatType.ppFixedFormatTypePDF, PpFixedFormatIntent.ppFixedFormatIntentPrint,
					RangeType: control.Id == "SavePdfSlide" ? PpPrintRangeType.ppPrintCurrent : PpPrintRangeType.ppPrintAll);
			}
		}

		#region AdType Callbacks
		public int GetAdTypeCount(IRibbonControl control) { return Names.AdTypes.Count; }

		static int WarpIndex(int index) {
			// Converts 0,1,2,  3,4,5,  6,7
			// to       0,3,6,  1,4,7,  2,5
			var columnSize = Names.AdTypes.Count / 3;
			var tallerColumns = Names.AdTypes.Count % 3;
			return columnSize * (index % 3) // Add the number of items in the preceding columns.
				 + Math.Min(tallerColumns, (index % 3))	// Add one for each taller preceding column.
				 + index / 3;
		}

		public string GetAdTypeLabel(IRibbonControl control, int index) { return Names.AdTypes[WarpIndex(index)].Name; }
		public string GetAdTypeId(IRibbonControl control, int index) { return "Insert" + Names.AdTypes[WarpIndex(index)].Name; }

		public void InsertAd(IRibbonControl control, string selectedId, int selectedIndex) {
			var jp = control.Journal();

			if (!jp.ConfirmModification())
				return;

			var typeName = selectedId.Substring("Insert".Length);
			jp.CreateAd(Names.AdTypes.First(t => t.Name == typeName)).Shape.ForceSelect();
		}
		#endregion

		public void InsertSpecialPage(IRibbonControl control) {
			control.Window().View.Slide = control.Window().Presentation.Slides.Add(1, PpSlideLayout.ppLayoutBlank);
		}
		public void DeleteAd(IRibbonControl control) {
			string message;
			var ad = control.CurrentAd();
			if (ad == null) return;

			if (!control.Journal().ConfirmModification())
				return;

			if (ad.Row.Payments.Any())
				message = String.Format(CultureInfo.CurrentCulture, "Are you sure you want to delete this ad?\r\nThe ad's {0:c} in payments will not be deleted.\r\nYou should probably delete them first.",
										ad.Row.Payments.Sum(p => p.Amount));
			else if (ad.Row.Pledges.Any())
				message = String.Format(CultureInfo.CurrentCulture, "Are you sure you want to delete this ad?\r\nThe ad's {0:c} in pledges will not be deleted.\r\nYou should probably delete them first.",
										ad.Row.Pledges.Sum(p => p.Amount));
			else
				message = "Are you sure you want to delete this ad?";

			if (Dialog.Warn(message))
				ad.Delete();
		}

		public void AutoFormat(IRibbonControl control) {
			var ad = control.CurrentAd();
			if (ad == null) return;
			var warnings = ad.CheckWarnings();
			if (warnings.Any()
			 && !Dialog.Warn("This ad has unresolved warnings:\r\n • " + warnings.Join("\r\n • ", w => w.Message)
						   + "\r\nThe autoformatter may not catch everything.  Do you want to autoformat anyway?"))
				return;
			new AdFormatter(ad, Config.GetElement("Journal", "AutoFormatRules")).FormatText();
		}
	}
}
