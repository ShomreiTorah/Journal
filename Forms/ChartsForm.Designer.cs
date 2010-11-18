namespace ShomreiTorah.Journal.Forms {
	partial class ChartsForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			DevExpress.XtraCharts.SimpleDiagram simpleDiagram1 = new DevExpress.XtraCharts.SimpleDiagram();
			DevExpress.XtraCharts.Series series1 = new DevExpress.XtraCharts.Series();
			DevExpress.XtraCharts.PieSeriesLabel pieSeriesLabel1 = new DevExpress.XtraCharts.PieSeriesLabel();
			DevExpress.XtraCharts.PiePointOptions piePointOptions1 = new DevExpress.XtraCharts.PiePointOptions();
			DevExpress.XtraCharts.PiePointOptions piePointOptions2 = new DevExpress.XtraCharts.PiePointOptions();
			DevExpress.XtraCharts.PieSeriesView pieSeriesView1 = new DevExpress.XtraCharts.PieSeriesView();
			DevExpress.XtraCharts.PieSeriesLabel pieSeriesLabel2 = new DevExpress.XtraCharts.PieSeriesLabel();
			DevExpress.XtraCharts.PieSeriesView pieSeriesView2 = new DevExpress.XtraCharts.PieSeriesView();
			DevExpress.XtraCharts.Series series2 = new DevExpress.XtraCharts.Series();
			DevExpress.XtraCharts.PieSeriesLabel pieSeriesLabel3 = new DevExpress.XtraCharts.PieSeriesLabel();
			DevExpress.XtraCharts.PiePointOptions piePointOptions3 = new DevExpress.XtraCharts.PiePointOptions();
			DevExpress.XtraCharts.PiePointOptions piePointOptions4 = new DevExpress.XtraCharts.PiePointOptions();
			DevExpress.XtraCharts.PieSeriesView pieSeriesView3 = new DevExpress.XtraCharts.PieSeriesView();
			DevExpress.XtraCharts.PieSeriesLabel pieSeriesLabel4 = new DevExpress.XtraCharts.PieSeriesLabel();
			DevExpress.XtraCharts.PieSeriesView pieSeriesView4 = new DevExpress.XtraCharts.PieSeriesView();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChartsForm));
			this.adTypesSource = new ShomreiTorah.Journal.Forms.ChartBindingSource(this.components);
			this.xtraTabControl1 = new DevExpress.XtraTab.XtraTabControl();
			this.xtraTabPage1 = new DevExpress.XtraTab.XtraTabPage();
			this.chartControl1 = new DevExpress.XtraCharts.ChartControl();
			this.xtraTabPage2 = new DevExpress.XtraTab.XtraTabPage();
			this.chartControl2 = new DevExpress.XtraCharts.ChartControl();
			((System.ComponentModel.ISupportInitialize)(this.adTypesSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).BeginInit();
			this.xtraTabControl1.SuspendLayout();
			this.xtraTabPage1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.chartControl1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(simpleDiagram1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(series1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(pieSeriesLabel1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(pieSeriesView1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(pieSeriesLabel2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(pieSeriesView2)).BeginInit();
			this.xtraTabPage2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.chartControl2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(series2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(pieSeriesLabel3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(pieSeriesView3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(pieSeriesLabel4)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(pieSeriesView4)).BeginInit();
			this.SuspendLayout();
			// 
			// adTypesSource
			// 
			this.adTypesSource.DataSet = ShomreiTorah.Journal.Forms.ChartDataSet.AdTypes;
			this.adTypesSource.Position = 0;
			// 
			// xtraTabControl1
			// 
			this.xtraTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.xtraTabControl1.Location = new System.Drawing.Point(0, 0);
			this.xtraTabControl1.Name = "xtraTabControl1";
			this.xtraTabControl1.SelectedTabPage = this.xtraTabPage1;
			this.xtraTabControl1.Size = new System.Drawing.Size(753, 485);
			this.xtraTabControl1.TabIndex = 0;
			this.xtraTabControl1.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.xtraTabPage1,
            this.xtraTabPage2});
			this.xtraTabControl1.Selected += new DevExpress.XtraTab.TabPageEventHandler(this.xtraTabControl1_Selected);
			// 
			// xtraTabPage1
			// 
			this.xtraTabPage1.Controls.Add(this.chartControl1);
			this.xtraTabPage1.Name = "xtraTabPage1";
			this.xtraTabPage1.Size = new System.Drawing.Size(745, 456);
			this.xtraTabPage1.Text = "Ad Value by Type";
			// 
			// chartControl1
			// 
			this.chartControl1.DataSource = this.adTypesSource;
			simpleDiagram1.Dimension = 1;
			this.chartControl1.Diagram = simpleDiagram1;
			this.chartControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chartControl1.Legend.AlignmentHorizontal = DevExpress.XtraCharts.LegendAlignmentHorizontal.Right;
			this.chartControl1.Location = new System.Drawing.Point(0, 0);
			this.chartControl1.Name = "chartControl1";
			series1.ArgumentDataMember = "Type";
			series1.DataSource = this.adTypesSource;
			pieSeriesLabel1.LineVisible = true;
			series1.Label = pieSeriesLabel1;
			piePointOptions1.Pattern = "{A}s: {V}";
			piePointOptions1.PercentOptions.ValueAsPercent = false;
			piePointOptions1.PointView = DevExpress.XtraCharts.PointView.ArgumentAndValues;
			piePointOptions1.ValueNumericOptions.Format = DevExpress.XtraCharts.NumericFormat.Currency;
			piePointOptions1.ValueNumericOptions.Precision = 0;
			series1.LegendPointOptions = piePointOptions1;
			series1.Name = "Series 1";
			piePointOptions2.Pattern = "{A}s";
			piePointOptions2.PercentOptions.ValueAsPercent = false;
			piePointOptions2.PointView = DevExpress.XtraCharts.PointView.Argument;
			piePointOptions2.ValueNumericOptions.Format = DevExpress.XtraCharts.NumericFormat.Currency;
			piePointOptions2.ValueNumericOptions.Precision = 0;
			series1.PointOptions = piePointOptions2;
			series1.SynchronizePointOptions = false;
			series1.ValueDataMembersSerializable = "Value";
			pieSeriesView1.RuntimeExploding = false;
			series1.View = pieSeriesView1;
			this.chartControl1.SeriesSerializable = new DevExpress.XtraCharts.Series[] {
        series1};
			pieSeriesLabel2.LineVisible = true;
			this.chartControl1.SeriesTemplate.Label = pieSeriesLabel2;
			pieSeriesView2.RuntimeExploding = false;
			this.chartControl1.SeriesTemplate.View = pieSeriesView2;
			this.chartControl1.Size = new System.Drawing.Size(745, 456);
			this.chartControl1.TabIndex = 0;
			// 
			// xtraTabPage2
			// 
			this.xtraTabPage2.Controls.Add(this.chartControl2);
			this.xtraTabPage2.Name = "xtraTabPage2";
			this.xtraTabPage2.Size = new System.Drawing.Size(745, 456);
			this.xtraTabPage2.Text = "Ad Count by Type";
			// 
			// chartControl2
			// 
			this.chartControl2.DataSource = this.adTypesSource;
			this.chartControl2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chartControl2.Legend.AlignmentHorizontal = DevExpress.XtraCharts.LegendAlignmentHorizontal.Right;
			this.chartControl2.Location = new System.Drawing.Point(0, 0);
			this.chartControl2.Name = "chartControl2";
			series2.ArgumentDataMember = "Type";
			series2.DataSource = this.adTypesSource;
			pieSeriesLabel3.LineVisible = true;
			series2.Label = pieSeriesLabel3;
			piePointOptions3.Pattern = "{A}s: {V}";
			piePointOptions3.PercentOptions.ValueAsPercent = false;
			piePointOptions3.PointView = DevExpress.XtraCharts.PointView.ArgumentAndValues;
			series2.LegendPointOptions = piePointOptions3;
			series2.Name = "Series 1";
			piePointOptions4.Pattern = "{A}s";
			piePointOptions4.PointView = DevExpress.XtraCharts.PointView.Argument;
			series2.PointOptions = piePointOptions4;
			series2.SynchronizePointOptions = false;
			series2.ValueDataMembersSerializable = "Count";
			pieSeriesView3.RuntimeExploding = false;
			series2.View = pieSeriesView3;
			this.chartControl2.SeriesSerializable = new DevExpress.XtraCharts.Series[] {
        series2};
			pieSeriesLabel4.LineVisible = true;
			this.chartControl2.SeriesTemplate.Label = pieSeriesLabel4;
			pieSeriesView4.RuntimeExploding = false;
			this.chartControl2.SeriesTemplate.View = pieSeriesView4;
			this.chartControl2.Size = new System.Drawing.Size(745, 456);
			this.chartControl2.TabIndex = 0;
			// 
			// ChartsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(753, 485);
			this.Controls.Add(this.xtraTabControl1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ChartsForm";
			this.Text = "Journal Charts";
			((System.ComponentModel.ISupportInitialize)(this.adTypesSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).EndInit();
			this.xtraTabControl1.ResumeLayout(false);
			this.xtraTabPage1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(simpleDiagram1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(pieSeriesLabel1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(pieSeriesView1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(series1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(pieSeriesLabel2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(pieSeriesView2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.chartControl1)).EndInit();
			this.xtraTabPage2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(pieSeriesLabel3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(pieSeriesView3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(series2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(pieSeriesLabel4)).EndInit();
			((System.ComponentModel.ISupportInitialize)(pieSeriesView4)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.chartControl2)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraTab.XtraTabControl xtraTabControl1;
		private DevExpress.XtraTab.XtraTabPage xtraTabPage1;
		private DevExpress.XtraTab.XtraTabPage xtraTabPage2;
		private ChartBindingSource adTypesSource;
		private DevExpress.XtraCharts.ChartControl chartControl1;
		private DevExpress.XtraCharts.ChartControl chartControl2;
	}
}