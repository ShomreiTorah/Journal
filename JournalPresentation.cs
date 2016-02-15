using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Microsoft.Office.Interop.PowerPoint;
using ShomreiTorah.Common;
using ShomreiTorah.Data;
using ShomreiTorah.Singularity;
using ShomreiTorah.WinForms;

namespace ShomreiTorah.Journal {
	///<summary>Manages a PowerPoint presentation containing a journal.</summary>
	///<remarks>
	/// Ad shapes are named after their JournalAd.AdId (a GUID)
	/// Ad slides have a Tag["AdType"] equal to the AdType.Name
	///</remarks>
	public sealed class JournalPresentation {
		class AdShapeCollection : KeyedCollection<string, AdShape> {
			protected override string GetKeyForItem(AdShape item) { return item.Row.AdId.ToString(); }

			public AdShape GetAd(string id) {
				if (base.Dictionary == null)
					return this.FirstOrDefault(a => a.Row.AdId.ToString() == id);
				AdShape retVal;
				base.Dictionary.TryGetValue(id, out retVal);
				return retVal;
			}
		}

		const string TagYear = "JournalYear";
		internal const string TagAdType = "AdType";
		readonly AdShapeCollection writableAds = new AdShapeCollection();

		///<summary>Checks whether a PowerPoint presentation contains a Singularity journal.</summary>
		public static int? GetYear(Presentation presentation) {
			if (presentation == null) throw new ArgumentNullException("presentation");
			var tag = presentation.Tags[TagYear];
			return String.IsNullOrEmpty(tag) ? new int?() : int.Parse(tag, CultureInfo.InvariantCulture);
		}
		///<summary>Marks a PowerPoint presentation as being a journal.</summary>
		///<remarks>After calling this method, you can create a JournalPresentation object from the presentation.</remarks>
		public static void MakeJournal(Presentation presentation, int year) {
			if (presentation == null) throw new ArgumentNullException("presentation");
			presentation.Tags.Add(TagYear, year.ToString(CultureInfo.InvariantCulture));
		}
		///<summary>Unmarks a PowerPoint presentation as being a journal.</summary>
		public static void KillJournal(Presentation presentation) {
			if (presentation == null) throw new ArgumentNullException("presentation");
			presentation.Tags.Delete(TagYear);
		}

