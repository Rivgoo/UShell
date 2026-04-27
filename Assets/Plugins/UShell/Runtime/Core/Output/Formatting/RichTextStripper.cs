using System;
using System.Text;

namespace UShell.Runtime.Core.Output.Formatting
{
	/// <summary>
	/// A utility for parsing and removing Unity Rich Text tags from a string.
	/// </summary>
	/// <remarks>
	/// Crucial for accurately calculating the visual length of strings when building ASCII tables 
	/// or aligning text, as raw formatting tags (like <c>&lt;color=#FFF&gt;</c>) do not occupy visual space.
	/// </remarks>
	public static class RichTextStripper
	{
		private const int MaxTagNameLength = 16;

		/// <summary>
		/// Removes all supported rich text markup tags from the provided string, 
		/// leaving only the printable characters. Respects <c>&lt;noparse&gt;</c> blocks.
		/// </summary>
		/// <param name="richText">The string containing formatting tags.</param>
		/// <returns>A clean string with no formatting tags.</returns>
		public static string Strip(string richText)
		{
			if (string.IsNullOrEmpty(richText))
				return richText;

			ReadOnlySpan<char> span = richText.AsSpan();

			if (span.IndexOf('<') < 0)
				return richText;

			var sb = new StringBuilder(span.Length);
			bool inNoparse = false;
			int i = 0;

			while (i < span.Length)
			{
				char c = span[i];

				if (c != '<')
				{
					sb.Append(c);
					i++;
					continue;
				}

				int closeIdx = IndexOfClosingBracket(span, i + 1);
				if (closeIdx < 0)
				{
					sb.Append(c);
					i++;
					continue;
				}

				ReadOnlySpan<char> tagContent = span.Slice(i + 1, closeIdx - i - 1);

				if (inNoparse)
				{
					if (IsClosingNoparse(tagContent))
					{
						inNoparse = false;
						i = closeIdx + 1;
					}
					else
					{
						sb.Append('<');
						sb.Append(tagContent);
						sb.Append('>');
						i = closeIdx + 1;
					}
					continue;
				}

				if (IsOpenNoparse(tagContent))
				{
					inNoparse = true;
					i = closeIdx + 1;
					continue;
				}

				if (IsKnownTag(tagContent))
				{
					i = closeIdx + 1;
				}
				else
				{
					sb.Append('<');
					sb.Append(tagContent);
					sb.Append('>');
					i = closeIdx + 1;
				}
			}

			string result = sb.ToString();

			return result.Length == richText.Length ? richText : result;
		}

		/// <summary>
		/// Extracts a substring of the raw visual text based on character indices, ignoring the underlying rich text tags.
		/// </summary>
		public static string ExtractRange(string richText, int startChar, int endChar)
		{
			if (string.IsNullOrEmpty(richText) || startChar >= endChar)
				return string.Empty;

			string plain = Strip(richText);

			startChar = Math.Max(0, startChar);
			endChar = Math.Min(plain.Length, endChar);

			if (startChar >= endChar) return string.Empty;

			return plain.Substring(startChar, endChar - startChar);
		}

		private static int IndexOfClosingBracket(ReadOnlySpan<char> span, int from)
		{
			for (int i = from; i < span.Length; i++)
			{
				if (span[i] == '>') return i;
				if (span[i] == '<') return -1;
			}
			return -1;
		}

		private static bool IsOpenNoparse(ReadOnlySpan<char> tagContent)
		{
			return tagContent.Equals("noparse".AsSpan(), StringComparison.OrdinalIgnoreCase);
		}

		private static bool IsClosingNoparse(ReadOnlySpan<char> tagContent)
		{
			if (tagContent.Length < 2 || tagContent[0] != '/') return false;
			return tagContent.Slice(1).Equals("noparse".AsSpan(), StringComparison.OrdinalIgnoreCase);
		}

		private static ReadOnlySpan<char> ExtractTagName(ReadOnlySpan<char> tagContent)
		{
			ReadOnlySpan<char> name = tagContent.Length > 0 && tagContent[0] == '/'
				? tagContent.Slice(1)
				: tagContent;

			int end = 0;
			while (end < name.Length && name[end] != '=' && name[end] != ' ')
				end++;

			return name.Slice(0, Math.Min(end, MaxTagNameLength));
		}

		private static bool IsKnownTag(ReadOnlySpan<char> tagContent)
		{
			ReadOnlySpan<char> name = ExtractTagName(tagContent);
			if (name.IsEmpty) return false;

			return
				name.Equals("color".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("alpha".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("bold".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("b".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("italic".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("i".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("underline".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("u".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("strikethrough".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("s".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("size".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("font".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("sprite".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("link".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("mark".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("sub".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("sup".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("uppercase".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("smallcaps".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("br".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("pos".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("indent".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("margin".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("width".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("space".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("page".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("rotate".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("voffset".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("cspace".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("mspace".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("nobr".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("allcaps".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
				name.Equals("lowercase".AsSpan(), StringComparison.OrdinalIgnoreCase);
		}
	}
}