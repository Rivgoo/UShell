using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core.Parsing.Types.BuiltIn
{
	/// <summary>Parses a single Unicode character.</summary>
	public sealed class CharParser : TypeParser<char>
	{
		/// <inheritdoc/>
		public override ExecutionResult<char> ParseTyped(string input)
		{
			if (char.TryParse(input, out char result))
				return ExecutionResult<char>.Success(result);

			return ExecutionResult<char>.Failure(ShellError.Create(ShellErrorCode.Bind_TypeMismatch, -1, input, "char"));
		}
	}
}