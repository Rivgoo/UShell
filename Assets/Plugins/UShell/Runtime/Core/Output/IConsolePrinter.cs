using System;
using System.Collections.Generic;
using UShell.Runtime.Core.Output.Formatting;

namespace UShell.Runtime.Core.Output
{
	public interface IConsolePrinter
	{
		event Action<LogEntry> OnLogAdded;
		event Action<Guid, LogEntry> OnLogUpdated;

		void Print(LogEntry entry);
		void UpdatePrint(Guid id, LogEntry entry);
		void PrintTable(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows, TableStyle style = TableStyle.Standard);
	}
}