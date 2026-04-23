using System.Collections.Generic;

namespace UShell.Runtime.Core.Output
{
	public interface IConsolePrinter
	{
		void Print(LogEntry entry);
		void PrintTable(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows);
	}
}