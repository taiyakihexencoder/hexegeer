using System.Collections.Generic;

namespace hexegeer.internallib {
	public static class ListExtensions {
		public static List<R> Map<T, R>(this List<T> list, System.Func<T, R> f) {
			List<R> mapped = new List<R>();
			foreach(T element in list) { mapped.Add(f(element)); }
			return mapped;
		}
	}
}