using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Execution.Context;
using UShell.Runtime.Core.History;

namespace UShell.Runtime.Core
{
	public interface IShellCore
	{
		ICommandExecutor Executor { get; }
		ICommandRegistry Registry { get; }
		ICommandHistory History { get; }
		IInteractiveSession InteractiveSession { get; }
		ISessionState SessionState { get; }
	}
}