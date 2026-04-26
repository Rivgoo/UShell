#nullable enable
using System.Collections.Generic;

namespace UShell.Runtime.Core.History
{
	public sealed class CommandHistory : ICommandHistory
	{
		private readonly List<string> _entries;

		private int _navigationIndex = -1;
		private string? _uncommittedInput;

		public int MaxCapacity { get; }
		public IReadOnlyList<string> Entries => _entries;

		public CommandHistory(int maxCapacity = 100)
		{
			MaxCapacity = maxCapacity > 0 ? maxCapacity : 1;
			_entries = new List<string>(MaxCapacity);
		}

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

		public void Clear()
		{
			_entries.Clear();
			ResetNavigation();
		}

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

		public void ResetNavigation()
		{
			_navigationIndex = -1;
			_uncommittedInput = null;
		}
	}
}