using System;

namespace UShell.Runtime.Core.Parsing.Types
{
	public interface ITypeParserRegistry
	{
		bool TryGetParser(Type type, out ITypeParser parser);
		void Register(ITypeParser parser, bool forceOverride = false);
	}
}