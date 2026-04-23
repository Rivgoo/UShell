using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core
{
	public interface IShellCore
	{
		ICommandExecutor Executor { get; }
		ICommandRegistry Registry { get; }
	}
}