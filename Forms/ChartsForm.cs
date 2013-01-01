using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
		public ChartsForm(int year) {
			InitializeComponent();
			this.year = year;
			Text = "Journal " + year + " Charts";
			xtraTabControl1.SelectedTabPageIndex = 0;
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
			switch (keyData) {
				case Keys.Escape:
					Close();
					return true;
				case Keys.F5:
					RefreshCharts();
					return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
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

			foreach (var source in GetDataSources(chart)) {
				if (!source.HasRealData)	//If we haven't loaded this datasource yet, do so.
					source.RefreshList(year);
			}
		}

		private void refresh_Click(object sender, EventArgs e) { RefreshCharts(); }
		void RefreshCharts() {
			foreach (var source in xtraTabControl1.TabPages
						.Where(t => t.Controls.Count > 0)
						.Select(t => t.Controls[0])
						.OfType<ChartControl>()
						.SelectMany(GetDataSources)) {

				if (source.HasRealData)			//If we already loaded this datasource,  refresh it.
					source.RefreshList(year);	//Don't refresh datasources that haven't been loaded
			}
		}

		static IEnumerable<ChartBindingSource> GetDataSources(ChartControl chart) {
			var dataSource = chart.DataSource as ChartBindingSource;
			if (dataSource != null)
				yield return dataSource;

			foreach (Series series in chart.Series) {
				dataSource = series.DataSource as ChartBindingSource;
				if (dataSource != null)
					yield return dataSource;
			}
		}
	}

	[DefaultProperty("DataSet")]
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

		const int DesignerYear = 2011;

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
							Count = pledges.AdCount(),
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

		static IList GenerateAdTypeRunningTotals(int year, DataContext dc) {
			var totalCounts = new DefaultDictionary<string, int>();
			var totalValues = new DefaultDictionary<string, decimal>();

			var pledges = dc.Table<Pledge>().Rows
					.Where(p => p.GetJournalYear() == year && Names.AdTypes.Any(t => t.PledgeSubType == p.SubType))
					.ToArray();

			DateTime firstAd = pledges.Min(p => p.Date.Date), lastAd = pledges.Max(p => p.Date.Date);
			var dates = Enumerable.Range(0, (lastAd - firstAd).Days + 1).Select(i => firstAd.AddDays(i));
			var pledgeLookup = pledges.ToLookup(p => new { p.Date.Date, p.SubType });

			return dates.SelectMany(date =>
				Names.AdTypes.Select(type => new {
					Date = date,
					AdType = type.PledgeSubType,

					TotalCount = totalCounts[type.PledgeSubType] += pledgeLookup[new { Date = date, SubType = type.PledgeSubType }].AdCount(),
					TotalValue = totalValues[type.PledgeSubType] += pledgeLookup[new { Date = date, SubType = type.PledgeSubType }].Sum(p => p.Amount)
				})
			).ToArray();
		}

		static IList GenerateYearlyRunningTotals(int year, DataContext dc) {
			var pledges = dc.Table<Pledge>().Rows
					.Where(p => p.GetJournalYear() == year && Names.AdTypes.Any(t => t.PledgeSubType == p.SubType))
					.ToArray();

			var info = dc.Table<MelaveMalkaInfo>().Rows.FirstOrDefault(i => i.Year == year);
			if (info == null) return null;

			DateTime firstAd = pledges.Min(p => p.Date.Date), lastAd = pledges.Max(p => p.Date.Date);
			var dates = Enumerable.Range(0, (lastAd - firstAd).Days + 1).Select(i => firstAd.AddDays(i));
			var pledgeLookup = pledges.ToLookup(p => p.Date.Date);

			int totalCount = 0;
			decimal totalValue = 0;
			return dates.Select(date => new {
				Date = date,
				DeadlineDelta = (info.AdDeadline - date).Days,

				TotalCount = totalCount += pledgeLookup[date].AdCount(),
				TotalValue = totalValue += pledgeLookup[date].Sum(p => p.Amount)
			}).ToArray();
		}

		static readonly Dictionary<ChartDataSet, DataGenerator> DataSetGenerators = new Dictionary<ChartDataSet, DataGenerator> {
			{ ChartDataSet.AdTypes,				GenerateAdTypes				},
			{ ChartDataSet.RunningAdTypeTotals,	GenerateAdTypeRunningTotals	},
			{ ChartDataSet.ThisYearRunningTotal, GenerateYearlyRunningTotals },
			{ ChartDataSet.LastYearRunningTotal, (year, dc) => GenerateYearlyRunningTotals(year - 1, dc) },
		};

		delegate IList DataGenerator(int year, DataContext dc);
		#endregion
	}
	enum ChartDataSet {
		None,
		AdTypes,
		RunningAdTypeTotals,
		ThisYearRunningTotal,
		LastYearRunningTotal
	}
}