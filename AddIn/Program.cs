using System;
using System.Diagnostics;
using System.Windows.Forms;
using ShomreiTorah.Common;
using ShomreiTorah.Data;
using ShomreiTorah.Data.UI;
using ShomreiTorah.Data.UI.Forms;
using ShomreiTorah.Singularity;
using ShomreiTorah.Singularity.Sql;
using DevExpress.Skins;
using DevExpress.LookAndFeel;
using DevExpress.UserSkins;
using ShomreiTorah.WinForms;
using System.Threading;

namespace ShomreiTorah.Journal.AddIn {
	class Program : AppFramework {
		///<summary>Gets the typed table containing the given row type in the current AppFramework.</summary>
		///<remarks>This method is replaced to use the new Current property.</remarks>
		public static new TypedTable<TRow> Table<TRow>() where TRow : Row {
			if (Current.DataContext.Table<Person>().Rows.Count == 0)
				Current.RefreshDatabase();
			return Current.DataContext.Table<TRow>();
		}
		public static void Initialize() {
			Current.ToString();	//Force property getter
		}
		///<summary>Indicates whether the Data.UI AppFramework has been initialized.</summary>
		public static bool WasInitialized { get { return AppFramework.Current != null; } }	//The base class property won't auto-init.
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

			RegisterStandardSettings();
			RegisterSettings();
			SyncContext = CreateDataContext();
		}

		protected override void RegisterSettings() {
			OfficeSkins.Register();
			SkinManager.EnableFormSkinsIfNotVista();
			UserLookAndFeel.Default.SkinName = "Office 2010 Blue";
			Dialog.DefaultTitle = "Shomrei Torah Journal";

			RegisterRowDetail<Person>(p => new SimplePersonDetails(p).Show(Globals.ThisAddIn == null ? null : Globals.ThisAddIn.Application.Window()));
		}
		protected override DataSyncContext CreateDataContext() {
			var context = new DataContext();
			context.Tables.AddTable(Person.CreateTable());
			context.Tables.AddTable(Pledge.CreateTable());
			context.Tables.AddTable(Payment.CreateTable());
			context.Tables.AddTable(Deposit.CreateTable());
			context.Tables.AddTable(JournalAd.CreateTable());
			context.Tables.AddTable(MelaveMalkaInvitation.CreateTable());
			context.Tables.AddTable(MelaveMalkaSeat.CreateTable());

			var dsc = new DataSyncContext(context, new SqlServerSqlProvider(DB.Default));
			dsc.Tables.AddPrimaryMappings();
			return dsc;
		}

		protected override Form CreateMainForm() { throw new NotSupportedException(); }
		protected override ISplashScreen CreateSplash() { throw new NotSupportedException(); }
	}
}
