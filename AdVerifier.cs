using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ShomreiTorah.Data;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;

namespace ShomreiTorah.Journal {
	///<summary>Checks for warnings about ads.</summary>
	public static class AdVerifier {
		static readonly Func<AdShape, IEnumerable<AdWarning>>[] warners = { CheckPledges, CheckPayments, CheckNames, CheckSlidePosition };

		static IEnumerable<AdWarning> CheckPayments(AdShape ad) {
			var pledges = ad.Row.Pledges.ToDictionary(p => p.Person);
			foreach (var payment in ad.Row.Payments) {
				Pledge pledge;
				if (!pledges.TryGetValue(payment.Person, out pledge))
					yield return new AdWarning(ad, payment.Person.VeryFullName + " has a payment but not a pledge");
				else {
					if (pledge.Amount != payment.Amount)
						yield return new AdWarning(ad,
							String.Format(CultureInfo.CurrentCulture,
										  "{0} has a pledge for {1:c} but a payment for {2:c}",
										  payment.Person.VeryFullName, pledge.Amount, payment.Amount)
						);
					if (payment.Method == "Check" && String.IsNullOrWhiteSpace(payment.CheckNumber))
						yield return new AdWarning(ad,
							String.Format(CultureInfo.CurrentCulture,
										  "{0} has a {1:c} check that is missing a check number",
										  payment.Person.VeryFullName, payment.Amount)
						);
				}
			}
		}
		static IEnumerable<AdWarning> CheckPledges(AdShape ad) {
			var total = ad.Row.Pledges.Sum(p => p.Amount);
			if (total == 0)
				yield return new AdWarning(ad, "This ad has no pledges");
			else if (total > ad.AdType.DefaultPrice)
				yield return new AdWarning(ad, "This ad's pledges add up to " + total.ToString("c", CultureInfo.CurrentCulture));
		}

		#region Name Checker
		static IEnumerable<AdWarning> CheckNames(AdShape ad) {
			var body = ad.Shape.TextFrame2.TextRange.Text;
			return ad.Row.Pledges
				.Where(p => !HasName(p.Person, body))
				.Select(p => new AdWarning(ad, p.Person.VeryFullName + " does not appear in the ad text"));
		}
		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		static bool HasName(Person person, string text) {
			if (!String.IsNullOrEmpty(person.FullName)
			 && Regex.IsMatch(text, String.Format(@"(^|\W){0}(\W|$)", Regex.Escape(person.FullName))))
				return true;
			if (Regex.IsMatch(text, String.Format(@"(^|\W){0} Family(\W|$)", Regex.Escape(person.LastName))))
				return true;

			if (String.IsNullOrEmpty(person.HerName)
			 && Regex.IsMatch(text, String.Format(@"(^|\W){0} {1}(\W|$)", Regex.Escape(person.HisName), Regex.Escape(person.LastName))))
				return true;
			if (String.IsNullOrEmpty(person.HisName)
			 && Regex.IsMatch(text, String.Format(@"(^|\W){0} {1}(\W|$)", Regex.Escape(person.HerName), Regex.Escape(person.LastName))))
				return true;

			if (Regex.IsMatch(text, String.Format(@"(^|\W){0}\W(.*?\W)?{1}\W(.*?\W)?{2}(\W|$)",
									Regex.Escape(person.HisName), Regex.Escape(person.HerName), Regex.Escape(person.LastName))))
				return true;

			if (Regex.IsMatch(text, String.Format(@"(^|\W){1}\W(.*?\W)?{0}\W(.*?\W)?{2}(\W|$)",
									Regex.Escape(person.HisName), Regex.Escape(person.HerName), Regex.Escape(person.LastName))))
				return true;

			return false;
		}
		#endregion

		static IEnumerable<AdWarning> CheckSlidePosition(AdShape ad) {
			var previousSlide = (PowerPoint.Slide)ad.Shape.Parent;
			//The loop is necessary to skip special pages in the middle of the ads
			while (true) {
				if (previousSlide.SlideIndex <= 1)
					yield break;
				previousSlide = ad.Presentation.Presentation.Slides[previousSlide.SlideIndex - 1];
				var slideType = previousSlide.AdType();

				if (slideType == null) continue;	//Skip special pages in the middle of the ads

				if (slideType.Id > ad.AdType.Id)
					yield return new AdWarning(ad, "This ad is after a " + slideType.Name.ToLowerInvariant() + " page");
				//Either the ad is preceded by an OK slide or
				//we just gave an error. Either way, stop now
				yield break;
			}
		}

		///<summary>Gets warnings about an ad, if any.</summary>
		public static IEnumerable<AdWarning> CheckWarnings(this AdShape ad) {
			if (ad == null) throw new ArgumentNullException("ad");
			var commentLines = (ad.Row.Comments ?? "").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			return warners.SelectMany(f => f(ad))
						  .Where(w => w != null)
						  .Where(w => !commentLines.Any(c => c.StartsWith(w.Message, StringComparison.CurrentCultureIgnoreCase)));
		}
	}
	///<summary>A warning about an ad.</summary>
	public sealed class AdWarning {
		///<summary>Creates an AdWarning instance.</summary>
		public AdWarning(AdShape ad, string message) {
			if (ad == null) throw new ArgumentNullException("ad");

			Ad = ad;
			Message = message;
		}

		///<summary>Gets the ad that the warning applies to.</summary>
		public AdShape Ad { get; private set; }
		///<summary>Gets the warning message.</summary>
		public string Message { get; private set; }
		/////<summary>Gets the text that must appear in the ad's comments field to suppress the warning.</summary>
		//public string SuppressionPrefix { get; private set; }

		///<summary>Adds a line to the ad's comments that suppresses this warning.</summary>
		public void Suppress() {
			Ad.Row.Comments += Environment.NewLine + Message + " because (?)";
			Ad.Row.Comments = Ad.Row.Comments.Trim();
		}
	}
}
