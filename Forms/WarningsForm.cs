using System;
using System.Linq;
using System.Windows.Forms;
using DevExpress.Data.Filtering;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;

namespace ShomreiTorah.Journal.Forms {
	partial class WarningsForm : XtraForm {
		JournalPresentation journal;
		public WarningsForm(JournalPresentation journal) {
			if (journal == null) throw new ArgumentNullException("journal");
			InitializeComponent();

			this.journal = journal;
			Text = "Journal " + journal.Year + " Warnings";
			RebindGrid();
			gridView.ActiveFilterCriteria = new OperandProperty("IsSuppressed") == new OperandValue(false);
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
			//The appearances only get their defaults after the ctor.
			gridView.PaintAppearance.FocusedRow.Assign(gridView.PaintAppearance.HideSelectionRow);
			suppressionEdit.Appearance.Assign(gridView.PaintAppearance.HideSelectionRow);
			disabledSuppressionEdit.Appearance.Assign(gridView.PaintAppearance.HideSelectionRow);
		}

		private void refresh_Click(object sender, EventArgs e) { RebindGrid(); }
		void RebindGrid() {
			grid.DataSource = journal.Ads.SelectMany(AdVerifier.CheckAllWarnings).ToList();
		}

		private void gridView_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e) {
			if (e.Column == colWarning) {
				var warning = (AdWarning)gridView.GetRow(e.RowHandle);
				e.RepositoryItem = warning.IsSuppressed ? disabledSuppressionEdit : suppressionEdit;
			}
		}
		private void suppressionEdit_ButtonClick(object sender, ButtonPressedEventArgs e) {
			var warning = (AdWarning)gridView.GetFocusedRow();
			warning.Suppress();
			RebindGrid();
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
		//This handler handles both edits.
		private void suppressionEdit_DoubleClick(object sender, EventArgs e) {
			var warning = (AdWarning)gridView.GetFocusedRow();
			warning.Ad.Shape.ForceSelect();
		}
	}
}