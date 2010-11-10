using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.Utils;

namespace ShomreiTorah.Journal.Forms {
	partial class WarningsForm : XtraForm {
		JournalPresentation journal;
		public WarningsForm(JournalPresentation journal) {
			if (journal == null) throw new ArgumentNullException("journal");
			InitializeComponent();


			this.journal = journal;
			Text = "Journal " + journal.Year + " Warnings";
			RebindGrid();
		}
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
			//The appearances only get their defaults after the ctor.
			gridView.PaintAppearance.FocusedRow.Assign(gridView.PaintAppearance.HideSelectionRow);
			suppressionEdit.Appearance.Assign(gridView.PaintAppearance.HideSelectionRow);
		}

		private void suppressionEdit_ButtonClick(object sender, ButtonPressedEventArgs e) {
			var warning = (AdWarning)gridView.GetFocusedRow();
			warning.Suppress();
			RebindGrid();
		}

		private void refresh_Click(object sender, EventArgs e) { RebindGrid(); }

		void RebindGrid() {
			grid.DataSource = journal.Ads.SelectMany(AdVerifier.CheckWarnings).ToList();
		}

		private void gridView_DoubleClick(object sender, EventArgs e) {
			var info = gridView.CalcHitInfo(grid.PointToClient(Control.MousePosition));

			if (info.RowHandle >= 0 && info.InRow) {
				var dx = e as DXMouseEventArgs;
				if (dx != null) dx.Handled = true;

				var warning = (AdWarning)gridView.GetRow(info.RowHandle);
				warning.Ad.Shape.ForceSelect();
			}
		}

		private void suppressionEdit_DoubleClick(object sender, EventArgs e) {
			var warning = (AdWarning)gridView.GetFocusedRow();
			warning.Ad.Shape.ForceSelect();
		}
	}
}