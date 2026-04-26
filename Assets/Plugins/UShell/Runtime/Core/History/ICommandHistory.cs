#nullable enable
using System.Collections.Generic;

namespace UShell.Runtime.Core.History
{
	public interface ICommandHistory
	{
		IReadOnlyList<string> Entries { get; }
		int MaxCapacity { get; }

		void Add(string command);
		void Clear();

		string? GetPrevious(string currentUncommittedInput);
		string? GetNext();
		void ResetNavigation();
	}
}