		///<summary>Creates a JournalPresentation from an existing PowerPoint presentation and a Singularity database.</summary>
		public JournalPresentation(Presentation presentation, DataContext dc) {
			if (presentation == null) throw new ArgumentNullException("presentation");
			if (dc == null) throw new ArgumentNullException("dc");

			Year = GetYear(presentation).Value;
			MelaveMalka = dc.Table<MelaveMalkaInfo>().Rows.FirstOrDefault(i => i.Year == Year);
			Ads = new ReadOnlyCollection<AdShape>(writableAds);
			Presentation = presentation;
			AdsTable = dc.Table<JournalAd>();

			var idMap = AdsTable.Rows.Where(ad => ad.Year == Year).ToDictionary(ad => ad.AdId.ToString());
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
		///<summary>Gets the information about the Melava Malka managed by this instance.</summary>
		public MelaveMalkaInfo MelaveMalka { get; private set; }
		///<summary>Gets the presentation containing the ads.</summary>
		public Presentation Presentation { get; private set; }
		///<summary>Gets the Singularity table containing the ad data.</summary>
		public TypedTable<JournalAd> AdsTable { get; private set; }

		///<summary>Gets the ads in the journal.</summary>
		public ReadOnlyCollection<AdShape> Ads { get; private set; }
		///<summary>Gets the ad describing the given shape, if any.</summary>
		public AdShape GetAd(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			return writableAds.GetAd(shape.Name);
		}
		///<summary>Gets the AdShape object containing the given JournalAd row.</summary>
		public AdShape GetAd(JournalAd ad) {
			if (ad == null) throw new ArgumentNullException("ad");
			return writableAds.GetAd(ad.AdId.ToString());
		}

		///<summary>Confirms that the user is not mistakenly modifying an old journal.</summary>
		///<returns>False if the user is making a mistake.</returns>
		public bool ConfirmModification() {
			if (MelaveMalka == null) {
				Dialog.Inform("Please enter the Melave Malka info in the Billing.");
			} else if (DateTime.Now > MelaveMalka.MelaveMalkaDate) {
				if (!Dialog.Warn("Are you sure you want to modify the " + Year + " journal post-facto?"))
					return false;
			}
			return true;
		}

		#region Creation
		///<summary>Creates a new ad in the journal.</summary>
		public AdShape CreateAd(AdType type) {
			if (type == null) throw new ArgumentNullException("type");
			var shape = CreateAdShape(type);

			var row = new JournalAd {
				AdType = type.Name,
				DateAdded = DateTime.Now,
				Year = Year,
				ExternalId = 1 + (AdsTable.Rows.Where(ad => ad.Year == Year).Max(ad => (int?)ad.ExternalId) ?? 0)	//I need to use int? to handle an empty sequence
			};
			shape.Name = row.AdId.ToString();
			AdsTable.Rows.Add(row);
			var retVal = new AdShape(this, shape, row);
			writableAds.Add(retVal);
			return retVal;
		}

		///<summary>Creates a new shape for a given ad type.</summary>
		Shape CreateAdShape(AdType type) {
			if (type.AdsPerPage > 1) {
				//For fractional ad types, see if there's room for one more
				//ad in the last slide of that type (if any).
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

			var newSlide = Presentation.InsertSlide(type.Name, GetAdPosition(type));
			newSlide.Tags.Add(TagAdType, type.Name);

			//Delete placeholders for other ads on the
			//slide (For full pages, this is a no-op).
			for (int n = 2; n <= type.AdsPerPage; n++)
				newSlide.Shapes.Placeholders[2].Delete();	//Each time a placeholder is deleted, Placeholders[2] becomes the next one.

			return newSlide.Shapes.Placeholders[1];
		}

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
		///<summary>Gets the slide position of a new ad of the specified type.</summary>
		///<param name="type">The ad type to position.</param>
		///<returns>The slide index: a number between 1 and 1 + the number of slides, that can be passed to Slides.Add().</returns>
		private int GetAdPosition(AdType type) {
			//Get the number of slides with ads that precede ours,
			//(including special pages with no ad type), then add
			//one to get a one-based index.
			return 1 + Presentation.Slides.Items()
				.TakeWhile(s => s.AdType() == null || s.AdType().Index <= type.Index)
				.Count();
		}
		#endregion

		#region Deletion
		///<summary>Deletes an ad from the journal.</summary>
		///<remarks>Both the shape and the Singularity row will be deleted.
		///Any associated pledges, payments, or seating reservations will not be deleted.</remarks>
		public void DeleteAd(AdShape ad) {
			if (ad == null) throw new ArgumentNullException("ad");
			if (ad.Presentation != this) throw new ArgumentException("Ad must be in the journal", "ad");
			DeleteAdShape(ad);
			ad.Row.RemoveRow();
			ad.Presentation = null;
			writableAds.Remove(ad);
		}
		///<summary>Deletes an ad's shape.</summary>
		///<remarks>The ad's row is not affected.</remarks>
		private void DeleteAdShape(AdShape ad) {
			if (ad.AdType.AdsPerPage == 1) {						//If it is a full-size ad (as opposed to halves or quarters),
				((Slide)ad.Shape.Parent).Delete();					//Delete its slide.
			} else {												//If it is a fractional ad,
				//Find the last ad of our type and delete
				//it.  If it isn't the ad we're trying to
				//delete, move it to our ad, then delete
				//its original.
				Slide lastSlide = GetLastSlide(ad.AdType);
				AdShape lastAd = lastSlide.Shapes.Placeholders.Items()
						.Take(ad.AdType.AdsPerPage).Select(GetAd).Last(a => a != null);	//Get the last non-null ad on the slide

				if (lastAd == ad)
					DeleteFractionalAdShape(ad.Shape);
				else {
					//If the last ad isn't the one we're trying to delete,
					//replace our ad with the last ad before deleting the
					//last ad.
					using (new ClipboardScope()) {
						lastAd.Shape.TextFrame.TextRange.Copy();	//Copy the text of the last ad.
						ad.Shape.TextFrame.TextRange.Delete();		//Delete the text of the old ad
						ad.Shape.TextFrame.TextRange.Paste();		//Paste the text of the last ad in to the old ad.
					}
					ad.Shape.Name = lastAd.Row.AdId.ToString();		//Rename our ad's shape to its new ad.
					DeleteFractionalAdShape(lastAd.Shape);			//Delete the last ad's original shape,
					lastAd.Shape = ad.Shape;						//Then set its shape to our ad's shape
				}
			}
			ad.Shape = null;
		}

		///<summary>Deletes an ad shape, and, if it is the only ad on its slide, its slide.</summary>
		///<param name="adShape">The shape to delete.</param>
		///<remarks>This function is used to delete fractional ads and ensure that a blank page is not left over.</remarks>
		private void DeleteFractionalAdShape(Shape adShape) {
			Slide slide = (Slide)adShape.Parent;

			//If the slide has no non-null placeholders
			//other than the one we're deleting, delete
			//the entire slide.
			if (slide.Shapes.Placeholders.Count == 1
			 || !slide.Shapes.Placeholders.Items()
							.Take(slide.AdType().AdsPerPage)
							.Any(s => s != adShape && GetAd(s) != null)
				)
				slide.Delete();
			else {
				adShape.Delete();
				//After deleting the shape, its placeholder will remain.
				//Clean those up.
				foreach (var ph in slide.Shapes.Placeholders.Items()) {
					if (ph.Name.Contains("Placeholder") && String.IsNullOrWhiteSpace(ph.TextFrame.TextRange.Text))
						ph.Delete();
				}
			}
		}
		#endregion

		///<summary>Changes an ad's type and updates the presentation appropriately.</summary>
		///<remarks>The ad's pledges or payments are not affected.</remarks>
		public void ChangeAdType(AdShape ad, AdType newAdType) {
			if (ad == null) throw new ArgumentNullException("ad");
			if (ad.Presentation != this) throw new ArgumentException("Ad must be in the journal", "ad");
			ad.AdType = newAdType;
		}

		//This overload is called from the AdShape.AdType setter.
		internal void ChangeAdType(AdShape ad, AdType newAdType, Action<AdType> typeSetter) {
			if (ad.AdType == newAdType) return;
			Slide slide = (Slide)ad.Shape.Parent;

			if (ad.AdType.AdsPerPage == 1 && newAdType.AdsPerPage == 1) {
				bool isPlaceholder = ad.Shape.Type == Microsoft.Office.Core.MsoShapeType.msoPlaceholder;
				slide.CustomLayout = Presentation.SlideMaster.CustomLayouts.GetLayout(newAdType.Name);
				slide.Tags.Add(TagAdType, newAdType.Name);	//Add overwrites existing tags.

				int newPos = GetAdPosition(newAdType);
				//If it is after than the current position,  decrement it
				//to allow for the ad's removal from its current location
				if (newPos > slide.SlideIndex) newPos--;
				slide.MoveTo(newPos);
				// If the ad had no text, its placeholder Shape object is
				// replaced by the layout change.  Change the ad to point
				// to the new placeholder.
				if (isPlaceholder) {
					ad.Shape = slide.Shapes.Placeholders[1];
					ad.Shape.Name = ad.Row.AdId.ToString();
				}
			} else {	//If either the new or the old ad types are fractional pages, it must be handled differently.
				//Deleting the shape may involve copying
				//the last ad into its place. Therefore,
				//I cannot delete it inside the scope in
				//which I copy its text to the new shape
				//Instead, I create the new shape, kill
				//the old one,  then attach the new one.
				Shape newShape;
				using (new ClipboardScope()) {
					ad.Shape.TextFrame.TextRange.Copy();

					newShape = CreateAdShape(newAdType);
					newShape.TextFrame.TextRange.Paste();
				}
				DeleteAdShape(ad);

				//After deleting the old shape, plug in the new one
				ad.Shape = newShape;
				ad.Shape.Name = ad.Row.AdId.ToString();
			}
			ad.Row.AdType = newAdType.Name;
			typeSetter(newAdType);	//This sets the private field in AdShape
		}
	}
	static class PowerPointJournalExtensions {
		///<summary>Gets the type of the ads on a slide, or null if the slide does not contain ads.</summary>
		public static AdType AdType(this Slide slide) {
			return Names.AdTypes.FirstOrDefault(a => a.Name == slide.Tags[JournalPresentation.TagAdType]);
		}
	}
}
