namespace UShell.Runtime.Core.Suggestions
{
	public readonly struct SuggestionContext
	{
		public string PartialValue { get; }

		public SuggestionContext(string partialValue)
		{
			PartialValue = partialValue ?? string.Empty;
		}
	}
}