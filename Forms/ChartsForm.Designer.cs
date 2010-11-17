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
			DevExpress.XtraCharts.Series series1 = new DevExpress.XtraCharts.Series();
			DevExpress.XtraCharts.PieSeriesLabel pieSeriesLabel1 = new DevExpress.XtraCharts.PieSeriesLabel();
			DevExpress.XtraCharts.PiePointOptions piePointOptions1 = new DevExpress.XtraCharts.PiePointOptions();
			DevExpress.XtraCharts.PiePointOptions piePointOptions2 = new DevExpress.XtraCharts.PiePointOptions();
			DevExpress.XtraCharts.PieSeriesView pieSeriesView1 = new DevExpress.XtraCharts.PieSeriesView();
			DevExpress.XtraCharts.PieSeriesLabel pieSeriesLabel2 = new DevExpress.XtraCharts.PieSeriesLabel();
			DevExpress.XtraCharts.PieSeriesView pieSeriesView2 = new DevExpress.XtraCharts.PieSeriesView();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChartsForm));
			this.chartBindingSource1 = new ShomreiTorah.Journal.Forms.ChartBindingSource(this.components);
			this.xtraTabControl1 = new DevExpress.XtraTab.XtraTabControl();
			this.xtraTabPage1 = new DevExpress.XtraTab.XtraTabPage();
			this.chartControl1 = new DevExpress.XtraCharts.ChartControl();
			this.xtraTabPage2 = new DevExpress.XtraTab.XtraTabPage();
			((System.ComponentModel.ISupportInitialize)(this.chartBindingSource1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).BeginInit();
			this.xtraTabControl1.SuspendLayout();
			this.xtraTabPage1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.chartControl1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(series1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(pieSeriesLabel1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(pieSeriesView1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(pieSeriesLabel2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(pieSeriesView2)).BeginInit();
			this.SuspendLayout();
			// 
			// chartBindingSource1
			// 
			this.chartBindingSource1.Position = 0;
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
			this.chartControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chartControl1.Location = new System.Drawing.Point(0, 0);
			this.chartControl1.Name = "chartControl1";
			series1.ArgumentDataMember = "AdTypes.Type";
			series1.DataSource = this.chartBindingSource1;
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
			series1.ValueDataMembersSerializable = "AdTypes.Value";
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
			this.xtraTabPage2.Name = "xtraTabPage2";
			this.xtraTabPage2.Size = new System.Drawing.Size(745, 456);
			this.xtraTabPage2.Text = "xtraTabPage2";
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
			((System.ComponentModel.ISupportInitialize)(this.chartBindingSource1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).EndInit();
			this.xtraTabControl1.ResumeLayout(false);
			this.xtraTabPage1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(pieSeriesLabel1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(pieSeriesView1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(series1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(pieSeriesLabel2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(pieSeriesView2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.chartControl1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraTab.XtraTabControl xtraTabControl1;
		private DevExpress.XtraTab.XtraTabPage xtraTabPage1;
		private DevExpress.XtraTab.XtraTabPage xtraTabPage2;
		private ChartBindingSource chartBindingSource1;
		private DevExpress.XtraCharts.ChartControl chartControl1;
	}
}