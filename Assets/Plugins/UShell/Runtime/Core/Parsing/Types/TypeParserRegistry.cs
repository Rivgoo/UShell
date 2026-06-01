using System;
using System.Collections.Generic;
using UShell.Runtime.Core.Exceptions;

namespace UShell.Runtime.Core.Parsing.Types
{
	internal sealed class TypeParserRegistry : ITypeParserRegistry
	{
		private readonly Dictionary<Type, ITypeParser> _parsers = new();

		public bool TryGetParser(Type type, out ITypeParser parser)
		{
			return _parsers.TryGetValue(type, out parser!);
		}

		public void Register(ITypeParser parser, bool forceOverride = false)
		{
			if (parser == null) throw new ArgumentNullException(nameof(parser));

			if (!forceOverride && _parsers.ContainsKey(parser.TargetType))
			{
				throw new ParserRegistrationException(
					$"A parser for type '{parser.TargetType.Name}' is already registered.");
			}

			_parsers[parser.TargetType] = parser;
		}

		public bool TryRemoveParser(Type targetType)
		{
			if (targetType == null) throw new ArgumentNullException(nameof(targetType));

			return _parsers.Remove(targetType);
		}
	}
}