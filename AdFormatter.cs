using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Office.Core;
using ShomreiTorah.Common;
using ShomreiTorah.Data;
using ShomreiTorah.Journal.AddIn;

namespace ShomreiTorah.Journal {
	///<summary>Applies preset text formatting to specific substrings in an ad.</summary>
	class AdFormatter {
		readonly IReadOnlyCollection<FormatRule> rules;
		public AdFormatter(AdShape ad, XElement configuredRules) {
			Ad = ad;
			rules = (from rule in configuredRules.Elements("FormatRule")
					 from regex in rule.Elements().SelectMany(GetRegexes)
					 select new FormatRule(regex, rule.Element("Format"))
					).ToList().AsReadOnly();
		}
		public AdShape Ad { get; }
		IEnumerable<Regex> GetRegexes(XElement element) {
			switch (element.Name.LocalName) {
				case "MatchHonorees":
					if (Ad.Presentation.MelaveMalka == null)
						return Enumerable.Empty<Regex>();
					return Ad.Presentation.MelaveMalka.Honorees.SelectMany(AdVerifier.GetNameRegexes);
				case "MatchDonors":
					return Ad.Row.Pledges.SelectMany(p => AdVerifier.GetNameRegexes(p.Person));
				case "Match":
					return new[] { new Regex(element.Attribute("Regex").Value) };
				case "MatchPerson":
					foreach (var field in element.Attributes())
						if (!Person.Schema.Columns.Contains(field.Name.LocalName))
							throw new ConfigurationException($"Unexpected attribute {field} in <MatchPerson>");
					var fields = element.Attributes().ToDictionary(
						a => a.Name.LocalName,
						a => a.Value
					);
					var people = Program.Table<Person>().Rows.Where(p => fields.All(f => f.Value.Equals(p[f.Key])));
					if (people.Has(2))
						throw new ConfigurationException($"Format rule {element} matches multiple people: {people.Join(", ", p => p.VeryFullName)}");
					var person = people.FirstOrDefault();
					if (person == null)
						throw new ConfigurationException($"Format rule {element} doesn't match anyone in the master directory.");
					return AdVerifier.GetNameRegexes(person);
				case "Format":  // Ignore this element.
					return Enumerable.Empty<Regex>();
				default:
					throw new ConfigurationException($"Unexpected <{element.Name}> element in <FormatRule>.");
			}
		}

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
