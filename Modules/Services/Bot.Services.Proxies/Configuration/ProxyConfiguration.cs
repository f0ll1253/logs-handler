namespace Bot.Services.Proxies.Configuration {
	public class ProxyConfiguration {
		// Multithreading:Proxy:MaxThreads
		public int MaxThreads { get; set; }
		
		public bool OnInCheck { get; set; } = false;
		public bool OnOutCheck { get; set; } = false;
		public string CheckUrl { get; set; } = "https://api.ipify.org";
		public int CheckTimeout { get; set; } = 2000;
	}
}