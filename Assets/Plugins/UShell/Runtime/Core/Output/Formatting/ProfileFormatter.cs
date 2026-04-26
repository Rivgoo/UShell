#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using UShell.Runtime.Core.Commands;

namespace UShell.Runtime.Core.Output.Formatting
{
	public static class ProfileFormatter
	{
		public static string FormatSectionHeader(string title)
		{
			string left = RichText.Color("  ── ", ShellPalette.AccentMuted);
			string name = RichText.Bold(RichText.Color(title, ShellPalette.AccentBright));
			string right = RichText.Color(" " + new string('─', Math.Max(0, 44 - title.Length)), ShellPalette.AccentDim);
			return left + name + right;
		}

		public static string Indent(int level)
		{
			return new string(' ', level * 2);
		}

		public static void AppendKeyValue(StringBuilder sb, string key, string value)
		{
			string k = RichText.Color(key.PadRight(14), ShellPalette.TextMuted);
			sb.Append(Indent(1)).Append(k).Append("  ").AppendLine(value);
		}

		public static void AppendKeyValueWide(StringBuilder sb, string key, string value, string valueColor)
		{
			string k = RichText.Color(key.PadRight(20), ShellPalette.TextMuted);
			sb.Append(Indent(1)).Append(k).Append("  ").AppendLine(RichText.Color(value, valueColor));
		}

		public static string FormatStat(string text, string color)
		{
			return RichText.Color(text, color);
		}

		public static string FormatBool(bool value)
		{
			return value
				? RichText.Color("yes", ShellPalette.Success)
				: RichText.Color("no", ShellPalette.TextMuted);
		}

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
					? RichText.Color(" [", ShellPalette.TextDim) + nameStr + typePart + RichText.Color("]", ShellPalette.TextDim)
					: RichText.Color(" <", ShellPalette.TextDim) + nameStr + typePart + RichText.Color(">", ShellPalette.TextDim);

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

		public static string FormatEnvironmentTags(EnvironmentTag tags)
		{
			if (tags == EnvironmentTag.Any) return RichText.Color("Any", ShellPalette.TextMuted);

			var parts = new List<string>(3);
			if ((tags & EnvironmentTag.Editor) != 0) parts.Add(RichText.Color("[Editor]", ShellPalette.BadgeEditor));
			if ((tags & EnvironmentTag.Development) != 0) parts.Add(RichText.Color("[Development]", ShellPalette.BadgeDev));
			if ((tags & EnvironmentTag.Release) != 0) parts.Add(RichText.Color("[Release]", ShellPalette.BadgeRelease));

			return string.Join("  ", parts);
		}

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