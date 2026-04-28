using System.Globalization;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core.Parsing.Types.BuiltIn
{
	/// <summary>Parses a 16-bit unsigned integer.</summary>
	public sealed class UShortParser : TypeParser<ushort>
	{
		/// <inheritdoc/>
		public override ExecutionResult<ushort> ParseTyped(string input)
		{
			if (ushort.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort result))
				return ExecutionResult<ushort>.Success(result);

			return ExecutionResult<ushort>.Failure(ShellError.Create(ShellErrorCode.Bind_TypeMismatch, -1, input, "ushort"));
		}
	}
}