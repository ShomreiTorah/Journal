using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraLayout;
using DevExpress.XtraLayout.Utils;
using ShomreiTorah.Common;
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

			colMensSeats.Caption = MelaveMalkaSeat.MensSeatsCaption;
			colWomensSeats.Caption = MelaveMalkaSeat.WomensSeatsCaption;

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
			adSearcher.Properties.Columns.RemoveAt(adSearcher.Properties.Columns.FindIndex(c => c.Caption == "Zip code"));

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
			if (this.ad == newAd && !force) {
				CheckWarnings();
				return;
			}
			Focus();	// Commit any pending edits.
			DisposeDataSources();

			this.ad = newAd;
			layoutControl1.Visible = ad != null;
			if (ad == null) return;
			adsBindingSource.Position = adsBindingSource.IndexOf(ad.Row);
			adType.EditValue = ad.AdType;

			//Singularity's dependency parser cannot handle 
			//external rows, so I can't use ad.Row inside of
			//the filter. However, I want to pick up changes
			//to the ad's ExternalId, so I use a function.
			Func<int> externalId = () => ad.Row.ExternalId;

			pledgesSource.DataSource = pledges = Program.Table<Pledge>().Filter(
				p => p.ExternalSource == "Journal " + journal.Year && p.ExternalId == externalId()
			);
			paymentsSource.DataSource = payments = Program.Table<Payment>().Filter(
				p => p.ExternalSource == "Journal " + journal.Year && p.ExternalId == externalId()
			);
			CheckWarnings();

			//I need to call BeginInvoke so that the new row gets painted first.
			pledges.RowAdded += delegate { BeginInvoke(new Action(pledgesView.BestFitColumns)); };
			payments.RowAdded += delegate { BeginInvoke(new Action(paymentsView.BestFitColumns)); };
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

			if (!journal.ConfirmModification())
				return;

			if (!CheckAdjustPledges("Change Ad Type", newType: newType)) {
				adType.EditValue = ad.AdType;		//Reset the editor's value.
				return;
			}

			ad.AdType = newType;
			ad.Shape.ForceSelect();
		}

		///<summary>Checks whether the user wants to adjust pledge amounts.</summary>
		bool CheckAdjustPledges(string actionName, AdType newType = null, IList<Pledge> newPledges = null) {
			var oldType = ad.AdType;
			var oldPledges = pledges.Rows;
			if (oldPledges.Count == 0) return true;		//If there weren't any pledges, there's nothing to do.

			newType = newType ?? ad.AdType;
			newPledges = newPledges ?? (IList<Pledge>)pledges.Rows;

			//If there are pledges, try adjusting the amounts
			if (newPledges.Count > 0) {
				//If there is a payment, we assume that it
				//has the correct amount and don't adjust 
				//anything.  If the pledge amounts aren't 
				//equal, we assume that something strange 
				//is being billed, and don't adjust them. 
				if (payments.Rows.Any()
				 || oldPledges.Any(p => Math.Abs(p.Amount - oldPledges[0].Amount) > 1)		//Allow off-by-one, in case it came from an indivisible pledge count
				 || oldPledges.Sum(p => p.Amount) != oldType.DefaultPrice) {
					ShowColumnTooltip(colPledgeAmount, new ToolTipControllerShowEventArgs {
						Rounded = true,
						ShowBeak = true,
						IconType = ToolTipIconType.Information,
						ToolTipType = ToolTipType.Standard,
						Title = actionName,
						ToolTip = "You probably want to adjust the pledge amounts.",
					});
				} else {
					decimal newAmount = newType.DefaultPrice / newPledges.Count;
					int baseAmount = (int)newAmount;	//Truncate
					int higherAdCount = 0;		//The number of ads which should receive $(baseAmount + 1) pledges to add up correctly
					string message;
					if (newAmount == baseAmount)
						message = String.Format(
							CultureInfo.CurrentCulture,
							"Would you like to change each pledge to {0:c} to match a {1}?",
							baseAmount, newType.PledgeSubType.ToLowerInvariant()
						);
					else {
						higherAdCount = (int)newType.DefaultPrice - (baseAmount * newPledges.Count);

						if (higherAdCount == 1)
							message = String.Format(
								CultureInfo.CurrentCulture,
								"Would you like to change the first pledge to {0:c} to and the rest to {1:c} to match a {2}?",
								baseAmount + 1, baseAmount, newType.PledgeSubType.ToLowerInvariant()
							);
						else
							message = String.Format(
								CultureInfo.CurrentCulture,
								"Would you like to change the first {0} pledges to {1:c} to and the rest to {2:c} to match a {3}?",
								higherAdCount, baseAmount + 1, baseAmount, newType.PledgeSubType.ToLowerInvariant()
							);
					}
					switch (Dialog.Show(message, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning)) {
						case DialogResult.Cancel:			//Don't change the ad type.
							return false;
						case DialogResult.No:				//Don't change any pledges.
							break;
						case DialogResult.Yes:				//Adjust the pledge amounts
							int pledgeIndex = 0;
							foreach (var pledge in newPledges.OrderBy(p => p.Person.LastName)) {
								if (pledgeIndex < higherAdCount)
									pledge.Amount = baseAmount + 1;
								else
									pledge.Amount = baseAmount;
								pledgeIndex++;
							}
							break;
					}
				}
			}
			return true;
		}

		//TODO: Auto-adjust when deleting pledges
		#region Add pledge/payment
		private void pledgeAdder_PersonSelecting(object sender, PersonSelectingEventArgs e) {
			if (pledges.Rows.Any(p => p.Person == e.Person)) {
				if (!Dialog.Warn("This ad already has a pledge by " + e.Person.VeryFullName + ".\r\nAre you sure you want to add another one?")) {
					e.Cancel = true;
					return;
				}
			}
			if (e.Method == PersonSelectionReason.ResultClick
			 && !e.Person.Invitees.Any(i => i.Year == journal.Year)) {
				if (!Dialog.Warn(e.Person.VeryFullName + " has not been invited to the Melave Malka.\r\nAre you sure you selected the correct person?"))
					e.Cancel = true;
			}
		}

		private void pledgeAdder_EditValueChanged(object sender, EventArgs e) {
			if (pledgeAdder.SelectedPerson == null) return;

			if (!journal.ConfirmModification())
				return;

			var pledge = ad.Row.CreatePledge();
			pledge.Person = pledgeAdder.SelectedPerson;
			pledge.Amount = ad.AdType.DefaultPrice;

			pledgeAdder.SelectedPerson = null;

			var newPledges = new Pledge[pledges.Rows.Count + 1];
			pledges.Rows.CopyTo(newPledges, 0);
			newPledges[pledges.Rows.Count] = pledge;
			if (!CheckAdjustPledges("Add Pledge", newPledges: newPledges))
				return;

			Program.Table<Pledge>().Rows.Add(pledge);
		}

		private void paymentMenuEdit_ButtonClick(object sender, ButtonPressedEventArgs e) {
			var pledge = (Pledge)pledgesView.GetFocusedRow();
			var menu = new DXPopupMenu();
			foreach (var dontUse in Names.PaymentMethods) {
				var method = dontUse;	//Force a separate variable for each closure
				menu.Items.Add(new DXMenuItem(method, delegate {
					if (payments.Rows.Any(p => p.Person == pledge.Person)) {
						if (!Dialog.Warn("You already entered a payment for " + pledge.Person.VeryFullName + ".\r\nAre you sure you want to add another payment?"))
							return;
					}
					var payment = ad.Row.CreatePayment();
					payment.Method = method;
					payment.Person = pledge.Person;
					payment.Amount = pledge.Amount;
					Program.Table<Payment>().Rows.Add(payment);
					Program.Table<PledgeLink>().Rows.Add(new PledgeLink { Pledge = pledge, Payment = payment, Amount = pledge.Amount });

					var rowHandle = paymentsView.GetRowHandle(payments.Rows.IndexOf(payment));
					paymentsView.SetSelection(rowHandle, makeVisible: true);

					if (payment.Method == "Check") {
						ShowColumnTooltip(colCheckNumber, new ToolTipControllerShowEventArgs {
							Rounded = true,
							ShowBeak = true,
							IconType = ToolTipIconType.Question,
							ToolTipType = ToolTipType.Standard,
							Title = "Add Check",
							ToolTip = "Please enter the check number and the date on the check",
						});
					}
				}));
			}
			var control = (Control)sender;
			new SkinMenuManager(LookAndFeel).ShowPopupMenu(menu, control, control.PointToClient(MousePosition));
		}
		#endregion

		static void ShowColumnTooltip(GridColumn column, ToolTipControllerShowEventArgs args) {
			var view = column.View;
			var viewInfo = (GridViewInfo)view.GetViewInfo();
			args.ToolTipLocation = ToolTipLocation.TopRight;
			args.SelectedControl = view.GridControl;

			var controller = new ToolTipController();
			controller.ShowHint(args, view.GridControl.PointToScreen(viewInfo.ColumnsInfo[column].Bounds.Location));
		}

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
		private void externalId_Validating(object sender, CancelEventArgs e) {
			if (journal.Ads.Any(a => a.Row.ExternalId == externalId.Value && a.Row != ad.Row)) {
				externalId.ErrorText = "There is already an ad with external ID " + externalId.Value;
				e.Cancel = true;
			}
		}

		#region Seating
		private void pledgesView_CustomUnboundColumnData(object sender, CustomColumnDataEventArgs e) {
			if (pledges == null) return;	//Still initializing

			if (e.Column.FieldName.StartsWith("Seat/", StringComparison.OrdinalIgnoreCase)) {
				var field = e.Column.FieldName.Substring("Seat/".Length);
				var pledge = pledges.Rows[e.ListSourceRowIndex];
				var seat = pledge.Person.MelaveMalkaSeats.FirstOrDefault(s => s.Year == journal.Year);

				if (e.IsGetData) {
					e.Value = seat == null ? null : seat[field];	//No reservation means unsure
				} else {	//Modify the existing seating row or add a new one
					if (seat != null) {
						seat[field] = e.Value;
						if (seat.MensSeats == null && seat.WomensSeats == null)
							seat.RemoveRow();
					} else {					//There isn't an existing seat row
						if (e.Value == null)	//If it's still null, don't change anything
							return;
						else {
							seat = new MelaveMalkaSeat {
								Year = journal.Year,
								Person = pledge.Person,
								DateAdded = DateTime.Now
							};
							seat[field] = e.Value;
							Program.Table<MelaveMalkaSeat>().Rows.Add(seat);
						}
					}
				}
			}
		}
		#endregion

		//TODO: Button to refresh warnings
		#region Warnings
		private void comments_Properties_Validating(object sender, CancelEventArgs e) {
			BeginInvoke(new Action(CheckWarnings));	//In case the user deleted a suppression
		}

		void CheckWarnings() {
			try {
				warningsGroup.BeginUpdate();
				foreach (var lci in warningsGroup.Items.OfType<LayoutControlItem>().ToList()) {
					lci.Control.Dispose();
				}
				warningsGroup.Clear();

				if (ad == null) return;	//I still need to clear the controls
				var warnings = ad.CheckWarnings().ToList();

				warningsGroup.Visibility = warnings.Count > 0 ? LayoutVisibility.OnlyInRuntime : LayoutVisibility.Never;

				foreach (var dontUse in warnings) {
					var warning = dontUse;	//Force closures to get separate variables

					var button = new SimpleButton {
						Text = "Suppress",
						MaximumSize = new Size(60, 22),
						SuperTip = Utilities.CreateSuperTip(title: "Suppress warning", body: "Adds a line to the ad's comments that suppresses this warning")
					};
					button.Click += delegate {
						warning.Suppress();
						CheckWarnings();
					};
					var item = warningsGroup.AddItem(warning.Message, button);
					item.ControlAlignment = ContentAlignment.MiddleLeft;
				}
			} finally { warningsGroup.EndUpdate(); }
		}
		#endregion
	}
}
