#nullable enable
using System.Collections.Generic;

namespace UShell.Runtime.Core.Execution.Context
{
	public interface ISessionState
	{
		bool TryGetValue(string name, out object? value);
		void SetValue(string name, object? value);
		bool Remove(string name);
		void Clear();
		IReadOnlyList<string> GetVariables();
	}
}