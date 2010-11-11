using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Office.Interop.PowerPoint;
using ShomreiTorah.Data;
using Office = Microsoft.Office.Core;

namespace ShomreiTorah.Journal.AddIn {
	public sealed partial class ThisAddIn {
		#region Manage open journals
		readonly Dictionary<Presentation, JournalPresentation> openJournals = new Dictionary<Presentation, JournalPresentation>();

		///<summary>Shows the JournalProperties form for a presentation, allowing the user to change the year.</summary>
		public void ShowProperties(Presentation presentation) {
			if (presentation == null) throw new ArgumentNullException("presentation");
			Program.Initialize();
			using (var form = new Forms.JournalProperties(presentation)) {
				var oldYear = form.JournalYear;
				if (form.ShowDialog(presentation.Application.Window()) != DialogResult.OK) return;
				if (oldYear == form.JournalYear) return;

				openJournals.Remove(presentation);
				if (form.JournalYear.HasValue) {
					JournalPresentation.MakeJournal(presentation, form.JournalYear.Value);

					var jp = RegisterJournal(presentation, createTaskPane: oldYear == null);	//Only create a new taskpane if it wasn't already a journal

					if (oldYear != null)
						((AdPane)GetTaskPane(presentation).Control).ReplaceJournal(jp);
				} else {
					JournalPresentation.KillJournal(presentation);
					UnregisterJournal(presentation);
				}

				//Force UI to invalidate
				//presentation.Windows[1].ActivePane.ViewType = PpViewType.ppViewNormal;
				presentation.Windows[1].View.GotoSlide(1);
				((Slide)presentation.Windows[1].View.Slide).Shapes.SelectAll();
				presentation.Windows[1].Selection.Unselect();
			}
		}

		JournalPresentation RegisterJournal(Presentation presentation, bool createTaskPane = true) {
			var jp = new JournalPresentation(presentation, Program.Table<JournalAd>());
			openJournals.Add(presentation, jp);
			if (createTaskPane)
				CreateTaskPane(jp);
			return jp;
		}
		void UnregisterJournal(Presentation presentation) {
			CustomTaskPanes.Remove(GetTaskPane(presentation));
			openJournals.Remove(presentation);
		}

		void CreateTaskPane(JournalPresentation jp) {
			var pane = CustomTaskPanes.Add(new AdPane(jp), "Ad Details", jp.Presentation.Windows[1]);
			pane.DockPosition = Office.MsoCTPDockPosition.msoCTPDockPositionRight;
			pane.Width = 650;
			pane.Visible = true;
		}

		///<summary>Gets the journal contained by a presentation, or null if the presentation is not a journal.</summary>
		public JournalPresentation GetJournal(Presentation presentation) {
			if (presentation == null) throw new ArgumentNullException("presentation");
			return openJournals.GetValue(presentation);
		}
		public Microsoft.Office.Tools.CustomTaskPane GetTaskPane(Presentation presentation) {
			return CustomTaskPanes.FirstOrDefault(ctp => presentation.Windows.Cast<object>().Contains(ctp.Window));
		}
		#endregion

		private void ThisAddIn_Startup(object sender, EventArgs e) {
			Application.AfterPresentationOpen += Application_AfterPresentationOpen;
			Application.PresentationCloseFinal += Application_PresentationCloseFinal;

			Application.PresentationSave += Application_PresentationSave;
		}

		//These handlers should try not to directly use types from
		//other DLLs so that the JITter won't need to load them in
		//normal (non-journal) usage.  Instead, call other methods
		//that use the types after checking that we have a journal
		void Application_AfterPresentationOpen(Presentation Pres) {
			if (JournalPresentation.GetYear(Pres) != null)
				RegisterJournal(Pres);
		}
		void Application_PresentationSave(Presentation Pres) {
			if (GetJournal(Pres) != null)
				Program.Current.SaveDatabase();
		}
		void Application_PresentationCloseFinal(Presentation Pres) {
			if (GetJournal(Pres) != null) {
				Program.Current.SaveDatabase();
				UnregisterJournal(Pres);
			}
		}
		private void ThisAddIn_Shutdown(object sender, EventArgs e) {
			if (Program.WasInitialized)
				Program.Current.SaveDatabase();
		}

		protected override Office.IRibbonExtensibility CreateRibbonExtensibilityObject() {
			return new JournalRibbon();
		}

		#region VSTO generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InternalStartup() {
			this.Startup += new System.EventHandler(ThisAddIn_Startup);
			this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
		}

		#endregion
	}
}
