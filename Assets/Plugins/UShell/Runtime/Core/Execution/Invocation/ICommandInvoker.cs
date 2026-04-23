#nullable enable
namespace UShell.Runtime.Core.Execution.Invocation
{
	public interface ICommandInvoker
	{
		void Invoke(object?[] args);
	}
}