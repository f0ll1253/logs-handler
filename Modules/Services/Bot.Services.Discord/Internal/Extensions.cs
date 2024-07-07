namespace Bot.Services.Discord.Internal {
	internal static class Extensions {
		public static bool AtIndex(this uint value, int shift) => (value & (1 << shift)) >> shift == 1;
	}
}