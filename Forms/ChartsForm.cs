using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Collections;
using ShomreiTorah.Singularity;
using ShomreiTorah.Journal.AddIn;
using System.Threading;
using ShomreiTorah.Data;
using DevExpress.XtraTab;
using DevExpress.XtraCharts;

namespace ShomreiTorah.Journal.Forms {
	partial class ChartsForm : XtraForm {
		public ChartsForm() {
			InitializeComponent();
		}

		private void xtraTabControl1_Selected(object sender, TabPageEventArgs e) {
			if (e.Page == null || e.Page.Controls.Count == 0) return;
			var chart = e.Page.Controls[0] as ChartControl;
			if (chart == null) return;


		}
	}

	class ChartBindingSource : BindingSource {
		public ChartBindingSource() { DataSource = new ChartDataSet(); }
		public ChartBindingSource(IContainer container) : base(container) { DataSource = new ChartDataSet(); }

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new object DataSource {
			get { return base.DataSource; }
			set { base.DataSource = value; }
		}
	}
	class ChartDataSet {
		static ChartDataSet() { Program.CheckDesignTime(); }

		public ChartData AdTypes { get; private set; }

		public ChartDataSet() {
			AdTypes = new ChartData(dc =>
				dc.Table<Pledge>().Rows
				.Where(p => Names.AdTypes.Any(t => t.PledgeSubType == p.SubType))
				.GroupBy(
					p => p.SubType,
					(subtype, pledges) => new {
						Type = subtype,
						Count = pledges.Count(),
						Value = pledges.Sum(p => p.Amount)
					}
				).ToArray()
			);
		}
	}

	class ChartData : IListSource {
		static readonly Lazy<DataContext> dummyContext = new Lazy<DataContext>(delegate {
			var context = new DataContext();
			Program.CreateTables(context);
			return context;
		}, LazyThreadSafetyMode.None);

		readonly Func<DataContext, IList> listCreator;
		public ChartData(Func<DataContext, IList> listCreator) {
			if (listCreator == null) throw new ArgumentNullException("listCreator");
			this.listCreator = listCreator;

			if (Program.Current.IsDesignTime) {
				list = listCreator(Program.Current.DataContext);
				HasLoaded = true;
			} else
				list = listCreator(dummyContext.Value);
		}


		///<summary>Indicates whether the instance is populated with actual data.</summary>
		public bool HasLoaded { get; private set; }

		IList list;
		public void RefreshList() {
			list = listCreator(Program.Current.DataContext);
			HasLoaded = true;
		}

		public bool ContainsListCollection { get { return false; } }
		public IList GetList() { return list; }
	}
}