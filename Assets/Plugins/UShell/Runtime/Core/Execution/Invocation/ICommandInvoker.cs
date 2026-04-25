#nullable enable
namespace UShell.Runtime.Core.Execution.Invocation
{
	public interface ICommandInvoker
	{
		object? Invoke(object?[] args);
	}
}