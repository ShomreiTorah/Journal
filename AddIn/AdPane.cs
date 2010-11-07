using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using ShomreiTorah.Data;
using ShomreiTorah.Data.UI.Controls;
using ShomreiTorah.Data.UI.DisplaySettings;
using ShomreiTorah.Singularity;
using ShomreiTorah.WinForms;
using ShomreiTorah.WinForms.Controls.Lookup;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;

namespace ShomreiTorah.Journal.AddIn {
	[ToolboxItem(false)]
	partial class AdPane : XtraUserControl {
		JournalPresentation journal;
		readonly PowerPoint.DocumentWindow window;
		AdShape ad;
		FilteredTable<Pledge> pledges;
		FilteredTable<Payment> payments;
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
			pledgesGrid.DataSource = pledgesSource;
			paymentsGrid.DataSource = paymentsSource;
			SetAd(window.CurrentAd(), force: true);

			adSearcher.Properties.DataSource = new FilteredTable<Pledge>(
				Program.Table<Pledge>(),
				p => p.ExternalSource == "Journal " + journal.Year
			);
			EditorRepository.PersonOwnedLookup.Apply(adSearcher.Properties);
			adSearcher.Properties.Columns.Add(new DataSourceColumn("SubType") { Caption = "Type" });
			adSearcher.Properties.Columns.Insert(0, new DataSourceColumn("ExternalId", 35) { Caption = "ID" });

			window.Application.WindowSelectionChange += Application_WindowSelectionChange;
		}

		void Application_WindowSelectionChange(PowerPoint.Selection Sel) {
			var activeWindow = (PowerPoint.DocumentWindow)Sel.Parent;
			if (activeWindow.Presentation == journal.Presentation)
				SetAd(activeWindow.CurrentAd());
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
			if (pledges != null) pledges.Dispose();
			if (payments != null) payments.Dispose();
		}
		void SetAd(AdShape newAd, bool force = false) {
			if (this.ad == newAd && !force) return;
			DisposeDataSources();

			this.ad = newAd;
			layoutControl1.Visible = ad != null;
			if (!layoutControl1.Visible) return;
			adsBindingSource.Position = adsBindingSource.IndexOf(ad.Row);
			adType.EditValue = ad.AdType;

			//Singularity's dependency parser cannot handle 
			//external rows, so I can't use ad.Row inside of
			//the filter. However, I want to pick up changes
			//to the ad's ExternalId, so I use a function.
			Func<int> externalId = () => ad.Row.ExternalId;
			pledgesSource.DataSource = pledges = new FilteredTable<Pledge>(
				Program.Table<Pledge>(),
				p => p.ExternalSource == "Journal " + journal.Year && p.ExternalId == externalId()
			);
			paymentsSource.DataSource = payments = new FilteredTable<Payment>(
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
			ad.Shape.ForceSelect();
		}

		#region Add pledge/payment
		private void pledgeAdder_PersonSelecting(object sender, PersonSelectingEventArgs e) {
			if (pledges.Rows.Any(p => p.Person == e.Person)) {
				if (!Dialog.Warn("This ad already has a pledge by " + e.Person.VeryFullName + ".\r\nAre you sure you want to add another one?")) {
					e.Cancel = true;
					return;
				}
			}
			if (!e.Person.Invitees.Any(i => i.Year == journal.Year)) {
				if (!Dialog.Warn(e.Person.VeryFullName + " has not been invited to the Melave Malka.\r\nAre you sure you selected the correct person?"))
					e.Cancel = true;
			}
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
			foreach (var dontUse in Names.PaymentMethods) {
				var method = dontUse;	//Force a separate variable for each closure
				menu.Items.Add(new DXMenuItem(method, delegate {
					if (payments.Rows.Any(p => p.Person == pledge.Person)) {
						if (!Dialog.Warn("You already entered a payment for " + pledge.Person.VeryFullName + ".\r\nAre you sure you want to add a second payment?"))
							return;
					}
					var payment = ad.Row.CreatePayment();
					payment.Method = method;
					payment.Person = pledge.Person;
					payment.Amount = pledge.Amount;
					Program.Table<Payment>().Rows.Add(payment);
				}));
			}
			var control = (Control)sender;
			new SkinMenuManager(LookAndFeel).ShowPopupMenu(menu, control, control.PointToClient(MousePosition));
		}
		#endregion


		private void adSearcher_EditValueChanged(object sender, EventArgs e) {
			var pledge = adSearcher.EditValue as Pledge;
			if (pledge == null) return;

			var matchedAd = journal.Ads.FirstOrDefault(a => pledge.ExternalId == a.Row.ExternalId);
			if (matchedAd == null) {
				Dialog.ShowError("Cannot find matching ad.\r\nSomething is very wrong.");
				return;
			}
			SetAd(matchedAd);
			matchedAd.Shape.ForceSelect();
			adSearcher.EditValue = null;
		}
	}
}
