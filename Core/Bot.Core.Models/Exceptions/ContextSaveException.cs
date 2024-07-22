namespace Bot.Core.Models.Exceptions {
	public class ContextSaveException(Exception e, string area) : Exception($"[{area}] Error while saving, for more details check inner exception", e);
}