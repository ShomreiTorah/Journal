using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.UserSkins;
using ShomreiTorah.Common;
using ShomreiTorah.Data;
using ShomreiTorah.Data.UI;
using ShomreiTorah.Data.UI.DisplaySettings;
using ShomreiTorah.Data.UI.Forms;
using ShomreiTorah.Singularity;
using ShomreiTorah.Singularity.Sql;
using ShomreiTorah.WinForms;
using System.Composition.Hosting;

namespace ShomreiTorah.Journal.AddIn {
	class Program : AppFramework {
		///<summary>Gets the typed table containing the given row type in the current AppFramework.</summary>
		///<remarks>This method is replaced to use the new Current property.</remarks>
		public static new TypedTable<TRow> Table<TRow>() where TRow : Row {
			if (Current.DataContext.Table<Person>().Rows.Count == 0)
				Current.RefreshDatabase();
			return Current.DataContext.Table<TRow>();
		}

		///<summary>Sets up the DataContext to support the PaymentImport UI.</summary>
		public static void SetUpPaymentImport() {
			LoadTables(EmailAddress.Schema, Billing.PaymentImport.ImportedPayment.Schema);

			if (!Person.Schema.Columns.Contains("BalanceDue")) {
				Person.Schema.Columns.AddCalculatedColumn<Person, decimal>("BalanceDue",
					person => person.Pledges.Sum(p => p.Amount) - person.Payments.Sum(p => p.Amount));
			}
		}

		public static void Save() {
			// Commit any active editors.
			NativeMethods.GetFocusedControl()?.Parent?.Focus();
			try {
				Current.SaveDatabase();
			} catch (Exception ex) {
				Current.HandleException(ex);
			}
		}

		public static void CheckDesignTime() {
			//If the project is re-built, AppFramework.Current
			//will refer to the instance from the old assembly
			if (AppFramework.Current != null && typeof(Program).Assembly != AppFramework.Current.GetType().Assembly)
				AppFramework.Current = null;

			AppFramework.CheckDesignTime(new Program());
		}

		public static void Initialize() {
			Current.ToString(); //Force property getter
		}
		///<summary>Indicates whether the Data.UI AppFramework has been initialized.</summary>
		public static bool WasInitialized { get { return AppFramework.Current != null; } }  //The base class property won't auto-init.
		public static new Program Current {
			get {
				if (AppFramework.Current != null)
					return (Program)AppFramework.Current;

				var retVal = new Program();
				AppFramework.Current = retVal;
				retVal.InitializeRuntime();

				return retVal;
			}
		}
		///<summary>Initializes the journal addin at runtime</summary>
		void InitializeRuntime() {
			IsDesignTime = false;
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			//VSTO will not register the SynchronizationContext, so I need to do it myself.
			SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

			if (!Debugger.IsAttached) {
				Application.ThreadException += (sender, e) => HandleException(e.Exception);
				AppDomain.CurrentDomain.UnhandledException += (sender, e) => HandleException((Exception)e.ExceptionObject);
			}

			AddDefaultExceptionHandlers();
			RegisterStandardSettings();
			RegisterSettings();
			SyncContext = CreateDataContext();

			Statistics = new StatsManager(DataContext);
			Globals.ThisAddIn.Shutdown += delegate { Save(); };
		}

		protected override void RegisterSettings() {
			SkinManager.EnableFormSkinsIfNotVista();
			UserLookAndFeel.Default.SkinName = "Office 2010 Blue";
			Dialog.DefaultTitle = "Shomrei Torah Journal";

			RegisterRowDetail<Person>(p => new SimplePersonDetails(p).Show(Globals.ThisAddIn == null ? null : Globals.ThisAddIn.Application.Window()));
			GridManager.RegisterBehavior(Pledge.Schema,
				DeletionBehavior.WithMessages<Pledge>(
					singular: p => {
						var message = "Are you sure you want to delete this " + p.Amount.ToString("c", CultureInfo.CurrentCulture) + " pledge?";

						var year = p.GetJournalYear();
						if (year.HasValue) {
							if (p.Person.MelaveMalkaSeats.Any(mms => mms.Year == year))
								message += Environment.NewLine + p.Person.FullName + "'s seating reservations will not be deleted.";

							var ad = Table<JournalAd>().Rows.FirstOrDefault(a => a.Year == year && p.ExternalId == a.ExternalId);
							if (ad != null && ad.Pledges.Has(2))    //If the pledge's ad has another pledge
								message += Environment.NewLine + "Remember to adjust the other pledge amounts.";
						}
						return message;
					},
					plural: pledges => "Are you sure you want to delete "
									  + (pledges.Count().ToString(CultureInfo.InvariantCulture) + " pledges totaling "
									   + pledges.Sum(p => p.Amount).ToString("c", CultureInfo.CurrentCulture) + "?\r\nNo seating reservations will be deleted.")
				)
			);
		}

		public StatsManager Statistics { get; private set; }
		public Lazy<CompositionHost> MefContainer { get; } = new Lazy<CompositionHost>(() => new ContainerConfiguration()
				 .WithAssembly(typeof(Program).Assembly)
				 .WithAssembly(typeof(Billing.PaymentImport.ImportForm).Assembly)
				 .CreateContainer());

		protected override DataSyncContext CreateDataContext() {
			Pledge.PersonColumn.AddIndex();
			Payment.PersonColumn.AddIndex();

			var context = new DataContext();
			CreateTables(context);
			var dsc = new DataSyncContext(context, new SqlServerSqlProvider(DB.Default));
			dsc.Tables.AddPrimaryMappings();

			return dsc;
		}

		///<summary>Creates the tables used by the addin.</summary>
		///<remarks>This method is called by the chart form to create
		public static void CreateTables(DataContext context) {
			//Remove unused calculated columns.  These columns are
			//used by the billing system, and reference tables that
			//we don't load.
			MelaveMalkaInvitation.Schema.Columns.RemoveColumn(MelaveMalkaInvitation.EmailAddressesColumn);
			MelaveMalkaInvitation.Schema.Columns.RemoveColumn(MelaveMalkaInvitation.AdAmountColumn);
			MelaveMalkaInvitation.Schema.Columns.RemoveColumn(MelaveMalkaInvitation.CallerColumn);

			context.Tables.AddTable(Person.CreateTable());
			context.Tables.AddTable(Pledge.CreateTable());
			context.Tables.AddTable(PledgeLink.CreateTable());
			context.Tables.AddTable(Payment.CreateTable());
			context.Tables.AddTable(Deposit.CreateTable());
			context.Tables.AddTable(JournalAd.CreateTable());
			context.Tables.AddTable(MelaveMalkaInvitation.CreateTable());
			context.Tables.AddTable(MelaveMalkaInfo.CreateTable());
			context.Tables.AddTable(MelaveMalkaSeat.CreateTable());
		}

		protected override Form CreateMainForm() { throw new NotSupportedException(); }
		protected override ISplashScreen CreateSplash() { throw new NotSupportedException(); }
	}
}
