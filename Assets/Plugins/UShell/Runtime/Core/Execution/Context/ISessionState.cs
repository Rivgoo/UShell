#nullable enable
using System.Collections.Generic;

namespace UShell.Runtime.Core.Execution.Context
{
	/// <summary>
	/// A runtime key-value store maintaining session-scoped variables and macros.
	/// </summary>
	/// <remarks>
	/// Variables stored here can be accessed directly from the shell input using the <c>$</c> prefix 
	/// (e.g., <c>$myVar</c>). They are cleared if the session ends or via the <c>macro.clear</c> command.
	/// </remarks>
	public interface ISessionState
	{
		/// <summary>
		/// Attempts to retrieve a stored value by its name.
		/// </summary>
		/// <param name="name">The case-insensitive name of the variable (without the <c>$</c> prefix).</param>
		/// <param name="value">The retrieved value, or null if not found.</param>
		/// <returns><c>true</c> if the variable exists; otherwise, <c>false</c>.</returns>
		bool TryGetValue(string name, out object? value);

		/// <summary>
		/// Stores or overwrites a value in the session state.
		/// </summary>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">The value to store.</param>
		void SetValue(string name, object? value);

		/// <summary>
		/// Removes a specific variable from the session state.
		/// </summary>
		/// <param name="name">The name of the variable to remove.</param>
		/// <returns><c>true</c> if the variable was found and removed; otherwise, <c>false</c>.</returns>
		bool Remove(string name);

		/// <summary>
		/// Wipes all currently stored variables and macros from the state.
		/// </summary>
		void Clear();

		/// <summary>
		/// Retrieves a read-only list of all currently registered variable names.
		/// </summary>
		/// <returns>A collection of registered keys.</returns>
		IReadOnlyList<string> GetVariables();
	}
}