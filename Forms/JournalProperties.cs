using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;

namespace ShomreiTorah.Journal.Forms {
	partial class JournalProperties : XtraForm {
		public JournalProperties(PowerPoint.Presentation presentation) {
			InitializeComponent();
			Text = presentation.Name + " Properties";
			JournalYear = JournalPresentation.GetYear(presentation);
		}

		static int DefaultYear { get { return DateTime.Today.AddMonths(5).Year; } }

		public int? JournalYear {
			get { return isJournal.Checked ? (int)year.Value : new int?(); }
			set {
				isJournal.Checked = value.HasValue;
				year.Value = value ?? DefaultYear;

				UpdateEditState();
			}
		}

		private void isJournal_CheckedChanged(object sender, EventArgs e) { UpdateEditState(); }
		void UpdateEditState(){
			year.Enabled = isJournal.Checked;
			year.EditValue = isJournal.Checked ? (object)DefaultYear : null;
		}
	}
}