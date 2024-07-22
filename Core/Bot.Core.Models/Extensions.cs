namespace Bot.Core.Models {
	public static class Extensions {
		public static IEnumerable<IGrouping<int, T>> GroupBy<T>(this IEnumerable<T> arr, int count) =>
				arr.Select((x, i) => (x, i))
				   .GroupBy(x => x.i / count, x => x.x);

		#region Multithreading
		
		// Limited threads
		public static IEnumerable<T> WithThreads<T>(this IEnumerable<T> arr, Func<T, Task> action, int max_threads, Predicate<IEnumerable<T>>? stop_prediction = null) {
			foreach (var group in arr.GroupBy(max_threads)) {
				if (stop_prediction?.Invoke(group) ?? false) {
					break;
				}
				
				group.WithThreads(action);
			}

			return arr;
		}
		public static IEnumerable<T> WithThreads<T>(this IEnumerable<T> arr, Action<T> action, int max_threads, Predicate<IEnumerable<T>>? stop_prediction = null) {
			foreach (var group in arr.GroupBy(max_threads)) {
				if (stop_prediction?.Invoke(group) ?? false) {
					break;
				}

				group.WithThreads(action);
			}

			return arr;
		}
		
		// Unlimited threads
		public static IEnumerable<T> WithThreads<T>(this IEnumerable<T> arr, Func<T, Task> action) {
			var threads = arr.Count();

			return arr.WithThreads(
				(@event, item) => async () => {
					await action.Invoke(item);

					if (Interlocked.Decrement(ref threads) == 0) {
						@event.Set();
					}
				}
			);
		}
		
		public static IEnumerable<T> WithThreads<T>(this IEnumerable<T> arr, Action<T> action) {
			var threads = arr.Count();
			
			return arr.WithThreads(
				(@event, item) => () => {
					action.Invoke(item);

					if (Interlocked.Decrement(ref threads) == 0) {
						@event.Set();
					}
				}
			);
		}

		/// <param name="start">ThreadStart with passed tuple of AutoResetEvent, Threads variable and T</param>
		private static IEnumerable<T> WithThreads<T>(this IEnumerable<T> arr, Func<AutoResetEvent, T, ThreadStart> start) {
			var @event = new AutoResetEvent(false);
            
			foreach (var item in arr) {
				new Thread(start.Invoke(@event, item)).Start();
			}

			@event.WaitOne();

			return arr;
		}

		#endregion
	}
}