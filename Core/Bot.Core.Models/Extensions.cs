using System.Text;

using Microsoft.Extensions.Configuration;

namespace Bot.Core.Models {
	public static class Extensions {
		public static IEnumerable<IGrouping<int, T>> GroupBy<T>(this IEnumerable<T> arr, int count) =>
				arr.Select((x, i) => (x, i))
				   .GroupBy(x => x.i / count, x => x.x);
		
		public static string GetPath(this IConfiguration config, string service) {
			var path = Path.Combine(config["Files:Root"]!, service);

			Directory.CreateDirectory(path);

			return path;
		}
		
		public static (string name, string path) GetPath(this IConfiguration config, byte[] bytes, Paths type) {
			var name = Encoding.UTF8.GetString(bytes[1..]);

			return (name, Path.Combine(config.GetPath(type.ToString()), name));
		}

		private static readonly EnumerationOptions _search_options = new() {
			MatchCasing = MatchCasing.CaseInsensitive,
			RecurseSubdirectories = false,
			ReturnSpecialDirectories = false,
			IgnoreInaccessible = true
		};
		public static string[] TryGetFilesOf(this string directory, string search_pattern) {
			if (Directory.GetDirectories(directory, search_pattern, _search_options).FirstOrDefault() is not { } folder || Directory.GetFiles(folder) is not {Length:>0} files) {
				return [];
			}

			return files;
		}
		
		#region Multithreading
		
		// Limited threads
		public static IEnumerable<T> WithThreads<T>(this IEnumerable<T> arr, int max_threads, Func<T, int, Task<bool>> prediction, Func<T, int, Task> on_success, Func<T, int, Task>? on_fail = null, Predicate<IEnumerable<T>>? stop_prediction = null) {
			foreach (var group in arr.GroupBy(max_threads)) {
				if (stop_prediction?.Invoke(group) ?? false) {
					break;
				}
				
				group.WithThreads(prediction, on_success, on_fail);
			}

			return arr;
		}
		public static IEnumerable<T> WithThreads<T>(this IEnumerable<T> arr, int max_threads, Func<T, int, bool> predicate, Action<T, int> on_success, Action<T, int>? on_fail = null, Predicate<IEnumerable<T>>? stop_prediction = null) {
			foreach (var group in arr.GroupBy(max_threads)) {
				if (stop_prediction?.Invoke(group) ?? false) {
					break;
				}

				group.WithThreads(predicate, on_success, on_fail);
			}

			return arr;
		}
		
		// Unlimited threads
		public static void WithThreads<T>(this IEnumerable<T> arr, Func<T, int, Task<bool>> predicate, Func<T, int, Task> on_success, Func<T, int, Task>? on_fail = null) {
			var threads = arr.Count();

			arr.WithThreads(
				(@event, index, item) => async () => {
					if (await predicate.Invoke(item, index)) {
						await on_success.Invoke(item, index);
					} else {
						await (on_fail?.Invoke(item, index) ?? Task.CompletedTask);
					}

					if (Interlocked.Decrement(ref threads) == 0) {
						@event.Set();
					}
				}
			);
		}
		
		public static void WithThreads<T>(this IEnumerable<T> arr, Func<T, int, bool> predicate, Action<T, int> on_success, Action<T, int>? on_fail = null) {
			var threads = arr.Count();
			
			arr.WithThreads(
				(@event, index, item) => () => {
					if (predicate.Invoke(item, index)) {
						on_success.Invoke(item, index);
					} else {
						on_fail?.Invoke(item, index);
					}

					if (Interlocked.Decrement(ref threads) == 0) {
						@event.Set();
					}
				}
			);
		}

		private static void WithThreads<T>(this IEnumerable<T> arr, Func<AutoResetEvent, int, T, ThreadStart> start) {
			var @event = new AutoResetEvent(false);
            
			var arr_static = arr.ToArray();

			for (int i = 0; i < arr_static.Length; i++) {
				new Thread(start.Invoke(@event, i, arr_static[i])).Start();
			}

			@event.WaitOne();
		}

		#endregion
	}
	
	public enum Paths {
		Extracted,
		Downloaded
	}
}