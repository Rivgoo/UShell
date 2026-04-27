using System.Collections.Generic;

namespace UShell.Runtime.Core.Suggestions
{
	public interface ISuggestionProvider
	{
		IEnumerable<string> GetSuggestions(SuggestionContext context);
	}
}