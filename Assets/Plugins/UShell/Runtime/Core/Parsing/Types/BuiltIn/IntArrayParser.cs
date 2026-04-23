using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core.Parsing.Types.BuiltIn
{
	public sealed class IntArrayParser : TypeParser<int[]>
	{
		public override ExecutionResult<int[]> ParseTyped(string input) =>
			ExecutionResult<int[]>.Failure("Use array syntax [1,2,3] for int arrays.");
	}
}