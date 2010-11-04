using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.PowerPoint;
using ShomreiTorah.Data;
using ShomreiTorah.Singularity;

namespace ShomreiTorah.Journal {
	///<summary>Manages a PowerPoint presentation containing a journal.</summary>
	///<remarks>
	/// Ad shapes are named after their JournalAd.AdId (a GUID)
	/// Ad slides have a Tag["AdType"] equal to the AdType.Name
	///</remarks>
	public sealed class JournalPresentation {
		internal const string TagAdType = "AdType";
		readonly List<AdShape> writableAds = new List<AdShape>();

		///<summary>Creates a JournalPresentation from an existing PowerPoint presentation and a Singularity table containing ad data.</summary>
		public JournalPresentation(Presentation presentation, TypedTable<JournalAd> adsTable) {
			if (presentation == null) throw new ArgumentNullException("presentation");
			if (adsTable == null) throw new ArgumentNullException("adsTable");

			Ads = new ReadOnlyCollection<AdShape>(writableAds);
			Presentation = presentation;
			AdsTable = adsTable;
			Year = int.Parse(presentation.Tags["JournalYear"], CultureInfo.InvariantCulture);

			var idMap = adsTable.Rows.Where(ad => ad.Year == Year).ToDictionary(ad => ad.AdId.ToString());
			writableAds.AddRange(
				from slide in Presentation.Slides.Items()
				where slide.AdType() != null
				from Shape shape in slide.Shapes.Placeholders
				let row = idMap.GetValue(shape.Name)
				where row != null
				select new AdShape(this, shape, row)
			);
		}

		///<summary>Gets the year of the journal managed by this instance.</summary>
		public int Year { get; private set; }
		///<summary>Gets the presentation containing the ads.</summary>
		public Presentation Presentation { get; private set; }
		///<summary>Gets the Singularity table containing the ad data.</summary>
		public TypedTable<JournalAd> AdsTable { get; private set; }

		///<summary>Gets the ads in the journal.</summary>
		public ReadOnlyCollection<AdShape> Ads { get; private set; }
		///<summary>Gets the ad describing the given shape.</summary>
		public AdShape GetAd(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			return Ads.FirstOrDefault(a => a.Shape == shape);	//TODO: Dictionary?
		}

		///<summary>Creates a new ad in the journal.</summary>
		public AdShape CreateAd(AdType type) {
			if (type == null) throw new ArgumentNullException("type");
			var shape = CreateAdShape(type);

			var row = new JournalAd { AdType = type.Name, DateAdded = DateTime.Now, Year = Year };
			shape.Name = row.AdId.ToString();
			var retVal = new AdShape(this, shape, row);
			writableAds.Add(retVal);
			return retVal;
		}

		#region Creation helpers
		///<summary>Gets the last slide containing the given ad type.</summary>
		///<param name="type">The maximum number of ads that can be contained on the slide.</param>
		///<returns>The slide, or null if there are no slides with that ad type.</returns>
		private Slide GetLastSlide(AdType type) {
			if (type == null) throw new ArgumentNullException("type");

			for (int n = Presentation.Slides.Count; n > 0; n--)
				if (Presentation.Slides[n].AdType() == type)
					return Presentation.Slides[n];

			return null;
		}

		///<summary>Creates a new shape for a given ad type.</summary>
		internal Shape CreateAdShape(AdType type) {
			if (type.AdsPerPage > 1) {
				Slide targetSlide = GetLastSlide(type);							//Get the last slide that contains this ad type.  If it has room, the ad will go there.
				if (targetSlide != null) {
					for (int n = 1; n <= type.AdsPerPage; n++) {				//For each ad (potential placeholder) on the last slide,
						if (n > targetSlide.Shapes.Placeholders.Count) 			//If the placeholder does not exist on this slide,
							return targetSlide.Shapes.AddPlaceholder(PpPlaceholderType.ppPlaceholderBody, -1, -1, -1, -1);

						if (GetAd(targetSlide.Shapes.Placeholders[n]) == null) 	//If the placeholder exists but has no ad,
							return targetSlide.Shapes.Placeholders[n];			//Use it.
					}
				}
			}
			//If we got here, there was no previous slide or it's full.
			//Either way, we need to insert a new slide.

			var newSlide = Presentation.InsertSlide(type.Name, GetSlideIndex(type));
			newSlide.Tags.Add(TagAdType, type.Name);

			//Delete placeholders for other ads on the
			//slide (For full pages, this is a no-op).
			for (int n = 2; n <= type.AdsPerPage; n++)
				newSlide.Shapes.Placeholders[2].Delete();	//Each time a placeholder is deleted, Placeholders[2] becomes the next one.

			return newSlide.Shapes.Placeholders[1];
		}

		///<summary>Gets the slide position of a new ad of the specified type.</summary>
		///<param name="type">The ad type to position.</param>
		///<returns>The slide index: a number between 1 and 1 + the number of slides, that can be passed to Slides.Add().</returns>
		///<remarks>The function is called by InsertAd and contains the code that position ads within the journal.  It works around any special pages at the beginning.</remarks>
		private int GetSlideIndex(AdType type) {
			//Get the number of slides with ads that precede ours,
			//(including special pages with no ad type), then add 
			//one to get a one-based index.
			return 1 + Presentation.Slides.Items()
				.TakeWhile(s => s.AdType() == null || s.AdType().Id <= type.Id)
				.Count();
		}
		#endregion
	}
	static class PowerPointJournalExtensions {
		///<summary>Gets the type of the ads on a slide, or null if the slide does not contain ads.</summary>
		public static AdType AdType(this Slide slide) {
			return Names.AdTypes.FirstOrDefault(a => a.Name == slide.Tags[JournalPresentation.TagAdType]);
		}
	}
}
