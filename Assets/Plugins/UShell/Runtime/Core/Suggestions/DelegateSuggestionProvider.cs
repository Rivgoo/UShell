using System;
using System.Collections.Generic;

namespace UShell.Runtime.Core.Suggestions
{
	public sealed class DelegateSuggestionProvider : ISuggestionProvider
	{
		private readonly Func<SuggestionContext, IEnumerable<string>> _func;

		public DelegateSuggestionProvider(Func<SuggestionContext, IEnumerable<string>> func)
		{
			_func = func ?? throw new ArgumentNullException(nameof(func));
		}

		public IEnumerable<string> GetSuggestions(SuggestionContext context)
		{
			return _func(context);
		}
	}
}