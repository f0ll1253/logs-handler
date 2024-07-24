namespace Bot.Core.Models {
	public static class Extensions {
		public static IEnumerable<IGrouping<int, T>> GroupBy<T>(this IEnumerable<T> arr, int count) =>
				arr.Select((x, i) => (x, i))
				   .GroupBy(x => x.i / count, x => x.x);

		#region Multithreading
		
		// Limited threads
		public static IEnumerable<T> WithThreads<T>(this IEnumerable<T> arr, Func<T, int, Task> action, int max_threads, Predicate<IEnumerable<T>>? stop_prediction = null) {
			foreach (var group in arr.GroupBy(max_threads)) {
				if (stop_prediction?.Invoke(group) ?? false) {
					break;
				}
				
				group.WithThreads(action);
			}

			return arr;
		}
		public static IEnumerable<T> WithThreads<T>(this IEnumerable<T> arr, Action<T, int> action, int max_threads, Predicate<IEnumerable<T>>? stop_prediction = null) {
			foreach (var group in arr.GroupBy(max_threads)) {
				if (stop_prediction?.Invoke(group) ?? false) {
					break;
				}

				group.WithThreads(action);
			}

			return arr;
		}
		
		// Unlimited threads
		public static IEnumerable<T> WithThreads<T>(this IEnumerable<T> arr, Func<T, int, Task> action) {
			var threads = arr.Count();

			return arr.WithThreads(
				(@event, index, item) => async () => {
					await action.Invoke(item, index);

					if (Interlocked.Decrement(ref threads) == 0) {
						@event.Set();
					}
				}
			);
		}
		
		public static IEnumerable<T> WithThreads<T>(this IEnumerable<T> arr, Action<T, int> action) {
			var threads = arr.Count();
			
			return arr.WithThreads(
				(@event, index, item) => () => {
					action.Invoke(item, index);

					if (Interlocked.Decrement(ref threads) == 0) {
						@event.Set();
					}
				}
			);
		}

		private static IEnumerable<T> WithThreads<T>(this IEnumerable<T> arr, Func<AutoResetEvent, int, T, ThreadStart> start) {
			var @event = new AutoResetEvent(false);
            
			var arr_static = arr.ToArray();

			for (int i = 0; i < arr_static.Length; i++) {
				new Thread(start.Invoke(@event, i, arr_static[i])).Start();
			}

			@event.WaitOne();

			return arr;
		}

		#endregion
	}
}