#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace UShell.Runtime.Core.Execution.Context
{
	/// <summary>
	/// The default implementation of <see cref="ISessionState"/> using a case-insensitive dictionary.
	/// </summary>
	public sealed class SessionState : ISessionState
	{
		private readonly Dictionary<string, object?> _variables = new(StringComparer.OrdinalIgnoreCase);

		/// <inheritdoc/>
		public bool TryGetValue(string name, out object? value)
		{
			return _variables.TryGetValue(name, out value);
		}

		/// <inheritdoc/>
		public void SetValue(string name, object? value)
		{
			if (string.IsNullOrWhiteSpace(name)) return;
			_variables[name] = value;
		}

		/// <inheritdoc/>
		public bool Remove(string name)
		{
			if (string.IsNullOrWhiteSpace(name)) return false;
			return _variables.Remove(name);
		}

		/// <inheritdoc/>
		public void Clear()
		{
			_variables.Clear();
		}

		/// <inheritdoc/>
		public IReadOnlyList<string> GetVariables()
		{
			return _variables.Keys.ToList().AsReadOnly();
		}
	}
}