using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;
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
		#endregion

		public void ShowProperties(IRibbonControl control) {
			var window = control.Window();
			Globals.ThisAddIn.ShowProperties(window.Presentation);
		}
		public void ShowDetailPane(IRibbonControl control) {
			var window = control.Window();
			Globals.ThisAddIn.CustomTaskPanes.FirstOrDefault(p => p.Window == window).Visible = true;
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

		public void InsertAd(IRibbonControl control, string selectedId, int selectedIndex) {
			var jp = control.Journal();

			if (!jp.ConfirmModification())
				return;

			var typeName = selectedId.Substring("Insert".Length);
			jp.CreateAd(Names.AdTypes.First(t => t.Name == typeName)).Shape.ForceSelect();
		}
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
	}
}
