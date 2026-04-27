using System.Collections.Generic;
using UShell.Runtime.Core.Commands;

namespace UShell.Runtime.Core.Abstractions
{
	/// <summary>
	/// Represents a read-only facade for querying registered commands and autocomplete suggestions.
	/// </summary>
	public interface ICommandRegistry
	{
		/// <summary>
		/// Attempts to locate a command by its primary name or alias.
		/// </summary>
		/// <param name="name">The name or alias to search for.</param>
		/// <param name="signature">The resolved command signature, if found.</param>
		/// <returns><c>true</c> if the command exists; otherwise, <c>false</c>.</returns>
		bool TryGetCommand(string name, out CommandSignature signature);

		/// <summary>
		/// Generates a list of valid autocompletion suggestions based on the user's raw string input.
		/// </summary>
		/// <remarks>
		/// This evaluates partial command names, parameter names (like <c>-id</c>), and parameter values 
		/// using registered <see cref="UShell.Runtime.Core.Suggestions.ISuggestionProvider"/>s.
		/// </remarks>
		/// <param name="input">The current text in the input field.</param>
		/// <returns>A collection of scored and ranked suggestions.</returns>
		IReadOnlyList<CommandSuggestion> GetSuggestions(string input);

		/// <summary>
		/// Retrieves all commands currently registered in the active environment.
		/// </summary>
		/// <returns>A read-only collection of valid command signatures.</returns>
		IReadOnlyCollection<CommandSignature> GetAllCommands();

		/// <summary>
		/// Generates and caches a compact, single-line usage string for the specified signature.
		/// </summary>
		/// <param name="signature">The command signature to format.</param>
		/// <returns>A rich-text formatted usage string.</returns>
		string GetCompactSignature(CommandSignature signature);
	}
}