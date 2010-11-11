namespace ShomreiTorah.Journal.Forms {
	partial class WarningsForm {
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
			DevExpress.XtraGrid.StyleFormatCondition styleFormatCondition1 = new DevExpress.XtraGrid.StyleFormatCondition();
			DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject1 = new DevExpress.Utils.SerializableAppearanceObject();
			DevExpress.Utils.SuperToolTip superToolTip1 = new DevExpress.Utils.SuperToolTip();
			DevExpress.Utils.ToolTipTitleItem toolTipTitleItem1 = new DevExpress.Utils.ToolTipTitleItem();
			DevExpress.Utils.ToolTipItem toolTipItem1 = new DevExpress.Utils.ToolTipItem();
			DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject2 = new DevExpress.Utils.SerializableAppearanceObject();
			DevExpress.Utils.SuperToolTip superToolTip2 = new DevExpress.Utils.SuperToolTip();
			DevExpress.Utils.ToolTipTitleItem toolTipTitleItem2 = new DevExpress.Utils.ToolTipTitleItem();
			DevExpress.Utils.ToolTipItem toolTipItem2 = new DevExpress.Utils.ToolTipItem();
			DevExpress.Utils.SuperToolTip superToolTip3 = new DevExpress.Utils.SuperToolTip();
			DevExpress.Utils.ToolTipTitleItem toolTipTitleItem3 = new DevExpress.Utils.ToolTipTitleItem();
			DevExpress.Utils.ToolTipItem toolTipItem3 = new DevExpress.Utils.ToolTipItem();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WarningsForm));
			this.colIsSuppressed = new DevExpress.XtraGrid.Columns.GridColumn();
			this.grid = new DevExpress.XtraGrid.GridControl();
			this.gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
			this.colExternalId = new DevExpress.XtraGrid.Columns.GridColumn();
			this.colAdType = new DevExpress.XtraGrid.Columns.GridColumn();
			this.colWarning = new DevExpress.XtraGrid.Columns.GridColumn();
			this.suppressionEdit = new DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit();
			this.disabledSuppressionEdit = new DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit();
			this.refresh = new DevExpress.XtraEditors.SimpleButton();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.suppressionEdit)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.disabledSuppressionEdit)).BeginInit();
			this.SuspendLayout();
			// 
			// colIsSuppressed
			// 
			this.colIsSuppressed.Caption = "Suppressed?";
			this.colIsSuppressed.FieldName = "IsSuppressed";
			this.colIsSuppressed.Name = "colIsSuppressed";
			this.colIsSuppressed.OptionsColumn.AllowEdit = false;
			this.colIsSuppressed.OptionsColumn.AllowFocus = false;
			this.colIsSuppressed.UnboundType = DevExpress.Data.UnboundColumnType.Boolean;
			// 
			// grid
			// 
			this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grid.Location = new System.Drawing.Point(0, 0);
			this.grid.MainView = this.gridView;
			this.grid.Name = "grid";
			this.grid.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.suppressionEdit,
            this.disabledSuppressionEdit});
			this.grid.Size = new System.Drawing.Size(477, 575);
			this.grid.TabIndex = 0;
			this.grid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView});
			// 
			// gridView
			// 
			this.gridView.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colExternalId,
            this.colAdType,
            this.colWarning,
            this.colIsSuppressed});
			this.gridView.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
			styleFormatCondition1.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Strikeout);
			styleFormatCondition1.Appearance.ForeColor = System.Drawing.Color.DarkGray;
			styleFormatCondition1.Appearance.Options.UseFont = true;
			styleFormatCondition1.Appearance.Options.UseForeColor = true;
			styleFormatCondition1.ApplyToRow = true;
			styleFormatCondition1.Column = this.colIsSuppressed;
			styleFormatCondition1.Condition = DevExpress.XtraGrid.FormatConditionEnum.Equal;
			styleFormatCondition1.Value1 = true;
			this.gridView.FormatConditions.AddRange(new DevExpress.XtraGrid.StyleFormatCondition[] {
            styleFormatCondition1});
			this.gridView.GridControl = this.grid;
			this.gridView.Name = "gridView";
			this.gridView.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseDown;
			this.gridView.OptionsSelection.EnableAppearanceFocusedCell = false;
			this.gridView.SortInfo.AddRange(new DevExpress.XtraGrid.Columns.GridColumnSortInfo[] {
            new DevExpress.XtraGrid.Columns.GridColumnSortInfo(this.colExternalId, DevExpress.Data.ColumnSortOrder.Ascending)});
			this.gridView.CustomRowCellEdit += new DevExpress.XtraGrid.Views.Grid.CustomRowCellEditEventHandler(this.gridView_CustomRowCellEdit);
			this.gridView.DoubleClick += new System.EventHandler(this.gridView_DoubleClick);
			// 
			// colExternalId
			// 
			this.colExternalId.Caption = "External ID";
			this.colExternalId.FieldName = "ExternalId";
			this.colExternalId.MaxWidth = 85;
			this.colExternalId.Name = "colExternalId";
			this.colExternalId.OptionsColumn.AllowEdit = false;
			this.colExternalId.OptionsColumn.AllowFocus = false;
			this.colExternalId.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
			this.colExternalId.Visible = true;
			this.colExternalId.VisibleIndex = 0;
			// 
			// colAdType
			// 
			this.colAdType.Caption = "Ad Type";
			this.colAdType.FieldName = "AdType";
			this.colAdType.MaxWidth = 80;
			this.colAdType.Name = "colAdType";
			this.colAdType.OptionsColumn.AllowEdit = false;
			this.colAdType.OptionsColumn.AllowFocus = false;
			this.colAdType.UnboundType = DevExpress.Data.UnboundColumnType.String;
			this.colAdType.Visible = true;
			this.colAdType.VisibleIndex = 1;
			// 
			// colWarning
			// 
			this.colWarning.Caption = "Warning";
			this.colWarning.ColumnEdit = this.suppressionEdit;
			this.colWarning.FieldName = "Message";
			this.colWarning.Name = "colWarning";
			this.colWarning.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
			this.colWarning.UnboundType = DevExpress.Data.UnboundColumnType.String;
			this.colWarning.Visible = true;
			this.colWarning.VisibleIndex = 2;
			// 
			// suppressionEdit
			// 
			this.suppressionEdit.AllowFocused = false;
			this.suppressionEdit.AutoHeight = false;
			toolTipTitleItem1.Text = "Suppress Warning";
			toolTipItem1.LeftIndent = 6;
			toolTipItem1.Text = "Adds a line to the ad\'s comments field suppressing this warning.";
			superToolTip1.Items.Add(toolTipTitleItem1);
			superToolTip1.Items.Add(toolTipItem1);
			this.suppressionEdit.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph, "Suppress", -1, true, true, false, DevExpress.XtraEditors.ImageLocation.MiddleCenter, null, new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.None), serializableAppearanceObject1, "", null, superToolTip1, true)});
			this.suppressionEdit.Name = "suppressionEdit";
			this.suppressionEdit.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
			this.suppressionEdit.UseParentBackground = true;
			this.suppressionEdit.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(this.suppressionEdit_ButtonClick);
			this.suppressionEdit.DoubleClick += new System.EventHandler(this.suppressionEdit_DoubleClick);
			// 
			// disabledSuppressionEdit
			// 
			this.disabledSuppressionEdit.AllowFocused = false;
			this.disabledSuppressionEdit.AutoHeight = false;
			toolTipTitleItem2.Text = "Suppress Warning";
			toolTipItem2.LeftIndent = 6;
			toolTipItem2.Text = "This warning has already been suppressed.";
			superToolTip2.Items.Add(toolTipTitleItem2);
			superToolTip2.Items.Add(toolTipItem2);
			this.disabledSuppressionEdit.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph, "Suppress", -1, false, true, false, DevExpress.XtraEditors.ImageLocation.MiddleCenter, null, new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.None), serializableAppearanceObject2, "", null, superToolTip2, true)});
			this.disabledSuppressionEdit.Name = "disabledSuppressionEdit";
			this.disabledSuppressionEdit.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
			this.disabledSuppressionEdit.DoubleClick += new System.EventHandler(this.suppressionEdit_DoubleClick);
			// 
			// refresh
			// 
			this.refresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.refresh.Image = global::ShomreiTorah.Journal.Properties.Resources.Refresh16;
			this.refresh.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.refresh.Location = new System.Drawing.Point(454, 0);
			this.refresh.Name = "refresh";
			this.refresh.Size = new System.Drawing.Size(23, 23);
			toolTipTitleItem3.Text = "Refresh Warnings";
			toolTipItem3.LeftIndent = 6;
			toolTipItem3.Text = "Re-checks the journal for warnings";
			superToolTip3.Items.Add(toolTipTitleItem3);
			superToolTip3.Items.Add(toolTipItem3);
			this.refresh.SuperTip = superToolTip3;
			this.refresh.TabIndex = 1;
			this.refresh.Text = "Refresh";
			this.refresh.Click += new System.EventHandler(this.refresh_Click);
			// 
			// WarningsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(477, 575);
			this.Controls.Add(this.refresh);
			this.Controls.Add(this.grid);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "WarningsForm";
			this.Text = "All Warnings";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.suppressionEdit)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.disabledSuppressionEdit)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraGrid.GridControl grid;
		private DevExpress.XtraGrid.Views.Grid.GridView gridView;
		private DevExpress.XtraGrid.Columns.GridColumn colExternalId;
		private DevExpress.XtraGrid.Columns.GridColumn colAdType;
		private DevExpress.XtraGrid.Columns.GridColumn colWarning;
		private DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit suppressionEdit;
		private DevExpress.XtraEditors.SimpleButton refresh;
		private DevExpress.XtraGrid.Columns.GridColumn colIsSuppressed;
		private DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit disabledSuppressionEdit;
	}
}