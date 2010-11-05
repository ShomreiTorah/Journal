using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;
using ShomreiTorah.Singularity;
using ShomreiTorah.Data;
using System.Windows.Forms;

namespace ShomreiTorah.Journal.AddIn {
	public partial class ThisAddIn {
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
					var jp = new JournalPresentation(presentation, Program.Table<JournalAd>());
					openJournals[presentation] = jp;

					var taskPane = GetTaskPane(presentation);
					if (taskPane == null)
						CreateTaskPane(jp);
					else {
						((AdPane)taskPane).ReplaceJournal(jp);
					}
				} else {
					JournalPresentation.KillJournal(presentation);
					CustomTaskPanes.Remove(GetTaskPane(presentation));
				}

				//Force UI to invalidate
				//presentation.Windows[1].ActivePane.ViewType = PpViewType.ppViewNormal;
				presentation.Windows[1].View.GotoSlide(1);
				((Slide)presentation.Windows[1].View.Slide).Shapes.SelectAll();
				presentation.Windows[1].Selection.Unselect();
			}
		}

		///<summary>Gets the journal contained by a presentation, or null if the presentation is not a journal.</summary>
		public JournalPresentation GetJournal(Presentation presentation) {
			if (presentation == null) throw new ArgumentNullException("presentation");
			return openJournals.GetValue(presentation);
		}
		public Microsoft.Office.Tools.CustomTaskPane GetTaskPane(Presentation presentation) {
			return CustomTaskPanes.FirstOrDefault(ctp => presentation.Windows.Cast<object>().Contains(ctp.Window));
		}

		private void ThisAddIn_Startup(object sender, EventArgs e) {
			Application.AfterPresentationOpen += Application_AfterPresentationOpen;
			Application.PresentationCloseFinal += Application_PresentationCloseFinal;

			Application.PresentationSave += Application_PresentationSave;
		}

		void Application_PresentationSave(Presentation Pres) {
			if (GetJournal(Pres) != null)
				Program.Current.SaveDatabase();
		}


		void Application_AfterPresentationOpen(Presentation Pres) {
			if (JournalPresentation.GetYear(Pres) != null) {
				var jp = new JournalPresentation(Pres, Program.Table<JournalAd>());
				openJournals.Add(Pres, jp);
				CreateTaskPane(jp);
			}
		}
		void CreateTaskPane(JournalPresentation jp) {
			var pane =CustomTaskPanes.Add(new AdPane(jp), "Ad Details", jp.Presentation.Windows[1]);
			pane.DockPosition = Office.MsoCTPDockPosition.msoCTPDockPositionRight;
			pane.Width = 450;
			pane.Visible = true;
		}
		void Application_PresentationCloseFinal(Presentation Pres) {
			CustomTaskPanes.Remove(GetTaskPane(Pres));
			openJournals.Remove(Pres);
		}

		private void ThisAddIn_Shutdown(object sender, EventArgs e) { }

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
