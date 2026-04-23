using System;
using System.Collections.Generic;

namespace UShell.Runtime.Core.Parsing.Types
{
	internal sealed class TypeParserRegistry : ITypeParserRegistry
	{
		private readonly Dictionary<Type, ITypeParser> _parsers = new();

		public bool TryGetParser(Type type, out ITypeParser parser)
		{
			return _parsers.TryGetValue(type, out parser!);
		}

		public void Register(ITypeParser parser)
		{
			if (parser == null) throw new ArgumentNullException(nameof(parser));

			_parsers[parser.TargetType] = parser;
		}
	}
}