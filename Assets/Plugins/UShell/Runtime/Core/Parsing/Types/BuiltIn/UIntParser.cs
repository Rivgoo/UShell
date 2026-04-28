using System.Globalization;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core.Parsing.Types.BuiltIn
{
	/// <summary>Parses a 32-bit unsigned integer.</summary>
	public sealed class UIntParser : TypeParser<uint>
	{
		/// <inheritdoc/>
		public override ExecutionResult<uint> ParseTyped(string input)
		{
			if (uint.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint result))
				return ExecutionResult<uint>.Success(result);

			return ExecutionResult<uint>.Failure(ShellError.Create(ShellErrorCode.Bind_TypeMismatch, -1, input, "uint"));
		}
	}
}