#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using UShell.Runtime.Core.Commands.Fluent;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Core.Output.Formatting;

namespace UShell.Runtime.Core.Abstractions
{
	public abstract class ShellProfile : IShellProfile
	{
		protected readonly IConsolePrinter Printer;

		protected ShellProfile(IConsolePrinter printer)
		{
			Printer = printer ?? throw new ArgumentNullException(nameof(printer));
		}

		public void RegisterCommands(ICommandBuilder builder)
		{
			if (builder == null) throw new ArgumentNullException(nameof(builder));
			Configure(builder);
		}

		protected abstract void Configure(ICommandBuilder builder);

		protected void Print(string message)
		{
			Printer.Print(new LogEntry(message, LogType.Standard));
		}

		protected void PrintSuccess(string message)
		{
			Printer.Print(new LogEntry(message, LogType.Success));
		}

		protected void PrintWarning(string message)
		{
			Printer.Print(new LogEntry(message, LogType.Warning));
		}

		protected void PrintError(string message)
		{
			Printer.Print(new LogEntry(message, LogType.Error));
		}

		protected void PrintTable(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows, TableStyle style = TableStyle.Standard)
		{
			Printer.PrintTable(headers, rows, style);
		}

		protected void PrintList(string title, IReadOnlyList<string> items, int limit)
		{
			if (items.Count == 0)
			{
				PrintWarning($"No results for: {title}.");
				return;
			}

			var sb = new StringBuilder();
			sb.AppendLine(ProfileFormatter.FormatSectionHeader(title));

			int shown = Math.Min(items.Count, limit);
			for (int i = 0; i < shown; i++)
			{
				sb.Append(RichText.Color($"  {i + 1,3}  ", ShellPalette.TextDim));
				sb.AppendLine(RichText.Color(items[i], ShellPalette.TextPrimary));
			}

			if (items.Count > limit)
			{
				sb.Append(RichText.Color($"  … and {items.Count - limit} more (use -limit {items.Count} to see all).", ShellPalette.TextHint));
			}

			Print(sb.ToString().TrimEnd());
		}
	}
}