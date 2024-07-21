namespace Bot.Core.Models {
	public static class Extensions {
		public static IEnumerable<IGrouping<int, T>> GroupBy<T>(this IEnumerable<T> arr, int count) =>
				arr
						.Select((x, i) => (x, i))
						.GroupBy(x => x.i / count, x => x.x);
	}
}