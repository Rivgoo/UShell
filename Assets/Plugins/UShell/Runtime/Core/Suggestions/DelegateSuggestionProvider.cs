using System;
using System.Collections.Generic;

namespace UShell.Runtime.Core.Suggestions
{
	/// <summary>
	/// A dynamic suggestion provider that invokes a custom delegate to resolve suggestions at runtime.
	/// </summary>
	public sealed class DelegateSuggestionProvider : ISuggestionProvider
	{
		private readonly Func<SuggestionContext, IEnumerable<string>> _func;

		/// <summary>
		/// Initializes a new instance wrapping the provided resolver function.
		/// </summary>
		public DelegateSuggestionProvider(Func<SuggestionContext, IEnumerable<string>> func)
		{
			_func = func ?? throw new ArgumentNullException(nameof(func));
		}

		/// <inheritdoc/>
		public IEnumerable<string> GetSuggestions(SuggestionContext context)
		{
			return _func(context);
		}
	}
}