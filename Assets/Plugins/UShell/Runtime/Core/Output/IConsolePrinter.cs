using System;
using System.Collections.Generic;
using UShell.Runtime.Core.Output.Formatting;

namespace UShell.Runtime.Core.Output
{
	/// <summary>
	/// The abstraction layer separating the shell's business logic from the visual UI output.
	/// </summary>
	/// <remarks>
	/// Any command, profile, or internal system that needs to display text must route it through this interface.
	/// This ensures UShell can be attached to any UI system (Unity UI, UI Toolkit, text file, etc.).
	/// </remarks>
	public interface IConsolePrinter
	{
		/// <summary>
		/// Fired when a new log entry is dispatched to the console.
		/// </summary>
		event Action<LogEntry> OnLogAdded;

		/// <summary>
		/// Fired when an existing log entry (tracked by a unique ID) should be modified in-place 
		/// (e.g., updating a progress bar).
		/// </summary>
		event Action<Guid, LogEntry> OnLogUpdated;

		/// <summary>
		/// Dispatches a new log entry to be rendered by the active UI.
		/// </summary>
		/// <param name="entry">The constructed log payload.</param>
		void Print(LogEntry entry);

		/// <summary>
		/// Modifies an already printed log entry. Useful for dynamic statuses like progress bars.
		/// </summary>
		/// <param name="id">The unique identifier of the entry to modify.</param>
		/// <param name="entry">The updated log payload.</param>
		void UpdatePrint(Guid id, LogEntry entry);

		/// <summary>
		/// Builds and dispatches an ASCII-formatted table to the output.
		/// </summary>
		/// <param name="headers">A list of table column names.</param>
		/// <param name="rows">A matrix of strings representing the table cells.</param>
		/// <param name="style">The border style of the table.</param>
		void PrintTable(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows, TableStyle style = TableStyle.Standard);
	}
}