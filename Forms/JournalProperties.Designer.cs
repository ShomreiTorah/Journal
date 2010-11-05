namespace ShomreiTorah.Journal.Forms {
	partial class JournalProperties {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JournalProperties));
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.year = new DevExpress.XtraEditors.SpinEdit();
			this.isJournal = new DevExpress.XtraEditors.CheckEdit();
			this.cancel = new DevExpress.XtraEditors.SimpleButton();
			this.ok = new DevExpress.XtraEditors.SimpleButton();
			((System.ComponentModel.ISupportInitialize)(this.year.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.isJournal.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// labelControl1
			// 
			this.labelControl1.Location = new System.Drawing.Point(12, 39);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(60, 13);
			this.labelControl1.TabIndex = 0;
			this.labelControl1.Text = "Journal Year";
			// 
			// year
			// 
			this.year.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
			this.year.Location = new System.Drawing.Point(93, 36);
			this.year.Name = "year";
			this.year.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
			this.year.Properties.DisplayFormat.FormatString = "n0";
			this.year.Properties.EditFormat.FormatString = "n0";
			this.year.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
			this.year.Properties.NullText = "N/A";
			this.year.Size = new System.Drawing.Size(75, 20);
			this.year.TabIndex = 1;
			// 
			// isJournal
			// 
			this.isJournal.Location = new System.Drawing.Point(12, 12);
			this.isJournal.Name = "isJournal";
			this.isJournal.Properties.Caption = "Is Journal";
			this.isJournal.Size = new System.Drawing.Size(156, 18);
			this.isJournal.TabIndex = 0;
			this.isJournal.CheckedChanged += new System.EventHandler(this.isJournal_CheckedChanged);
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(93, 62);
			this.cancel.Name = "cancel";
			this.cancel.Size = new System.Drawing.Size(75, 23);
			this.cancel.TabIndex = 3;
			this.cancel.Text = "Cancel";
			// 
			// ok
			// 
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.Location = new System.Drawing.Point(12, 62);
			this.ok.Name = "ok";
			this.ok.Size = new System.Drawing.Size(75, 23);
			this.ok.TabIndex = 2;
			this.ok.Text = "OK";
			// 
			// JournalProperties
			// 
			this.AcceptButton = this.ok;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(178, 95);
			this.ControlBox = false;
			this.Controls.Add(this.ok);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.isJournal);
			this.Controls.Add(this.year);
			this.Controls.Add(this.labelControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "JournalProperties";
			this.Text = "Presentation Properties";
			((System.ComponentModel.ISupportInitialize)(this.year.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.isJournal.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DevExpress.XtraEditors.LabelControl labelControl1;
		private DevExpress.XtraEditors.SpinEdit year;
		private DevExpress.XtraEditors.CheckEdit isJournal;
		private DevExpress.XtraEditors.SimpleButton cancel;
		private DevExpress.XtraEditors.SimpleButton ok;
	}
}