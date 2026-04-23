using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core.Parsing.Types.BuiltIn
{
	public sealed class StringParser : TypeParser<string>
	{
		public override ExecutionResult<string> ParseTyped(string input)
		{
			return ExecutionResult<string>.Success(input);
		}
	}
}