using System.Globalization;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core.Parsing.Types.BuiltIn
{
	public sealed class IntParser : TypeParser<int>
	{
		public override ExecutionResult<int> ParseTyped(string input)
		{
			if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
			{
				return ExecutionResult<int>.Success(result);
			}

			return ExecutionResult<int>.Failure($"Cannot parse '{input}' as an integer.");
		}
	}
}