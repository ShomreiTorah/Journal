using System;
using System.Collections.Generic;
using System.Globalization;
using ShomreiTorah.Data;

namespace ShomreiTorah.Journal {
	static class Extensions {
		const string ExternalSourcePrefix = "Journal";

		public static int? GetJournalYear(this Pledge pledge) { return GetJournalYear(pledge.ExternalSource); }
		public static int? GetJournalYear(this Payment payment) { return GetJournalYear(payment.ExternalSource); }
		public static int? GetJournalYear(string externalSource) {
			if (externalSource == null || !externalSource.StartsWith(ExternalSourcePrefix, StringComparison.OrdinalIgnoreCase))
				return null;
			return int.Parse(externalSource.Substring(ExternalSourcePrefix.Length), CultureInfo.InvariantCulture);
		}

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
