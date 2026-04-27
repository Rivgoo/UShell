namespace UShell.Runtime.Unity.UI
{
	/// <summary>
	/// Represents the interaction state of the console's input field.
	/// </summary>
	public enum ConsoleInputMode : byte
	{
		/// <summary>The input field is active and accepts standard command typing.</summary>
		Standard = 0,

		/// <summary>The input field is greyed out and unclickable (used when a command is computing).</summary>
		Locked = 1,

		/// <summary>The input field expects an answer to an interactive prompt (e.g., Yes/No).</summary>
		Prompt = 2
	}
}