namespace UShell.Runtime.Core.Commands
{
	public readonly struct CommandSuggestion
	{
		public string MatchText { get; }
		public CommandSignature Signature { get; }

		public CommandSuggestion(string matchText, CommandSignature signature)
		{
			MatchText = matchText;
			Signature = signature;
		}
	}
}