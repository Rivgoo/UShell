using System;
using System.Collections.Generic;

namespace UShell.Runtime.Core.Suggestions
{
	/// <summary>
	/// A suggestion provider that returns a fixed, pre-defined list of strings.
	/// </summary>
	public sealed class StaticSuggestionProvider : ISuggestionProvider
	{
		private readonly IReadOnlyList<string> _suggestions;

		/// <summary>
		/// Initializes a new instance holding the provided static collection.
		/// </summary>
		/// <param name="suggestions">The fixed suggestions to offer.</param>
		public StaticSuggestionProvider(IEnumerable<string> suggestions)
		{
			if (suggestions == null) throw new ArgumentNullException(nameof(suggestions));
			_suggestions = new List<string>(suggestions);
		}

		/// <inheritdoc/>
		public IEnumerable<string> GetSuggestions(SuggestionContext context)
		{
			return _suggestions;
		}
	}
}