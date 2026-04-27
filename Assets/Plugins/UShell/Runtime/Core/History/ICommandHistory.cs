#nullable enable
using System.Collections.Generic;

namespace UShell.Runtime.Core.History
{
	/// <summary>
	/// Maintains a chronological record of executed user inputs, allowing traversal via Up/Down arrow keys.
	/// </summary>
	public interface ICommandHistory
	{
		/// <summary>
		/// A read-only view of the currently stored history entries (oldest first, newest last).
		/// </summary>
		IReadOnlyList<string> Entries { get; }

		/// <summary>
		/// The maximum number of entries the history will retain before discarding the oldest.
		/// </summary>
		int MaxCapacity { get; }

		/// <summary>
		/// Commits a new user input to the history log.
		/// </summary>
		/// <param name="command">The raw string executed by the user.</param>
		void Add(string command);

		/// <summary>
		/// Wipes all entries from the history log.
		/// </summary>
		void Clear();

		/// <summary>
		/// Navigates backwards in time to fetch an older command.
		/// </summary>
		/// <param name="currentUncommittedInput">The text currently in the input field before navigating. This is saved to restore later.</param>
		/// <returns>The older command string, or null if at the beginning of history.</returns>
		string? GetPrevious(string currentUncommittedInput);

		/// <summary>
		/// Navigates forwards in time to fetch a newer command, or restores the uncommitted input.
		/// </summary>
		/// <returns>The newer command string, the uncommitted string, or null.</returns>
		string? GetNext();

		/// <summary>
		/// Resets the internal navigation index to the bottom of the stack without erasing the history.
		/// </summary>
		void ResetNavigation();
	}
}