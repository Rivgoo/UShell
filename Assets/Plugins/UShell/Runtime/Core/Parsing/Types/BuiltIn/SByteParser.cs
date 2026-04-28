using System.Globalization;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core.Parsing.Types.BuiltIn
{
	/// <summary>Parses an 8-bit signed integer.</summary>
	public sealed class SByteParser : TypeParser<sbyte>
	{
		/// <inheritdoc/>
		public override ExecutionResult<sbyte> ParseTyped(string input)
		{
			if (sbyte.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out sbyte result))
				return ExecutionResult<sbyte>.Success(result);

			return ExecutionResult<sbyte>.Failure(ShellError.Create(ShellErrorCode.Bind_TypeMismatch, -1, input, "sbyte"));
		}
	}
}