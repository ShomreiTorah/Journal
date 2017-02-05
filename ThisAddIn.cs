using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Office.Interop.PowerPoint;
using ShomreiTorah.Data;
using ShomreiTorah.Data.UI.Forms;
using ShomreiTorah.WinForms;
using Office = Microsoft.Office.Core;

namespace ShomreiTorah.Journal.AddIn {
	public sealed partial class ThisAddIn {
		#region Manage open journals
		readonly Dictionary<Presentation, JournalPresentation> openJournals = new Dictionary<Presentation, JournalPresentation>();

		///<summary>Gets the journal contained by a presentation, or null if the presentation is not a journal.</summary>
		public JournalPresentation GetJournal(Presentation presentation) {
			if (presentation == null) throw new ArgumentNullException("presentation");
			return openJournals.GetValue(presentation);
		}
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

					var jp = RegisterJournal(presentation, createTaskPane: oldYear == null);    //Only create a new taskpane if it wasn't already a journal

					if (oldYear != null && jp != null)
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

		bool addedAppHandlers;
		JournalPresentation RegisterJournal(Presentation presentation, bool createTaskPane = true) {
			Program.Initialize();   //Force Dialog.DefaultTitle before warning dialogs

			try {
				if (Program.Current.DataContext.Table<Person>().Rows.Count == 0)
					Program.Current.RefreshDatabase();
			} catch (TargetInvocationException ex) {
				Dialog.ShowError("An error occurred while reading the database.  Please fix the problem and restart PowerPoint.\n\n" + ex.InnerException.Message);
			}

			try {
				if (!addedAppHandlers) {
					//Since I only need these handlers if there's a journal open,
					//I only add them now so that they won't load assemblies when
					//they execute throughout the year.
					Application.PresentationCloseFinal += Application_PresentationCloseFinal;
					Application.PresentationSave += Application_PresentationSave;
					addedAppHandlers = true;
				}

				var actualYear = JournalPresentation.GetYear(presentation);
				int parsedYear = 0;
				foreach (var segment in presentation.FullName.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) {
					if (int.TryParse(segment, out parsedYear) && parsedYear > 2000) {
						if (parsedYear != actualYear) {
							if (Dialog.Warn("This journal is in a " + parsedYear + " folder, but the file is linked to the " + actualYear
										  + " journal.\r\nDo you want to link the file to " + parsedYear + "?"))
								JournalPresentation.MakeJournal(presentation, parsedYear);
						}
						break;  //If we found any year in the path, stop searching
					}
				}
				if (parsedYear < 2000)  //If we didn't find any years in the segments
					Dialog.Show("The journal probably ought to be in a folder for its year.", MessageBoxIcon.Warning);

				var jp = new JournalPresentation(presentation, Program.Current.DataContext);
				openJournals.Add(presentation, jp);
				if (createTaskPane)
					CreateTaskPane(jp);
				return jp;
			} catch (Exception ex) {
				new ExceptionReporter(ex).Show(Application.Window());
				return null;
			}
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

		public Microsoft.Office.Tools.CustomTaskPane GetTaskPane(Presentation presentation) {
			return CustomTaskPanes.FirstOrDefault(ctp => presentation.Windows.Cast<object>().Contains((object)ctp.Window));
		}
		#endregion

		#region Journal-only handlers
		//These handlers are only added if a journal is open.
		//This way, they won't load assemblies during normal 
		//(non-journal) use.
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
		#endregion


		private void ThisAddIn_Startup(object sender, EventArgs e) {
			Application.AfterPresentationOpen += Application_AfterPresentationOpen;
		}

		//These handlers should try not to directly use types from
		//other DLLs so that the JITter won't need to load them in
		//normal (non-journal) usage.  Instead, call other methods
		//that use the types after checking that we have a journal
		void Application_AfterPresentationOpen(Presentation Pres) {
			if (JournalPresentation.GetYear(Pres) != null)
				RegisterJournal(Pres);
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
		}
		#endregion
	}
}
