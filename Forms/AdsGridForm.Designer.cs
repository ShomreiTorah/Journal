namespace ShomreiTorah.Journal.Forms {
	partial class AdsGridForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdsGridForm));
			this.grid = new ShomreiTorah.Data.UI.Grid.SmartGrid(this.components);
			this.gridView = new ShomreiTorah.Data.UI.Grid.SmartGridView();
			this.colDateAdded = new ShomreiTorah.Data.UI.Grid.SmartGridColumn();
			this.colAdType = new ShomreiTorah.Data.UI.Grid.SmartGridColumn();
			this.colExternalId = new ShomreiTorah.Data.UI.Grid.SmartGridColumn();
			this.colComments = new ShomreiTorah.Data.UI.Grid.SmartGridColumn();
			this.colAdId = new ShomreiTorah.Data.UI.Grid.SmartGridColumn();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.grid.DataMember = "Ads";
			this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grid.Location = new System.Drawing.Point(0, 0);
			this.grid.MainView = this.gridView;
			this.grid.Name = "grid";
			this.grid.RegistrationCount = 49;
			this.grid.Size = new System.Drawing.Size(553, 645);
			this.grid.TabIndex = 0;
			this.grid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView});
			// 
			// gridView
			// 
			this.gridView.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colDateAdded,
            this.colAdType,
            this.colExternalId,
            this.colComments});
			this.gridView.GridControl = this.grid;
			this.gridView.Name = "gridView";
			this.gridView.SortInfo.AddRange(new DevExpress.XtraGrid.Columns.GridColumnSortInfo[] {
            new DevExpress.XtraGrid.Columns.GridColumnSortInfo(this.colExternalId, DevExpress.Data.ColumnSortOrder.Ascending)});
			this.gridView.DoubleClick += new System.EventHandler(this.gridView_DoubleClick);
			// 
			// colDateAdded
			// 
			this.colDateAdded.FieldName = "DateAdded";
			this.colDateAdded.Name = "colDateAdded";
			this.colDateAdded.OptionsColumn.AllowEdit = false;
			this.colDateAdded.OptionsColumn.AllowFocus = false;
			this.colDateAdded.OptionsColumn.ReadOnly = true;
			this.colDateAdded.Visible = true;
			this.colDateAdded.VisibleIndex = 0;
			this.colDateAdded.Width = 76;
			// 
			// colAdType
			// 
			this.colAdType.FieldName = "AdType";
			this.colAdType.Name = "colAdType";
			this.colAdType.OptionsColumn.AllowEdit = false;
			this.colAdType.OptionsColumn.AllowFocus = false;
			this.colAdType.OptionsColumn.ReadOnly = true;
			this.colAdType.Visible = true;
			this.colAdType.VisibleIndex = 1;
			this.colAdType.Width = 59;
			// 
			// colExternalId
			// 
			this.colExternalId.FieldName = "ExternalId";
			this.colExternalId.Name = "colExternalId";
			this.colExternalId.OptionsColumn.AllowEdit = false;
			this.colExternalId.OptionsColumn.AllowFocus = false;
			this.colExternalId.OptionsColumn.ReadOnly = true;
			this.colExternalId.Visible = true;
			this.colExternalId.VisibleIndex = 2;
			this.colExternalId.Width = 85;
			// 
			// colComments
			// 
			this.colComments.FieldName = "Comments";
			this.colComments.Name = "colComments";
			this.colComments.OptionsColumn.AllowEdit = false;
			this.colComments.OptionsColumn.AllowFocus = false;
			this.colComments.OptionsColumn.ReadOnly = true;
			this.colComments.Visible = true;
			this.colComments.VisibleIndex = 3;
			this.colComments.Width = 69;
			// 
			// colAdId
			// 
			this.colAdId.FieldName = "AdId";
			this.colAdId.Name = "colAdId";
			this.colAdId.Visible = true;
			this.colAdId.VisibleIndex = 0;
			// 
			// AdsGridForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(553, 645);
			this.Controls.Add(this.grid);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "AdsGridForm";
			this.Text = "AdsGridForm";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Data.UI.Grid.SmartGrid grid;
		private Data.UI.Grid.SmartGridView gridView;
		private Data.UI.Grid.SmartGridColumn colDateAdded;
		private Data.UI.Grid.SmartGridColumn colAdType;
		private Data.UI.Grid.SmartGridColumn colExternalId;
		private Data.UI.Grid.SmartGridColumn colComments;
		private Data.UI.Grid.SmartGridColumn colAdId;
	}
}