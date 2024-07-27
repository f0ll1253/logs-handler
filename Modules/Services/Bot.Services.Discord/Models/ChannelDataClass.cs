using Bot.Services.Discord.Models.Channels;

using Newtonsoft.Json;

namespace Bot.Services.Discord.Models {
	[JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
	public class ChannelDataClass {
			[JsonProperty("id")]
			public string Id { get; set; }
			
			[JsonProperty("type")]
			public ChannelTypes Type { get; set; }
			
			[JsonProperty("guild_id")]
			public string? GuildId { get; set; }
			
			[JsonProperty("position")]
			public int Position { get; set; }

			[JsonProperty("permission_overwrites")]
			public ICollection<OverwriteDataClass> PermissionOverwrites { get; set; } = [];
			
			[JsonProperty("name")]
			public string? Name { get; set; }
			
			[JsonProperty("topic")]
			public string? Topic { get; set; }
			
			[JsonProperty("nsfw")]
			public bool? Nsfw { get; set; }
			
			[JsonProperty("last_message_id")]
			public string? LastMessageId { get; set; }
			
			[JsonProperty("bitrate")]
			public int? Bitrate { get; set; }
			
			[JsonProperty("user_limit")]
			public int? UserLimit { get; set; }
			
			[JsonProperty("rate_limit_per_user")]
			public int? RateLimitPerUser { get; set; }

			[JsonProperty("recipients")]
			public ICollection<User> Recipients { get; set; } = [];
			
			[JsonProperty("icon")]
			public string? IconHash { get; set; }
			
			[JsonProperty("owner_id")]
			public string? OwnerId { get; set; }
			
			[JsonProperty("application_id")]
			public string? ApplicationId { get; set; }
			
			[JsonProperty("managed")]
			public bool? Managed { get; set; }
			
			[JsonProperty("parent_id")]
			public string? ParentId { get; set; }
			
			[JsonProperty("last_pin_timestamp")]
			public DateTimeOffset LastPinTimestamp { get; set; }
			
			[JsonProperty("rtc_region")]
			public string? RtcRegion { get; set; }
			
			[JsonProperty("video_quality_mode")]
			public int? VideoQualityMode { get; set; }
			
			[JsonProperty("message_count")]
			protected int? MessageCount { get; set; }
			
			[JsonProperty("member_count")]
			public int? MemberCount { get; set; }
			
			[JsonProperty("thread_metadata")]
			public ThreadMetadataDataClass? ThreadMetadata { get; set; }
			
			[JsonProperty("member")]
			public ThreadMemberDataClass? ThreadMember { get; set; }
			
			[JsonProperty("default_auto_archive_duration")]
			public int? DefaultAutoArchiveDuration { get; set; }
			
			[JsonProperty("permissions")]
			public string? Permissions { get; set; }
			
			[JsonProperty("flags")]
			public ChannelFlags Flags { get; set; }
			
			[JsonProperty("total_message_sent")]
			public int? TotalMessageSent { get; set; }

			[JsonProperty("available_tags")]
			public ICollection<ForumTagDataClass> AvailableTags { get; set; } = [];

			[JsonProperty("applied_tags")]
			public ICollection<string> AppliedTags { get; set; } = [];
			
			[JsonProperty("default_reaction_emoji")]
			public DefaultReactionDataClass? DefaultReaction { get; set; }
			
			[JsonProperty("default_thread_rate_limit_per_user")]
			public int? DefaultThreadRateLimitPerUser { get; set; }
			
			[JsonProperty("default_sort_order")]
			public int? DefaultSortOrder { get; set; }
			
			[JsonProperty("default_forum_layout")]
			public int? DefaultForumLayout { get; set; }
		}
		
		public enum ChannelTypes {
			/// <summary>
			/// a text channel within a server
			/// </summary>
			GUILD_TEXT = 0,
				
			/// <summary>
			/// a direct message between users
			/// </summary>
			DM,
				
			/// <summary>
			/// a voice channel within a server
			/// </summary>
			GUILD_VOICE,
				
			/// <summary>
			/// a direct message between multiple users
			/// </summary>
			GROUP_DM,
				
			/// <summary>
			/// an organizational category that contains up to 50 channels
			/// </summary>
			GUILD_CATEGORY,
				
			/// <summary>
			/// a channel that users can follow and crosspost into their own server (formerly news channels)
			/// </summary>
			GUILD_ANNOUNCEMENT,
				
			/// <summary>
			/// a channel that users can follow and crosspost into their own server (formerly news channels)
			/// </summary>
			ANNOUNCEMENT_THREAD,
				
			/// <summary>
			/// a temporary sub-channel within a GUILD_TEXT or GUILD_FORUM channel
			/// </summary>
			PUBLIC_THREAD,
				
			/// <summary>
			/// a temporary sub-channel within a GUILD_TEXT channel that is only viewable by those invited and those with the MANAGE_THREADS permission
			/// </summary>
			PRIVATE_THREAD,
				
			/// <summary>
			/// a voice channel for hosting events with an audience
			/// </summary>
			GUILD_STAGE_VOICE,
				
			/// <summary>
			/// he channel in a hub containing the listed servers
			/// </summary>
			GUILD_DIRECTORY,
				
			/// <summary>
			/// Channel that can only contain threads
			/// </summary>
			GUILD_FORUM,
				
			/// <summary>
			/// Channel that can only contain threads, similar to `GUILD_FORUM` channels
			/// </summary>
			GUILD_MEDIA,
		}
		
		[Flags]
		public enum ChannelFlags {
			PINNED = 1,
			REQUIRE_TAG = 4,
			HIDE_MEDIA_DOWNLOAD_OPTIONS = 15
		}
}