using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShomreiTorah.Data;
using Microsoft.Office.Interop.PowerPoint;

namespace ShomreiTorah.Journal {
	///<summary>Represents a single ad in a journal presentation.</summary>
	public sealed class AdShape {
		AdType adType;
		internal AdShape(JournalPresentation owner, Shape shape, JournalAd row) {
			Presentation = owner;
			Shape = shape;
			Row = row;
			adType = Names.AdTypes.First(t => t.Name == row.AdType);
		}

		///<summary>Gets the presentation containing the ad.</summary>
		public JournalPresentation Presentation { get; internal set; }
		///<summary>Gets the Singularity row containing data about the ad.</summary>
		public JournalAd Row { get; private set; }
		///<summary>Gets the PowerPoint textbox that contains the ad text.</summary>
		public Shape Shape { get; internal set; }

		///<summary>Gets or sets the ad type.</summary>
		public AdType AdType {
			get { return adType; }
			set {
				if (value == null) throw new ArgumentNullException("value");
				if (value == adType) return;
				Presentation.ChangeAdType(this, value, t => this.adType = t);
			}
		}

		///<summary>Deletes this ad from the journal.</summary>
		///<remarks>Both the shape and the Singularity row will be deleted.
		///Any associated pledges, payments, or seating reservations will not be deleted.</remarks>
		public void Delete() { Presentation.DeleteAd(this); }
	}
}
