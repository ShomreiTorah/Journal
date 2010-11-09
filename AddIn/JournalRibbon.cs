using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;
using ShomreiTorah.Data;
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
		}

		#region Boolean Callbacks
		public bool IsPresentation(IRibbonControl control) {
			var window = control.Window();
			return window != null && window.Presentation != null;
		}
		public bool IsJournal(IRibbonControl control) { return control.Journal() != null; }
		public bool IsAdSelected(IRibbonControl control) { return control.CurrentAd() != null; }
		#endregion

		public void ShowProperties(IRibbonControl control) {
			var window = control.Window();
			Globals.ThisAddIn.ShowProperties(window.Presentation);
		}
		public void ShowDetailPane(IRibbonControl control) {
			var window = control.Window();
			Globals.ThisAddIn.CustomTaskPanes.FirstOrDefault(p => p.Window == window).Visible = true;
		}

		public void SaveDB(IRibbonControl control) { Program.Current.SaveDatabase(); }
		public void RefreshDB(IRibbonControl control) {
			SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
			Program.Current.RefreshDatabase();
		}

		public void InsertAd(IRibbonControl control, string selectedId, int selectedIndex) {
			var jp = control.Journal();
			var typeName = selectedId.Substring("Insert".Length);
			jp.CreateAd(Names.AdTypes.First(t => t.Name == typeName)).Shape.ForceSelect();
		}
		public void InsertSpecialPage(IRibbonControl control) {
			control.Window().View.Slide = control.Window().Presentation.Slides.Add(1, PpSlideLayout.ppLayoutBlank);
		}
		public void DeleteAd(IRibbonControl control) {
			//TODO: Delete confirmation
			if (Dialog.Warn("Are you sure you want to delete this ad?"))
				control.CurrentAd().Delete();
		}
	}
}
