namespace Bot.Commands.Markups {
	public static class Markup_Admin {
		public static ReplyInlineMarkup StartMarkup { get; } = new() {
			rows = Markup_User.StartMarkupRows("admin_start")
							  .Append(
								  new() {
									  buttons = [
										  new KeyboardButtonCallback {
											  text = "Settings",
											  data = "admin_settings".Utf8()
										  }
									  ]
								  }
							  )
							  .ToArray()
		};

		public static ReplyInlineMarkup SettingsMarkup { get; } = new() // TODO append settings
		{
			rows = [
				new() {
					buttons = [
						new KeyboardButtonCallback {
							text = "Back",
							data = "admin_start".Utf8()
						}
					]
				}
			]
		};
	}
}