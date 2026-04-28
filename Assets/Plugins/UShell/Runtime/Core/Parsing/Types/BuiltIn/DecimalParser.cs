using System.Globalization;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core.Parsing.Types.BuiltIn
{
	/// <summary>Parses a precise decimal number, ideal for financial/strict numeric operations.</summary>
	public sealed class DecimalParser : TypeParser<decimal>
	{
		/// <inheritdoc/>
		public override ExecutionResult<decimal> ParseTyped(string input)
		{
			if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result))
				return ExecutionResult<decimal>.Success(result);

			return ExecutionResult<decimal>.Failure(ShellError.Create(ShellErrorCode.Bind_TypeMismatch, -1, input, "decimal"));
		}
	}
}