using System.Collections.Generic;

namespace TodoHarvester.Infrastructure
{
	internal static class ListExtensions
	{
		public static IList<T> AddIfNotNull<T>(this IList<T> list, T item) where T : class
		{
			if (item != null)
			{
				list.Add(item);
			}

			return list;
		}
	}
}