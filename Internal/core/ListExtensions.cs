using System.Collections.Generic;

namespace hexegeer.internallib {
	public static class ListExtensions {
		public static List<R> Map<T, R>(this List<T> list, System.Func<T, R> f) {
			List<R> mapped = new List<R>();
			foreach(T element in list) { mapped.Add(f(element)); }
			return mapped;
		}

		public static List<T> Distinct<T, R>(this List<T> list, System.Func<T, R> f) {
			List<T> distinct = new List<T>();
			List<R> keys = new List<R>();
			foreach(T element in list) {
				R key = f(element);
				if (!keys.Contains(key)) {
					keys.Add(key);
					distinct.Add(element);
				}
			}
			return distinct;
		}

		public static List<T> Distinct<T>(this List<T> list) {
			return Distinct(list, _ => _);
		}
	}
}