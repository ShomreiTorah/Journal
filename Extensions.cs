using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShomreiTorah.Journal {
	static class Extensions {
		//public static TValue? GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue? unused = null) where TValue : struct {
		//    TValue retVal;
		//    return dict.TryGetValue(key, out retVal) ? retVal : new TValue?();
		//}
		///<summary>Gets a value from a dictionary, or null if the key is not in the dictionary.</summary>
		public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : class {
			TValue retVal;
			return dict.TryGetValue(key, out retVal) ? retVal : null;
		}
	}
}
