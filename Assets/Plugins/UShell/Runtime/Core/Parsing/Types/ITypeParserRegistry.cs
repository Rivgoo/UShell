using System;

namespace UShell.Runtime.Core.Parsing.Types
{
	/// <summary>
	/// A centralized container that maps .NET types to their corresponding <see cref="ITypeParser"/>.
	/// </summary>
	public interface ITypeParserRegistry
	{
		/// <summary>
		/// Attempts to retrieve a registered parser for the specified type.
		/// </summary>
		/// <param name="type">The required target type.</param>
		/// <param name="parser">The resolved parser, if found.</param>
		/// <returns><c>true</c> if a parser exists for the type; otherwise, <c>false</c>.</returns>
		bool TryGetParser(Type type, out ITypeParser parser);

		/// <summary>
		/// Registers a new parser into the system.
		/// </summary>
		/// <param name="parser">The parser instance.</param>
		/// <param name="forceOverride">If <c>true</c>, overwrites any existing parser for the target type.</param>
		/// <exception cref="UShell.Runtime.Core.Exceptions.ParserRegistrationException">
		/// Thrown if a parser for the type already exists and <paramref name="forceOverride"/> is false.
		/// </exception>
		void Register(ITypeParser parser, bool forceOverride = false);

		/// <summary>
		/// Attempts to remove an existing parser for the specified type.
		/// </summary>
		/// <param name="targetType">The target type to unregister.</param>
		/// <returns><c>true</c> if the parser was found and removed; otherwise, <c>false</c>.</returns>
		bool TryRemoveParser(Type targetType);
	}
}