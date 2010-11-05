using System;
using System.ComponentModel;
using System.Linq;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using ShomreiTorah.Data;
using ShomreiTorah.Data.UI.Controls;
using ShomreiTorah.Singularity;
using ShomreiTorah.WinForms;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using DevExpress.Utils.Menu;
using System.Windows.Forms;

namespace ShomreiTorah.Journal.AddIn {
	[ToolboxItem(false)]
	partial class AdPane : XtraUserControl {
		JournalPresentation journal;
		readonly PowerPoint.DocumentWindow window;
		AdShape ad;
		public AdPane(JournalPresentation journal) {
			if (journal == null) throw new ArgumentNullException("journal");
			InitializeComponent();

			this.journal = journal;
			this.window = journal.Presentation.Windows[1];

			adType.Properties.Items.AddRange(Names.AdTypes);
			adType.Properties.DropDownRows = Names.AdTypes.Count;

			//The grids are bound indirectly through two FrameworkBindingSource
			//so that they don't re-apply settings at every change.
			paymentsSource.DataMember = pledgesSource.DataMember = null;
			SetAd(window.CurrentAd());

			window.Application.WindowSelectionChange += Application_WindowSelectionChange;
		}

		void Application_WindowSelectionChange(PowerPoint.Selection Sel) {
			if (Sel.Parent == journal.Presentation)
				SetAd(window.CurrentAd());
		}

		///<summary>Releases the unmanaged resources used by the AdPane and optionally releases the managed resources.</summary>
		///<param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				DisposeDataSources();
				window.Application.WindowSelectionChange -= Application_WindowSelectionChange;
				if (components != null) components.Dispose();
			}
			base.Dispose(disposing);
		}

		public void ReplaceJournal(JournalPresentation newJournal) {
			this.journal = newJournal;
			SetAd(window.CurrentAd());
		}

		void DisposeDataSources() {
			var disposable = pledgesSource.DataSource as IDisposable;
			if (disposable != null) disposable.Dispose();
			disposable = paymentsSource.DataSource as IDisposable;
			if (disposable != null) disposable.Dispose();
		}
		void SetAd(AdShape ad) {
			if (this.ad == ad) return;
			DisposeDataSources();
			this.ad = ad;
			layoutControl1.Visible = ad != null;
			if (!layoutControl1.Visible) return;
			adsBindingSource.Position = adsBindingSource.IndexOf(ad.Row);
			adType.EditValue = ad.AdType;

			//Singularity's dependency parser cannot handle 
			//external rows, so I can't use ad.Row inside of
			//the filter. However, I want to pick up changes
			//to the ad's ExternalId, so I use a function.
			Func<int> externalId = () => ad.Row.ExternalId;
			pledgesSource.DataSource = new FilteredTable<Pledge>(
				Program.Table<Pledge>(),
				p => p.ExternalSource == "Journal " + journal.Year && p.ExternalId == externalId()
			);
			paymentsSource.DataSource = new FilteredTable<Payment>(
				Program.Table<Payment>(),
				p => p.ExternalSource == "Journal " + journal.Year && p.ExternalId == externalId()
			);
		}

		private void adType_SelectedValueChanged(object sender, EventArgs e) {
			if (ad == null) return;
			var newType = adType.EditValue as AdType;
			if (newType == null) {
				adType.EditValue = ad.AdType;
				return;
			}
			if (newType == ad.AdType)
				return;
			//TODO: Change price?
			ad.AdType = newType;
		}
		private void pledgeAdder_EditValueChanged(object sender, EventArgs e) {
			if (pledgeAdder.SelectedPerson == null) return;
			var pledge = ad.Row.CreatePledge();
			pledge.Person = pledgeAdder.SelectedPerson;
			pledge.Amount = ad.AdType.DefaultPrice;	//TODO: Split price
			Program.Table<Pledge>().Rows.Add(pledge);
			pledgeAdder.SelectedPerson = null;
		}

		private void paymentMenuEdit_ButtonClick(object sender, ButtonPressedEventArgs e) {
			var pledge = (Pledge)pledgesView.GetFocusedRow();
			var menu = new DXPopupMenu();
			foreach (var method in Names.PaymentMethods) {
				menu.Items.Add(new DXMenuItem(method, delegate {
					var payment = ad.Row.CreatePayment();
					payment.Method = method;
					payment.Person = pledge.Person;
					payment.Amount = pledge.Amount;
					Program.Table<Payment>().Rows.Add(payment);
				}));
			}
			var control = (Control)sender;
			MenuManagerHelper.GetMenuManager(LookAndFeel)
				.ShowPopupMenu(menu, control, control.PointToClient(MousePosition));
		}

		private void pledgeAdder_PersonSelecting(object sender, PersonSelectingEventArgs e) {
			if (!e.Person.Invitees.Any(i => i.Year == journal.Year)) {
				if (!Dialog.Warn(e.Person.FullName + " has not been invited to the Melave Malka.\r\nAre you sure you selected the correct person?"))
					e.Cancel = true;
			}
		}

	}
}
