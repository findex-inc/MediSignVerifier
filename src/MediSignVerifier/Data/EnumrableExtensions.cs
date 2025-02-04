using System.Collections.Generic;
using System.Linq;

namespace SignatureVerifier.Data
{
	internal static class EnumrableExtensions
	{
		public static IEnumerable<T> AddIfNotContaints<T>(this IEnumerable<T> source, T entry)
		{
			var set = new HashSet<T>(source);
			if (!set.Contains(entry)) {

				set.Add(entry);
			}

			foreach (var e in set) {

				yield return e;
			}
		}


		public static IEnumerable<T> AddRangeIfNotContaints<T>(this IEnumerable<T> source, IEnumerable<T> entries)
		{
			var set = new HashSet<T>(source);

			foreach (var entry in entries) {

				if (!set.Contains(entry)) {

					set.Add(entry);
				}
			}

			foreach (var e in set) {

				yield return e;
			}
		}


		public static T[] ToArrayOrNull<T>(this IEnumerable<T> source)
		{
			if (!source.Any()) {

				return null;
			}

			return source.ToArray();
		}
	}
}
