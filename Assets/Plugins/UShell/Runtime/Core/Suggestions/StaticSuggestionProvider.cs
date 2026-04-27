using System;
using System.Collections.Generic;

namespace UShell.Runtime.Core.Suggestions
{
	public sealed class StaticSuggestionProvider : ISuggestionProvider
	{
		private readonly IReadOnlyList<string> _suggestions;

		public StaticSuggestionProvider(IEnumerable<string> suggestions)
		{
			if (suggestions == null) throw new ArgumentNullException(nameof(suggestions));
			_suggestions = new List<string>(suggestions);
		}

		public IEnumerable<string> GetSuggestions(SuggestionContext context)
		{
			return _suggestions;
		}
	}
}