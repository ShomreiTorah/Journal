using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using ShomreiTorah.Data;
using ShomreiTorah.Journal.AddIn;
using ShomreiTorah.Singularity;

namespace ShomreiTorah.Journal.Forms {
	//Charts are bound to LINQ-generated arrays of anonymous types.
	//These are created by the ChartBindingSource class; it exposes
	//an enum property to select the datasource.
	//To make the form load faster, I only bind each chart when its
	//tab is first focused. (Except at design-time, when everything
	//will load immediately)  To make this work, ChartBindingSource
	//will bind to null at runtime until the RefreshList method is 
	//called by the TabControl's Selected handler.
	partial class ChartsForm : XtraForm {
		readonly int year;
		public ChartsForm(int year) {	//TODO: Refresh
			InitializeComponent();
			this.year = year;
			Text = "Journal " + year + " Charts";
		}
		protected override void OnShown(EventArgs e) {
			base.OnShown(e);
			ReloadTab(xtraTabControl1.SelectedTabPage);
		}
		private void xtraTabControl1_Selected(object sender, TabPageEventArgs e) { ReloadTab(e.Page); }
		void ReloadTab(XtraTabPage page) {
			if (page == null || page.Controls.Count == 0) return;
			var chart = page.Controls[0] as ChartControl;
			if (chart == null) return;

			var source = chart.DataSource as ChartBindingSource;
			Debug.Assert(source != null, page.Text + " chart has no datasource!");
			if (!source.HasRealData)	//If we haven't loaded this datasource yet, do so.
				source.RefreshList(year);
		}
	}

	class ChartBindingSource : BindingSource {
		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static ChartBindingSource() { Program.CheckDesignTime(); }
		public ChartBindingSource() { }
		public ChartBindingSource(IContainer container) : base(container) { }

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Attribute replacement")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new object DataSource { get { return base.DataSource; } set { base.DataSource = value; } }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Attribute replacement")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new string DataMember { get { return base.DataMember; } set { base.DataMember = value; } }

		const int DesignerYear = 2010;

		ChartDataSet dataSet;
		///<summary>Gets or sets the dataset exposed by the BindingSource.</summary>
		[Description("Gets or sets the dataset exposed by the BindingSource.")]
		[Category("Data")]
		[DefaultValue(ChartDataSet.None)]
		public ChartDataSet DataSet {
			get { return dataSet; }
			set {
				dataSet = value;

				if (value == ChartDataSet.None)
					DataSource = null;
				else if (Program.Current.IsDesignTime)
					RefreshList(DesignerYear);
				else
					DataSource = null;	//At runtime, only bind real data when we're explicitly asked to.
			}
		}

		///<summary>Refreshes the datasource from the Singularity DataContext.</summary>
		///<remarks>This method is called when each tabpage is first selected 
		///to initially populate the chart, and when the refresh button is clicked.</remarks>
		public void RefreshList(int year) {
			if (DataSet == ChartDataSet.None) return;
			DataSource = DataSetGenerators[DataSet](year, Program.Current.DataContext);
			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
			HasRealData = true;
		}
		///<summary>Indicates whether the instance is bound to an actual dataset.</summary>
		[Browsable(false)]
		public bool HasRealData { get; private set; }

		#region Dataset Generators
		static IList GenerateAdTypes(int year, DataContext dc) {
			return dc.Table<Pledge>().Rows
					.Where(p => p.GetJournalYear() == year && Names.AdTypes.Any(t => t.PledgeSubType == p.SubType))
					.GroupBy(
						p => p.SubType,
						(subtype, pledges) => new {
							Type = subtype,
							Count = pledges.Count(),
							Value = pledges.Sum(p => p.Amount)
						}
					)
					.ToArray();
		}

		class DefaultDictionary<TKey, TValue> {
			readonly Dictionary<TKey, TValue> inner = new Dictionary<TKey, TValue>();

			public TValue this[TKey key] {
				get {
					TValue retVal;
					inner.TryGetValue(key, out retVal);
					return retVal;
				}
				set { inner[key] = value; }
			}
		}

		static IList GenerateRunningTotals(int year, DataContext dc) {
			var totalCounts = new DefaultDictionary<string, int>();
			var totalValues = new DefaultDictionary<string, decimal>();
			return dc.Table<Pledge>().Rows
					.Where(p => p.GetJournalYear() == year && Names.AdTypes.Any(t => t.PledgeSubType == p.SubType))
					.OrderBy(p => p.Date)
					.GroupBy(
						p => new { p.Date, p.SubType },
						(o, pledges) => new {
							o.Date,
							AdType = o.SubType,

							TotalCount = totalCounts[o.SubType] += pledges.Count(),
							TotalValue = totalValues[o.SubType] += pledges.Sum(p => p.Amount)
						}
					)
					.ToArray();
		}

		static readonly Dictionary<ChartDataSet, DataGenerator> DataSetGenerators = new Dictionary<ChartDataSet, DataGenerator> {
			{ ChartDataSet.AdTypes,			GenerateAdTypes			},
			{ ChartDataSet.RunningTotals,	GenerateRunningTotals	}
		};

		delegate IList DataGenerator(int year, DataContext dc);
		#endregion
	}
	enum ChartDataSet {
		None,
		AdTypes,
		RunningTotals
	}
}