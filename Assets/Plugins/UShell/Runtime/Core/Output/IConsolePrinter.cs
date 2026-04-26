using System;
using System.Collections.Generic;
using UShell.Runtime.Core.Output.Formatting;

namespace UShell.Runtime.Core.Output
{
	public interface IConsolePrinter
	{
		public event Action<LogEntry> OnLogAdded;

		void Print(LogEntry entry);
		void PrintTable(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows, TableStyle style = TableStyle.Standard);
	}
}