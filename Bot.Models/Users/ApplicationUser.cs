using Bot.Models.Abstractions;

using TL;

namespace Bot.Models.Users {
	public class ApplicationUser : IEntity<long> {
		public long Id { get; set; }

		// Foreign
		public List<string> Roles { get; set; }
		public List<Task> Tasks { get; set; }

		public static implicit operator ApplicationUser(User user) {
			return new() {
				Id = user.id
			};
		}
	}
}