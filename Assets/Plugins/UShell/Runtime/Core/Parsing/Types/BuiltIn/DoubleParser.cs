using System.Globalization;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core.Parsing.Types.BuiltIn
{
	/// <summary>Parses a standard double-precision floating-point number.</summary>
	public sealed class DoubleParser : TypeParser<double>
	{
		/// <inheritdoc/>
		public override ExecutionResult<double> ParseTyped(string input)
		{
			if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
				return ExecutionResult<double>.Success(result);

			return ExecutionResult<double>.Failure(ShellError.Create(ShellErrorCode.Bind_TypeMismatch, -1, input, "double"));
		}
	}
}