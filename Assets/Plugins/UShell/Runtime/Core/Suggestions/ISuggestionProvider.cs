using System.Collections.Generic;

namespace UShell.Runtime.Core.Suggestions
{
	/// <summary>
	/// Defines a contract for supplying dynamic autocomplete suggestions for a specific command parameter.
	/// </summary>
	/// <remarks>
	/// Used heavily in the fluent API via <see cref="UShell.Runtime.Core.Commands.Fluent.ICommandConfigurator.WithSuggestions(ISuggestionProvider)"/>.
	/// </remarks>
	public interface ISuggestionProvider
	{
		/// <summary>
		/// Generates a list of valid string suggestions based on the user's current input context.
		/// </summary>
		/// <param name="context">Contains the partial string the user has typed so far.</param>
		/// <returns>A collection of possible autocompletion strings.</returns>
		IEnumerable<string> GetSuggestions(SuggestionContext context);
	}
}