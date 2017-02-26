using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShomreiTorah.Common;
using ShomreiTorah.Data;

namespace ShomreiTorah.Journal.AddIn {
	static class ExcelExporter {
		public static void ExportExcel(this IEnumerable<JournalAd> ads, string path) {
			var file = DB.CreateFile(path);
			using (var connection = file.OpenConnection()) {
				connection.ExecuteNonQuery($@"
CREATE TABLE [Ad Pledges] (
	[Last Name]		NVARCHAR(128),
	[His Name]		NVARCHAR(128),
	[Her Name]		NVARCHAR(128),
	[Full Name]		NVARCHAR(128),
	[Address]		NVARCHAR(128),
	[City]			NVARCHAR(128),
	[State]			NVARCHAR(128),
	[Zip]			NVARCHAR(128),
	[Phone]			NVARCHAR(128),
	[Ad Type]		NVARCHAR(128),
	[Amount]		MONEY,
	[Amount Paid]	MONEY,
	[{MelaveMalkaSeat.MensSeatsCaption}]		INT,
	[{MelaveMalkaSeat.WomensSeatsCaption}]		INT,
	[Date]			DATETIME
);");


				foreach (var ap in ads.SelectMany(ad => ad.Pledges, (Ad, Pledge) => new { Ad, Pledge }).OrderBy(s => s.Pledge.Person.LastName)) {
					var person = ap.Pledge.Person;
					var seats = person.MelaveMalkaSeats.FirstOrDefault(s => s.Year == ap.Ad.Year);
					connection.ExecuteNonQuery(
						$@"INSERT INTO [Ad Pledges]
		([Last Name],	[His Name],	[Her Name],	[Full Name],	[Address],	[City],	[State],	[Zip], 	[Phone],	
		 [Ad Type], [Amount], [Amount Paid], [{MelaveMalkaSeat.MensSeatsCaption}], [{MelaveMalkaSeat.WomensSeatsCaption}], [Date])
VALUES	(@LastName,		@HisName,	@HerName,	@FullName,		@Address,	@City,	@State,		@Zip,	@Phone,	
		 @AdType,   @Amount,	  @AmountPaid,   @MensSeats, @WomensSeats, @Date);",
		new {
			person.LastName,
			person.HisName,
			person.HerName,
			person.FullName,
			person.Address,
			person.City,
			person.State,
			person.Zip,
			person.Phone,
			ap.Ad.AdType,
			ap.Pledge.Amount,
			AmountPaid = ap.Ad.Payments.Where(p => p.Person == person).Sum(p => p.Amount),
			MensSeats = seats?.MensSeats ?? 0,
			WomensSeats = seats?.WomensSeats ?? 0,
			Date = TruncateTime(ap.Ad.DateAdded),
		}
	);
				}
			}
		}
		static DateTime TruncateTime(DateTime time) {
			// OleDB chokes on milliseconds
			return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
		}
	}
}
