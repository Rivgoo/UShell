using System;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core.Parsing.Types
{
	/// <summary>
	/// Represents a non-generic contract for transforming raw string input into a specific .NET object type.
	/// </summary>
	/// <remarks>
	/// Prefer implementing <see cref="TypeParser{T}"/> instead of this interface directly.
	/// </remarks>
	public interface ITypeParser
	{
		/// <summary>
		/// The specific .NET type this parser is responsible for instantiating.
		/// </summary>
		Type TargetType { get; }

		/// <summary>
		/// Attempts to parse the raw text into the target object.
		/// </summary>
		/// <param name="input">The string segment provided by the user.</param>
		/// <returns>An execution result containing the parsed object, or a structured error if parsing failed.</returns>
		ExecutionResult<object> Parse(string input);
	}

	/// <summary>
	/// Represents a generic contract for transforming raw string input into a specific <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The strongly-typed object this parser creates.</typeparam>
	public interface ITypeParser<T> : ITypeParser
	{
		/// <summary>
		/// Attempts to parse the raw text into the target type <typeparamref name="T"/>.
		/// </summary>
		/// <param name="input">The string segment provided by the user.</param>
		/// <returns>An execution result containing the typed object, or a structured error.</returns>
		ExecutionResult<T> ParseTyped(string input);
	}
}