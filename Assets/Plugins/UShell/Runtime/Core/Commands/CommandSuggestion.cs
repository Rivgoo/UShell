#nullable enable

namespace UShell.Runtime.Core.Commands
{
	public readonly struct CommandSuggestion
	{
		public string MatchText { get; }
		public string DisplayText { get; }
		public CommandSignature? Signature { get; }
		public string Description { get; }
		public int Score { get; }

		public CommandSuggestion(string matchText, string displayText, CommandSignature? signature, string description, int score)
		{
			MatchText = matchText ?? string.Empty;
			DisplayText = displayText ?? string.Empty;
			Signature = signature;
			Description = description ?? string.Empty;
			Score = score;
		}
	}
}