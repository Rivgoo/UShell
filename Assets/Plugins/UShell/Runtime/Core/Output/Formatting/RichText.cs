namespace UShell.Runtime.Core.Output.Formatting
{
	/// <summary>
	/// Provides lightweight static helper methods for generating Unity-compatible Rich Text formatting tags.
	/// </summary>
	public static class RichText
	{
		/// <summary>
		/// Wraps the provided text in a <c>&lt;color&gt;</c> tag using the specified hex code.
		/// </summary>
		/// <param name="text">The string to be colored.</param>
		/// <param name="hexCode">A valid HTML/Unity hex color string (e.g., "#FF0000" or "#FFFFFFFF").</param>
		/// <returns>The formatted rich text string.</returns>
		public static string Color(string text, string hexCode)
		{
			if (string.IsNullOrEmpty(text)) return string.Empty;
			return $"<color={hexCode}>{text}</color>";
		}

		/// <summary>
		/// Wraps the provided text in a <c>&lt;b&gt;</c> bold tag.
		/// </summary>
		/// <param name="text">The string to be bolded.</param>
		/// <returns>The formatted rich text string.</returns>
		public static string Bold(string text)
		{
			if (string.IsNullOrEmpty(text)) return string.Empty;
			return $"<b>{text}</b>";
		}

		/// <summary>
		/// Wraps the provided text in an <c>&lt;i&gt;</c> italic tag.
		/// </summary>
		/// <param name="text">The string to be italicized.</param>
		/// <returns>The formatted rich text string.</returns>
		public static string Italic(string text)
		{
			if (string.IsNullOrEmpty(text)) return string.Empty;
			return $"<i>{text}</i>";
		}
	}
}