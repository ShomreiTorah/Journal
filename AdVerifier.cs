using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ShomreiTorah.Common;
using ShomreiTorah.Data;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;

namespace ShomreiTorah.Journal {
	///<summary>Checks for warnings about ads.</summary>
	public static class AdVerifier {
		static readonly Func<AdShape, IEnumerable<AdWarning>>[] warners = { CheckPledges, CheckPayments, CheckNames, CheckSlidePosition };

		static IEnumerable<AdWarning> CheckPayments(AdShape ad) {
			var pledgeMap = ad.Row.Pledges.ToLookup(p => p.Person);
			foreach (var payment in ad.Row.Payments) {
				var pledges = pledgeMap[payment.Person];
				if (!pledges.Any())
					yield return new AdWarning(ad, payment.Person.VeryFullName + " has a payment but not a pledge");
				else {
					var pledgeAmount = pledges.Sum(p => p.Amount);
					if (pledgeAmount != payment.Amount)
						yield return new AdWarning(ad,
							String.Format(CultureInfo.CurrentCulture,
										  "{0} has a pledge for {1:c} but a payment for {2:c}",
										  payment.Person.VeryFullName, pledgeAmount, payment.Amount)
						);
					if (payment.Method == "Check" && String.IsNullOrWhiteSpace(payment.CheckNumber))
						yield return new AdWarning(ad,
							String.Format(CultureInfo.CurrentCulture,
										  "{0} has a {1:c} check that is missing a check number",
										  payment.Person.VeryFullName, payment.Amount)
						);
				}
			}

			foreach (var group in pledgeMap.Where(g => g.Has(2)))
				yield return new AdWarning(ad, $"{group.Key.VeryFullName} has {group.Count()} pledges");
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
		static bool HasName(Person person, string text) {
			return GetNameRegexes(person).Any(r => r.IsMatch(text));
		}

		///<summary>Gets a collection of regexes to match all known forms that a person's name may be mentioned in an ad.</summary>
		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		public static IEnumerable<Regex> GetNameRegexes(Person person) {
			if (!String.IsNullOrWhiteSpace(person.FullName))
				yield return new Regex(String.Format(@"\b{0}\b", Regex.Escape(person.FullName)));

			if (!String.IsNullOrWhiteSpace(person.Salutation))
				yield return new Regex(String.Format(@"\b{0}\b", Regex.Escape(person.Salutation)));

			yield return new Regex(String.Format(@"\b{0} Family\b", Regex.Escape(person.LastName)));
			yield return new Regex(String.Format(@"\bThe {0}s\b", Regex.Escape(person.LastName)));

			if (!String.IsNullOrWhiteSpace(person.HisName))
				yield return new Regex(String.Format(@"\b{0} {1}\b", Regex.Escape(person.HisName), Regex.Escape(person.LastName)));

			if (!String.IsNullOrWhiteSpace(person.HerName))
				yield return new Regex(String.Format(@"\b{0} {1}\b", Regex.Escape(person.HerName), Regex.Escape(person.LastName)));

			yield return new Regex(String.Format(@"\b{0} & {1}\W(.*?\W)?{2}\b",
								   Regex.Escape(person.HisName ?? ""), Regex.Escape(person.HerName ?? ""), Regex.Escape(person.LastName)));

			yield return new Regex(String.Format(@"\b{1} & {0}\W(.*?\W)?{2}\b",
								   Regex.Escape(person.HisName ?? ""), Regex.Escape(person.HerName ?? ""), Regex.Escape(person.LastName)));
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

				if (slideType == null) continue;    //Skip special pages in the middle of the ads

				if (slideType.Index > ad.AdType.Index)
					yield return new AdWarning(ad, "This ad is after a " + slideType.Name.ToLowerInvariant() + " page");
				//Either the ad is preceded by an OK slide or
				//we just gave an error. Either way, stop now
				yield break;
			}
		}
		///<summary>Gets all warnings about an ad, if any. Suppressed warnings will also be returned.</summary>
		public static IEnumerable<AdWarning> CheckAllWarnings(this AdShape ad) {
			if (ad == null) throw new ArgumentNullException("ad");
			return warners.SelectMany(f => f(ad)).Where(w => w != null);
		}
		///<summary>Gets unsuppressed warnings about an ad, if any.</summary>
		public static IEnumerable<AdWarning> CheckWarnings(this AdShape ad) { return ad.CheckAllWarnings().Where(w => !w.IsSuppressed); }
	}
	///<summary>A warning about an ad.</summary>
	public sealed class AdWarning {
		///<summary>Creates an AdWarning instance.</summary>
		public AdWarning(AdShape ad, string message) {
			if (ad == null) throw new ArgumentNullException("ad");

			Ad = ad;
			Message = message;
			var commentLines = (ad.Row.Comments ?? "").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			IsSuppressed = commentLines.Any(c => c.StartsWith(Message, StringComparison.CurrentCultureIgnoreCase)); ;
		}

		///<summary>Gets the ad that the warning applies to.</summary>
		public AdShape Ad { get; private set; }
		///<summary>Gets the warning message.</summary>
		public string Message { get; private set; }
		///<summary>Indicates whether this ad has been suppressed.</summary>
		public bool IsSuppressed { get; private set; }

		//These properties are databound by WarningsForm
		public string AdType { get { return Ad.Row.AdType; } }
		public int ExternalId { get { return Ad.Row.ExternalId; } }

		/////<summary>Gets the text that must appear in the ad's comments field to suppress the warning.</summary>
		//public string SuppressionPrefix { get; private set; }

		///<summary>Adds a line to the ad's comments that suppresses this warning.</summary>
		public void Suppress() {
			Ad.Row.Comments += Environment.NewLine + Message + " because (?)";
			Ad.Row.Comments = Ad.Row.Comments.Trim();
		}
	}
}