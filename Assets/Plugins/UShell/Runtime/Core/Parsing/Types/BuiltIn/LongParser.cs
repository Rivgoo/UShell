using System.Globalization;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core.Parsing.Types.BuiltIn
{
	/// <summary>Parses a 64-bit signed integer.</summary>
	public sealed class LongParser : TypeParser<long>
	{
		/// <inheritdoc/>
		public override ExecutionResult<long> ParseTyped(string input)
		{
			if (long.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out long result))
				return ExecutionResult<long>.Success(result);

			return ExecutionResult<long>.Failure(ShellError.Create(ShellErrorCode.Bind_TypeMismatch, -1, input, "long"));
		}
	}
}