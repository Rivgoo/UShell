using System.Globalization;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core.Parsing.Types.BuiltIn
{
	/// <summary>Parses an 8-bit unsigned integer.</summary>
	public sealed class ByteParser : TypeParser<byte>
	{
		/// <inheritdoc/>
		public override ExecutionResult<byte> ParseTyped(string input)
		{
			if (byte.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte result))
				return ExecutionResult<byte>.Success(result);

			return ExecutionResult<byte>.Failure(ShellError.Create(ShellErrorCode.Bind_TypeMismatch, -1, input, "byte"));
		}
	}
}