#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using UShell.Runtime.Core.Commands;

namespace UShell.Runtime.Core.Output.Formatting
{
	/// <summary>
	/// A utility class providing standard formatting methods for rich-text console output.
	/// </summary>
	/// <remarks>
	/// Use these methods inside your <see cref="UShell.Runtime.Core.Abstractions.ShellProfile"/> 
	/// to ensure your custom commands match the visual style of the built-in shell commands.
	/// </remarks>
	public static class ProfileFormatter
	{
		/// <summary>
		/// Formats a string as a standardized section header (e.g., <c>── TITLE ───</c>).
		/// </summary>
		public static string FormatSectionHeader(string title)
		{
			string left = RichText.Color("  ── ", ShellPalette.AccentMuted);
			string name = RichText.Bold(RichText.Color(title, ShellPalette.AccentBright));
			string right = RichText.Color(" " + new string('─', Math.Max(0, 44 - title.Length)), ShellPalette.AccentDim);
			return left + name + right;
		}

		/// <summary>
		/// Returns a string of spaces for indentation.
		/// </summary>
		/// <param name="level">The depth level. Each level equals two spaces.</param>
		public static string Indent(int level)
		{
			return new string(' ', level * 2);
		}

		/// <summary>
		/// Appends a standardized key-value pair to a StringBuilder (key is muted, value is primary text).
		/// </summary>
		public static void AppendKeyValue(StringBuilder sb, string key, string value)
		{
			string k = RichText.Color(key.PadRight(14), ShellPalette.TextMuted);
			sb.Append(Indent(1)).Append(k).Append("  ").AppendLine(value);
		}

		/// <summary>
		/// Appends a wider key-value pair with a custom color for the value text.
		/// </summary>
		public static void AppendKeyValueWide(StringBuilder sb, string key, string value, string valueColor)
		{
			string k = RichText.Color(key.PadRight(20), ShellPalette.TextMuted);
			sb.Append(Indent(1)).Append(k).Append("  ").AppendLine(RichText.Color(value, valueColor));
		}

		/// <summary>
		/// Wraps text in the specified rich-text color.
		/// </summary>
		public static string FormatStat(string text, string color)
		{
			return RichText.Color(text, color);
		}

		/// <summary>
		/// Formats a boolean value as a colored "yes" (green) or "no" (muted grey).
		/// </summary>
		public static string FormatBool(bool value)
		{
			return value
				? RichText.Color("yes", ShellPalette.Success)
				: RichText.Color("no", ShellPalette.TextMuted);
		}

		/// <summary>
		/// Word-wraps text to ensure no line exceeds the maximum length.
		/// </summary>
		public static string WrapText(string text, int maxLineLength)
		{
			if (string.IsNullOrEmpty(text)) return string.Empty;

			var words = text.Split(' ');
			var sb = new StringBuilder();
			int currentLineLength = 0;

			for (int i = 0; i < words.Length; i++)
			{
				if (currentLineLength + words[i].Length > maxLineLength && currentLineLength > 0)
				{
					sb.AppendLine();
					currentLineLength = 0;
				}
				sb.Append(words[i]);
				currentLineLength += words[i].Length;

				if (i < words.Length - 1)
				{
					sb.Append(' ');
					currentLineLength++;
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// Formats a full multiline usage signature showing all parameters and their types.
		/// </summary>
		public static string FormatUsageSignature(CommandSignature sig, int maxWidth)
		{
			var sb = new StringBuilder();
			string cmdName = RichText.Color(sig.Name, ShellPalette.SyntaxCommand);
			sb.Append(cmdName);

			int currentLineLen = sig.Name.Length;

			foreach (CommandParameter p in sig.Parameters)
			{
				string typeStr = FriendlyTypeName(p.ParameterType);
				string typePart = RichText.Color($":{typeStr}", ShellPalette.SyntaxType);
				string nameStr = RichText.Color($"-{p.Name}", ShellPalette.SyntaxParam);

				string formattedParam = p.IsOptional
					? RichText.Color(" [", ShellPalette.TextTertiary) + nameStr + typePart + RichText.Color("]", ShellPalette.TextTertiary)
					: RichText.Color(" <", ShellPalette.TextTertiary) + nameStr + typePart + RichText.Color(">", ShellPalette.TextTertiary);

				int paramVisLen = RichTextStripper.Strip(formattedParam).Length;

				if (currentLineLen + paramVisLen > maxWidth && currentLineLen > 0)
				{
					sb.Append('\n');
					sb.Append("  ");
					currentLineLen = 2;
				}

				sb.Append(formattedParam);
				currentLineLen += paramVisLen;
			}

			return sb.ToString();
		}

		/// <summary>
		/// Formats a compact, single-line version of a command signature.
		/// </summary>
		public static string FormatCompactSignature(CommandSignature sig)
		{
			var sb = new StringBuilder();
			sb.Append(RichText.Color(sig.Name, ShellPalette.SyntaxCommand));

			foreach (CommandParameter p in sig.Parameters)
			{
				string typeStr = FriendlyTypeName(p.ParameterType);
				string typePart = RichText.Color($":{typeStr}", ShellPalette.SyntaxType);
				string nameStr = RichText.Color($"-{p.Name}", ShellPalette.SyntaxParam);

				string formattedParam = p.IsOptional
					? RichText.Color("[", ShellPalette.TextTertiary) + nameStr + typePart + RichText.Color("]", ShellPalette.TextTertiary)
					: RichText.Color("<", ShellPalette.TextTertiary) + nameStr + typePart + RichText.Color(">", ShellPalette.TextTertiary);

				sb.Append(" ").Append(formattedParam);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Returns colored text badges representing the allowed environments for a command.
		/// </summary>
		public static string FormatEnvironmentTags(EnvironmentTag tags)
		{
			if (tags == EnvironmentTag.Any) return RichText.Color("Any", ShellPalette.TextMuted);

			var parts = new List<string>(3);
			if ((tags & EnvironmentTag.Editor) != 0) parts.Add(RichText.Color("[Editor]", ShellPalette.BadgeEditor));
			if ((tags & EnvironmentTag.Development) != 0) parts.Add(RichText.Color("[Development]", ShellPalette.BadgeDev));
			if ((tags & EnvironmentTag.Release) != 0) parts.Add(RichText.Color("[Release]", ShellPalette.BadgeRelease));

			return string.Join("  ", parts);
		}

		/// <summary>
		/// Simplifies raw .NET type names (e.g., <c>System.Int32</c> becomes <c>int</c>) for UI display.
		/// </summary>
		public static string FriendlyTypeName(Type t)
		{
			if (t == typeof(int)) return "int";
			if (t == typeof(float)) return "float";
			if (t == typeof(bool)) return "bool";
			if (t == typeof(string)) return "string";
			if (t == typeof(double)) return "double";
			if (t == typeof(long)) return "long";
			if (t == typeof(int[])) return "int[]";
			if (t == typeof(float[])) return "float[]";
			if (t == typeof(string[])) return "string[]";
			return t.Name;
		}
	}
}