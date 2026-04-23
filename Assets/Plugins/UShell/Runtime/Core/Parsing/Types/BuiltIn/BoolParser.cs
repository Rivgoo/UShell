using System;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core.Parsing.Types.BuiltIn
{
	public sealed class BoolParser : TypeParser<bool>
	{
		public override ExecutionResult<bool> ParseTyped(string input)
		{
			if (bool.TryParse(input, out bool result))
			{
				return ExecutionResult<bool>.Success(result);
			}

			if (input == "1") return ExecutionResult<bool>.Success(true);
			if (input == "0") return ExecutionResult<bool>.Success(false);

			return ExecutionResult<bool>.Failure($"Cannot parse '{input}' as a boolean.");
		}
	}
}