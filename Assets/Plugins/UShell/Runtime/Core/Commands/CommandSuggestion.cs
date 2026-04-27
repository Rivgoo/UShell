#nullable enable

namespace UShell.Runtime.Core.Commands
{
	/// <summary>
	/// Represents a scored autocomplete suggestion returned by the registry during user input.
	/// </summary>
	public readonly struct CommandSuggestion
	{
		/// <summary>
		/// The exact text to append or insert into the input field if the user accepts this suggestion.
		/// </summary>
		public string MatchText { get; }

		/// <summary>
		/// The formatted text displayed to the user in the UI suggestion list.
		/// </summary>
		public string DisplayText { get; }

		/// <summary>
		/// The underlying command signature tied to this suggestion, if it represents a command name.
		/// Null if the suggestion is for a parameter value or alias.
		/// </summary>
		public CommandSignature? Signature { get; }

		/// <summary>
		/// A short contextual hint (e.g., "Command", "Parameter") clarifying what the suggestion represents.
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// The algorithmic score determining how closely the suggestion matches the user's input.
		/// Higher values indicate a stronger match.
		/// </summary>
		public int Score { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandSuggestion"/> struct.
		/// </summary>
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