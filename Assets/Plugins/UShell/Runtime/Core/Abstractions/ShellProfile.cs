#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using UShell.Runtime.Core.Commands.Fluent;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Core.Output.Formatting;

namespace UShell.Runtime.Core.Abstractions
{
	/// <summary>
	/// A base class for creating shell profiles. Provides convenient protected methods for 
	/// formatted console output, warnings, errors, and tables.
	/// </summary>
	/// <remarks>
	/// Prefer inheriting from <see cref="ShellProfile"/> rather than implementing 
	/// <see cref="IShellProfile"/> directly to easily access the printer utilities.
	/// </remarks>
	public abstract class ShellProfile : IShellProfile
	{
		/// <summary>
		/// The printer instance used for dispatching logs to the visual console interface.
		/// </summary>
		protected readonly IConsolePrinter Printer;

		/// <summary>
		/// Initializes a new instance of the profile with the specified printer.
		/// </summary>
		/// <param name="printer">The output printer instance.</param>
		/// <exception cref="ArgumentNullException">Thrown if the printer is null.</exception>
		protected ShellProfile(IConsolePrinter printer)
		{
			Printer = printer ?? throw new ArgumentNullException(nameof(printer));
		}

		/// <inheritdoc/>
		public void RegisterCommands(ICommandBuilder builder)
		{
			if (builder == null) throw new ArgumentNullException(nameof(builder));
			Configure(builder);
		}

		/// <summary>
		/// Override this method to configure and define the commands for this profile.
		/// </summary>
		/// <param name="builder">The builder used to declare commands.</param>
		protected abstract void Configure(ICommandBuilder builder);

		/// <summary>
		/// Prints a standard text message to the console.
		/// </summary>
		/// <param name="message">The message to display.</param>
		protected void Print(string message)
		{
			Printer.Print(new LogEntry(message, LogType.Standard));
		}

		/// <summary>
		/// Prints a success message (usually colored green) to the console.
		/// </summary>
		/// <param name="message">The success message.</param>
		protected void PrintSuccess(string message)
		{
			Printer.Print(new LogEntry(message, LogType.Success));
		}

		/// <summary>
		/// Prints a warning message (usually colored yellow/amber) to the console.
		/// </summary>
		/// <param name="message">The warning message.</param>
		protected void PrintWarning(string message)
		{
			Printer.Print(new LogEntry(message, LogType.Warning));
		}

		/// <summary>
		/// Prints an error message (usually colored red) to the console.
		/// </summary>
		/// <param name="message">The error message.</param>
		protected void PrintError(string message)
		{
			Printer.Print(new LogEntry(message, LogType.Error));
		}

		/// <summary>
		/// Formats and prints an aligned ASCII table to the console.
		/// </summary>
		/// <param name="headers">A list of column names.</param>
		/// <param name="rows">A list of rows, where each row is a list of column values.</param>
		/// <param name="style">The visual style of the table separators.</param>
		protected void PrintTable(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows, TableStyle style = TableStyle.Standard)
		{
			Printer.PrintTable(headers, rows, style);
		}

		/// <summary>
		/// Formats and prints a numbered list of items with a section header.
		/// </summary>
		/// <remarks>
		/// If the list size exceeds the <paramref name="limit"/>, the output will truncate and display 
		/// a hint on how to view the rest.
		/// </remarks>
		/// <param name="title">The title displayed above the list.</param>
		/// <param name="items">The collection of items to print.</param>
		/// <param name="limit">The maximum number of items to print before truncating.</param>
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