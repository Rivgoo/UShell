using System;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core.Parsing.Types
{
	/// <summary>
	/// The recommended base class for creating custom type parsers.
	/// </summary>
	/// <remarks>
	/// Custom type parsers allow commands to accept complex game objects (e.g., <c>ItemData</c>, <c>EnemyConfig</c>) 
	/// directly as parameters. The shell will automatically route the user's string input through your parser.
	/// </remarks>
	/// <typeparam name="T">The data type this parser generates.</typeparam>
	/// <example>
	/// <code>
	/// public class PlayerIdParser : TypeParser&lt;PlayerId&gt;
	/// {
	///     public override ExecutionResult&lt;PlayerId&gt; ParseTyped(string input)
	///     {
	///         if (int.TryParse(input, out int id))
	///             return ExecutionResult&lt;PlayerId&gt;.Success(new PlayerId(id));
	/// 
	///         return ExecutionResult&lt;PlayerId&gt;.Failure(
	///             ShellError.Create(ShellErrorCode.Bind_CustomError, -1, "Invalid Player ID format."));
	///     }
	/// }
	/// </code>
	/// </example>
	public abstract class TypeParser<T> : ITypeParser<T>
	{
		/// <inheritdoc/>
		public Type TargetType => typeof(T);

		/// <inheritdoc/>
		public ExecutionResult<object> Parse(string input)
		{
			ExecutionResult<T> result = ParseTyped(input);

			if (result.IsSuccess)
			{
				return ExecutionResult<object>.Success(result.Value!);
			}

			return ExecutionResult<object>.Failure(result.Error!.Value);
		}

		/// <summary>
		/// Implement this method to define the conversion logic from a string to <typeparamref name="T"/>.
		/// </summary>
		/// <param name="input">The raw text passed by the user.</param>
		/// <returns>A successful result with the object, or a failure result explaining why it couldn't be parsed.</returns>
		public abstract ExecutionResult<T> ParseTyped(string input);
	}
}