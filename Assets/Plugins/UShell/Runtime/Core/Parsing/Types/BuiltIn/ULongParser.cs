using System.Globalization;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core.Parsing.Types.BuiltIn
{
	/// <summary>Parses a 64-bit unsigned integer.</summary>
	public sealed class ULongParser : TypeParser<ulong>
	{
		/// <inheritdoc/>
		public override ExecutionResult<ulong> ParseTyped(string input)
		{
			if (ulong.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong result))
				return ExecutionResult<ulong>.Success(result);

			return ExecutionResult<ulong>.Failure(ShellError.Create(ShellErrorCode.Bind_TypeMismatch, -1, input, "ulong"));
		}
	}
}