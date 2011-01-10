using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ShomreiTorah.Singularity;
using ShomreiTorah.Data;
using ShomreiTorah.Journal.AddIn;
using DevExpress.Utils;

namespace ShomreiTorah.Journal.Forms {
	partial class AdsGridForm : DevExpress.XtraEditors.XtraForm {
		readonly JournalPresentation journal;
		readonly FilteredTable<JournalAd> datasource;
		public AdsGridForm(JournalPresentation jp) {
			InitializeComponent();
			journal = jp;
			Text = "Journal " + jp.Year + " Ads";
			grid.DataSource = datasource = Program.Table<JournalAd>().Filter(ad => ad.Year == journal.Year);
		}

		///<summary>Releases the unmanaged resources used by the AdsGridForm and optionally releases the managed resources.</summary>
		///<param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				datasource.Dispose();
				if (components != null) components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void gridView_DoubleClick(object sender, EventArgs e) {
			var info = gridView.CalcHitInfo(grid.PointToClient(Control.MousePosition));

			if (info.RowHandle >= 0 && info.InRow) {
				var row = (JournalAd)gridView.GetRow(info.RowHandle);
				var ad = journal.GetAd(row);
				ad.Shape.ForceSelect();

				var dx = e as DXMouseEventArgs;
				if (dx != null) dx.Handled = true;
			}
		}
	}
}