using System.Globalization;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core.Parsing.Types.BuiltIn
{
	public sealed class FloatParser : TypeParser<float>
	{
		public override ExecutionResult<float> ParseTyped(string input)
		{
			if (float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
			{
				return ExecutionResult<float>.Success(result);
			}

			return ExecutionResult<float>.Failure(
				ShellError.Create(ShellErrorCode.Bind_TypeMismatch, -1, input, "float"));
		}
	}
}