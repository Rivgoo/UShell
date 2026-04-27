#nullable enable
using System.Collections.Generic;

namespace UShell.Runtime.Core.History
{
	/// <summary>
	/// The default implementation of <see cref="ICommandHistory"/> maintaining a capped list of inputs.
	/// </summary>
	public sealed class CommandHistory : ICommandHistory
	{
		private readonly List<string> _entries;

		private int _navigationIndex = -1;
		private string? _uncommittedInput;

		/// <inheritdoc/>
		public int MaxCapacity { get; }

		/// <inheritdoc/>
		public IReadOnlyList<string> Entries => _entries;

		/// <summary>
		/// Initializes a new instance with the specified maximum capacity.
		/// </summary>
		/// <param name="maxCapacity">The number of historical entries to preserve.</param>
		public CommandHistory(int maxCapacity = 100)
		{
			MaxCapacity = maxCapacity > 0 ? maxCapacity : 1;
			_entries = new List<string>(MaxCapacity);
		}

		/// <inheritdoc/>
		public void Add(string command)
		{
			if (string.IsNullOrWhiteSpace(command)) return;

			if (_entries.Count > 0 && _entries[_entries.Count - 1] == command)
			{
				return;
			}

			_entries.Add(command);

			if (_entries.Count > MaxCapacity)
			{
				_entries.RemoveAt(0);
			}
		}

		/// <inheritdoc/>
		public void Clear()
		{
			_entries.Clear();
			ResetNavigation();
		}

		/// <inheritdoc/>
		public string? GetPrevious(string currentUncommittedInput)
		{
			if (_entries.Count == 0) return null;

			if (_navigationIndex == -1)
			{
				_uncommittedInput = currentUncommittedInput ?? string.Empty;
				_navigationIndex = _entries.Count - 1;
			}
			else if (_navigationIndex > 0)
			{
				_navigationIndex--;
			}

			return _entries[_navigationIndex];
		}

		/// <inheritdoc/>
		public string? GetNext()
		{
			if (_entries.Count == 0 || _navigationIndex == -1) return null;

			if (_navigationIndex < _entries.Count - 1)
			{
				_navigationIndex++;
				return _entries[_navigationIndex];
			}

			_navigationIndex = -1;
			string? restoredInput = _uncommittedInput;
			_uncommittedInput = null;

			return restoredInput;
		}

		/// <inheritdoc/>
		public void ResetNavigation()
		{
			_navigationIndex = -1;
			_uncommittedInput = null;
		}
	}
}