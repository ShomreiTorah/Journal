using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.PowerPoint;

namespace ShomreiTorah.Journal {
	///<summary>Contains extension methods for PowerPoint COM objects.</summary>
	static class PowerPointExtensions {
		///<summary>Inserts a slide with the specified master.</summary>
		///<param name="masterName">The name of the master to use.</param>
		///<param name="targetIndex">The 1-based index of the new slide.</param>
		///<returns>The new slide.</returns>
		public static Slide InsertSlide(this Presentation presentation, string masterName, int targetIndex) {
			return presentation.Slides.AddSlide(targetIndex, presentation.SlideMaster.CustomLayouts.GetLayout(masterName));
		}

		///<summary>Gets the CustomLayout with the specified name.</summary>
		///<param name="layoutName">The name of the layout to look for.</param>
		///<returns>The <typeparamref name="PowerPoint.CustomLayout"/> object.</returns>
		///<remarks>The indexer for <typeparamref name="PowerPoint.CustomLayouts"/> does not accept strings.
		///Therefore, I wrote this function to search it for the given layout.  
		///It is called in InsertSlide().  The enumerator for <typeparamref name="PowerPoint.CustomLayouts"/>
		///returns an unknown <typeparamref name="System.ComObject"/> that cannot be
		///casted to <typeparamref name="PowerPoint.CustomLayouts"/>. (QueryInterface() returns unsupported)</remarks>
		public static CustomLayout GetLayout(this CustomLayouts customLayouts, string layoutName) {
			for (int n = 1; n <= customLayouts.Count; n++) {
				if (customLayouts[n].Name == layoutName)
					return customLayouts[n];
			}
			throw new ArgumentException("Layout " + layoutName + " not found.", "layoutName");
		}

		public static IEnumerable<Slide> Items(this Slides slides) { return slides.Cast<Slide>(); }
		public static IEnumerable<Shape> Items(this Placeholders placeholders) { return placeholders.Cast<Shape>(); }
	}
}
