using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShomreiTorah.Data;
using ShomreiTorah.Singularity;
using System.Globalization;

namespace ShomreiTorah.Journal.AddIn {
	class StatsManager {
		///<summary>Gets the DataContext that this instance reads.</summary>
		public DataContext Context { get; private set; }

		readonly Dictionary<int, JournalStatistics> stats = new Dictionary<int, JournalStatistics>();

		static bool IsJournal(string externalSource) { return externalSource != null && externalSource.StartsWith("Journal ", StringComparison.OrdinalIgnoreCase); }
		static int GetYear(string externalSource) { return int.Parse(externalSource.Substring("Journal ".Length), CultureInfo.InvariantCulture); }

		///<summary>Creates a StatsManager that tracks statistics from the given journal.</summary>
		public StatsManager(DataContext context) {
			if (context == null) throw new ArgumentNullException("context");
			Context = context;
			AddHandlers();
		}
		void AddHandlers() {
			Context.Table<MelaveMalkaSeat>().RowAdded += delegate { ReadSeating(); };
			Context.Table<MelaveMalkaSeat>().ValueChanged += delegate { ReadSeating(); };
			Context.Table<MelaveMalkaSeat>().RowRemoved += delegate { ReadSeating(); };

			Context.Table<Pledge>().RowAdded += (sender, e) => { if (IsJournal(e.Row.ExternalSource)) ReadPledges(); };
			Context.Table<Pledge>().ValueChanged += (sender, e) => { if (IsJournal(e.Row.ExternalSource)) ReadPledges(); };
			Context.Table<Pledge>().RowRemoved += (sender, e) => { if (IsJournal(e.Row.ExternalSource)) ReadPledges(); };

			Context.Table<Payment>().RowAdded += (sender, e) => { if (IsJournal(e.Row.ExternalSource)) ReadPayments(); };
			Context.Table<Payment>().ValueChanged += (sender, e) => { if (IsJournal(e.Row.ExternalSource)) ReadPayments(); };
			Context.Table<Payment>().RowRemoved += (sender, e) => { if (IsJournal(e.Row.ExternalSource)) ReadPayments(); };

			Context.Table<MelaveMalkaSeat>().LoadCompleted += Table_LoadCompleted;
			Context.Table<Pledge>().LoadCompleted += Table_LoadCompleted;
			Context.Table<Payment>().LoadCompleted += Table_LoadCompleted;
		}

		void Table_LoadCompleted(object sender, EventArgs e) {
			if (changedDuringLoad) {
				ReadSeating();
				ReadPledges();
				ReadPayments();
			}
			changedDuringLoad = false;
		}

		///<summary>Gets a JournalStatistics instance containing stats for the given year.</summary>
		public JournalStatistics this[int year] {
			get {
				JournalStatistics retVal;
				if (!stats.TryGetValue(year, out retVal))
					stats.Add(year, (retVal = new JournalStatistics()));
				return retVal;
			}
		}

		bool changedDuringLoad;
		void ReadSeating() {
			if (Context.Table<MelaveMalkaSeat>().IsLoadingData) {
				changedDuringLoad = true;
				return;
			}

			foreach (var js in stats.Values) {
				js.FamilySeats = 0;
				js.MensSeats = 0;
				js.WomensSeats = 0;
			}
			foreach (var seat in Context.Table<MelaveMalkaSeat>().Rows) {
				var js = this[seat.Year];
				js.MensSeats += seat.MensSeats ?? 0;
				js.WomensSeats += seat.WomensSeats ?? 0;
				if (js.MensSeats > 0 || js.WomensSeats > 0)
					js.FamilySeats++;
			}
			OnChanged();
		}
		void ReadPledges() {
			if (Context.Table<Pledge>().IsLoadingData) {
				changedDuringLoad = true;
				return;
			}
			foreach (var js in stats.Values)
				js.TotalPledged = 0;

			foreach (var pledge in Context.Table<Pledge>().Rows) {
				if (!IsJournal(pledge.ExternalSource)) continue;
				this[GetYear(pledge.ExternalSource)].TotalPledged += pledge.Amount;
			}
			OnChanged();
		}
		void ReadPayments() {
			if (Context.Table<Payment>().IsLoadingData) {
				changedDuringLoad = true;
				return;
			}
			foreach (var js in stats.Values)
				js.TotalPaid = 0;

			foreach (var payment in Context.Table<Payment>().Rows) {
				if (!IsJournal(payment.ExternalSource)) continue;
				this[GetYear(payment.ExternalSource)].TotalPaid += payment.Amount;
			}
			OnChanged();
		}

		///<summary>Occurs when the statistics for any year change.</summary>
		///<remarks>This event is static to allow the ribbon
		///to handle it before the AppFramework is created,
		///without loading any other assemblies.</remarks>
		public static event EventHandler Changed;
		///<summary>Raises the Changed event.</summary>
		internal protected virtual void OnChanged() { OnChanged(EventArgs.Empty); }
		///<summary>Raises the Changed event.</summary>
		///<param name="e">An EventArgs object that provides the event data.</param>
		internal protected virtual void OnChanged(EventArgs e) {
			if (Changed != null)
				Changed(this, e);
		}
	}

	public class JournalStatistics {
		public decimal TotalPledged { get; set; }
		public decimal TotalPaid { get; set; }

		public int MensSeats { get; set; }
		public int WomensSeats { get; set; }
		public int FamilySeats { get; set; }
	}
}
