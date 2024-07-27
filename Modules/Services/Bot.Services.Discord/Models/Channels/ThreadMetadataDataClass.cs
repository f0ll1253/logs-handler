using Newtonsoft.Json;

namespace Bot.Services.Discord.Models.Channels {
	[JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
	public class ThreadMetadataDataClass {
		[JsonProperty("archived")]
		public bool Archived { get; set; }
			
		[JsonProperty("auto_archive_duration")]
		public int AutoArchiveDuration { get; set; }
			
		[JsonProperty("archive_timestamp")]
		public DateTimeOffset ArchiveTimestamp { get; set; }
			
		[JsonProperty("locked")]
		public bool Locked { get; set; }
			
		[JsonProperty("invitable")]
		public bool? Invitable { get; set; }
			
		[JsonProperty("create_timestamp")]
		public DateTimeOffset CreateTimestamp { get; set; }
	}
}