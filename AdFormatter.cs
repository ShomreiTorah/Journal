using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Office.Core;

namespace ShomreiTorah.Journal {
	///<summary>Applies preset text formatting to specific substrings in an ad.</summary>
	class AdFormatter {
		readonly List<FormatRule> rules = new List<FormatRule>();
		public AdFormatter(AdShape ad, XElement configuredRules) {
			Ad = ad;
			var honoreeFormat = configuredRules.Element("Honorees");
			if (honoreeFormat != null)
				rules.AddRange(from honoree in ad.Presentation.MelaveMalka.Honorees
							   from regex in AdVerifier.GetNameRegexes(honoree)
							   select new FormatRule(regex, honoreeFormat.Element("Format")));

			var donorFormat = configuredRules.Element("Donors");
			if (donorFormat != null)
				rules.AddRange(from pledge in ad.Row.Pledges
							   from regex in AdVerifier.GetNameRegexes(pledge.Person)
							   select new FormatRule(regex, donorFormat.Element("Format")));

			rules.AddRange(from rule in configuredRules.Elements("Pattern")
						   from regex in rule.Attributes("Regex")
						   select new FormatRule(new Regex(regex.Value), rule.Element("Format")));
		}
		public AdShape Ad { get; }


		///<summary>Formats the text of the ad.  Avoid calling this method if the ad has warnings.</summary>
		public void FormatText() {
			var text = Ad.Shape.TextFrame2.TextRange.Text;
			foreach (var rule in rules) {
				foreach (Match match in rule.Regex.Matches(text)) {
					rule.Apply(Ad.Shape.TextFrame2.TextRange.Characters[match.Index + 1, match.Length]);
				}
			}
		}

		class FormatRule {
			public FormatRule(Regex regex, XElement format) {
				Regex = regex;

				FontFamily = (string)format.Element("FontFamily");
				FontSize = (float?)format.Element("FontSize");
				Bold = (bool?)format.Element("Bold");
				Italic = (bool?)format.Element("Italic");
				AllCaps = (bool?)format.Element("AllCaps");
				SmallCaps = (bool?)format.Element("SmallCaps");

				var alignment = (string)format.Element("Alignment");
				if (alignment != null)
					Alignment = (MsoParagraphAlignment)Enum.Parse(typeof(MsoParagraphAlignment),
																  "msoAlign" + alignment, ignoreCase: true);
			}

			public Regex Regex { get; }

			string FontFamily { get; }
			float? FontSize { get; }
			bool? Bold { get; }
			bool? Italic { get; }
			bool? AllCaps { get; }
			bool? SmallCaps { get; }
			MsoParagraphAlignment? Alignment { get; }

			public void Apply(TextRange2 range) {
				if (FontFamily != null) range.Font.Name = FontFamily;
				if (FontSize != null) range.Font.Size = FontSize.Value;
				if (Bold != null) range.Font.Bold = ToTriState(Bold.Value);
				if (Italic != null) range.Font.Italic = ToTriState(Italic.Value);
				if (AllCaps != null) range.Font.Allcaps = ToTriState(AllCaps.Value);
				if (SmallCaps != null) range.Font.Smallcaps = ToTriState(SmallCaps.Value);
				if (Alignment != null) range.ParagraphFormat.Alignment = Alignment.Value;
			}

			static MsoTriState ToTriState(bool val) => val ? MsoTriState.msoTrue : MsoTriState.msoFalse;
		}
	}
}
