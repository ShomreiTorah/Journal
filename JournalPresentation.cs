using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.PowerPoint;
using ShomreiTorah.Singularity;
using ShomreiTorah.Data;
using System.Collections.ObjectModel;

namespace ShomreiTorah.Journal {
	///<summary>Manages a PowerPoint presentation containing a journal.</summary>
	public sealed class JournalPresentation {
		///<summary>Creates a JournalPresentation from an existing PowerPoint presentation and a Singularity table containing ad data.</summary>
		public JournalPresentation(Presentation presentation, TypedTable<JournalAd> adsTable) {
			if (presentation == null) throw new ArgumentNullException("presentation");
			if (adsTable == null) throw new ArgumentNullException("adsTable");

			Presentation = presentation;
			AdsTable = adsTable;
			Year = int.Parse(presentation.Tags["JournalYear"]);
		}

		///<summary>Gets the year of the journal managed by this instance.</summary>
		public int Year { get; private set; }
		///<summary>Gets the presentation containing the ads.</summary>
		public Presentation Presentation { get; private set; }
		///<summary>Gets the Singularity table containing the ad data.</summary>
		public TypedTable<JournalAd> AdsTable { get; private set; }

		///<summary>Gets the ads in the journal.</summary>
		public ReadOnlyCollection<AdShape> Ads { get; private set; }

		///<summary>Creates a new ad in the journal.</summary>
		public AdShape CreateAd(AdType type) {
			if (type == null) throw new ArgumentNullException("type");
			return null;
		}
	}
}
