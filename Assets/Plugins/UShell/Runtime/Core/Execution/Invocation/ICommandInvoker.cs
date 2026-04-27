#nullable enable
using UShell.Runtime.Core.Execution.Context;

namespace UShell.Runtime.Core.Execution.Invocation
{
	public interface ICommandInvoker
	{
		object? Invoke(ICommandContext? context, object?[] args);
	}
}