#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace UShell.Runtime.Core.Execution.Context
{
	public sealed class SessionState : ISessionState
	{
		private readonly Dictionary<string, object?> _variables = new(StringComparer.OrdinalIgnoreCase);

		public bool TryGetValue(string name, out object? value)
		{
			return _variables.TryGetValue(name, out value);
		}

		public void SetValue(string name, object? value)
		{
			if (string.IsNullOrWhiteSpace(name)) return;
			_variables[name] = value;
		}

		public bool Remove(string name)
		{
			if (string.IsNullOrWhiteSpace(name)) return false;
			return _variables.Remove(name);
		}

		public void Clear()
		{
			_variables.Clear();
		}

		public IReadOnlyList<string> GetVariables()
		{
			return _variables.Keys.ToList().AsReadOnly();
		}
	}
}