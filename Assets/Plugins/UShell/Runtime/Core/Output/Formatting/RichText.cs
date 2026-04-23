namespace UShell.Runtime.Core.Output.Formatting
{
	public static class RichText
	{
		public static string Color(string text, string hexCode)
		{
			if (string.IsNullOrEmpty(text)) return string.Empty;
			return $"<color={hexCode}>{text}</color>";
		}

		public static string Bold(string text)
		{
			if (string.IsNullOrEmpty(text)) return string.Empty;
			return $"<b>{text}</b>";
		}

		public static string Italic(string text)
		{
			if (string.IsNullOrEmpty(text)) return string.Empty;
			return $"<i>{text}</i>";
		}
	}
}