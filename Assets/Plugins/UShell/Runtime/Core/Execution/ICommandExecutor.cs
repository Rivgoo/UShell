#nullable enable
namespace UShell.Runtime.Core.Execution
{
	public interface ICommandExecutor
	{
		ExecutionResult<object?> Execute(string input);
	}
}