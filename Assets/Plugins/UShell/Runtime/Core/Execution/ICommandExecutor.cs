namespace UShell.Runtime.Core.Execution
{
	public interface ICommandExecutor
	{
		ExecutionResult Execute(string input);
	}
}