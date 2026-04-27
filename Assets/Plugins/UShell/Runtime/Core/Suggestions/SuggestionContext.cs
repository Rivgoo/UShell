namespace UShell.Runtime.Core.Suggestions
{
	/// <summary>
	/// Encapsulates the runtime state when a user requests autocomplete suggestions (e.g., by pressing Tab).
	/// </summary>
	public readonly struct SuggestionContext
	{
		/// <summary>
		/// The partial string the user has currently typed for this parameter. 
		/// Can be empty if they haven't typed anything yet.
		/// </summary>
		public string PartialValue { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SuggestionContext"/> struct.
		/// </summary>
		public SuggestionContext(string partialValue)
		{
			PartialValue = partialValue ?? string.Empty;
		}
	}
}