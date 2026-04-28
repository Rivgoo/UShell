using System.Globalization;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core.Parsing.Types.BuiltIn
{
	/// <summary>Parses a 16-bit signed integer.</summary>
	public sealed class ShortParser : TypeParser<short>
	{
		/// <inheritdoc/>
		public override ExecutionResult<short> ParseTyped(string input)
		{
			if (short.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out short result))
				return ExecutionResult<short>.Success(result);

			return ExecutionResult<short>.Failure(ShellError.Create(ShellErrorCode.Bind_TypeMismatch, -1, input, "short"));
		}
	}
}