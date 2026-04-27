using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Execution.Context;
using UShell.Runtime.Core.History;

namespace UShell.Runtime.Core
{
	/// <summary>
	/// The top-level facade containing all interconnected subsystems of a running UShell instance.
	/// </summary>
	/// <remarks>
	/// Produced by the <see cref="UShell.Runtime.Core.Bootstrapping.ShellBuilder"/>. 
	/// Connect this core to a UI system or input provider to bridge user actions to the executor.
	/// </remarks>
	public interface IShellCore
	{
		/// <summary>
		/// The system responsible for taking raw strings, parsing them, and invoking commands.
		/// </summary>
		ICommandExecutor Executor { get; }

		/// <summary>
		/// The lookup table of all active commands and autocomplete logic.
		/// </summary>
		ICommandRegistry Registry { get; }

		/// <summary>
		/// The rolling log of previously executed commands.
		/// </summary>
		ICommandHistory History { get; }

		/// <summary>
		/// The manager responsible for locking the shell and requesting user input for asynchronous tasks.
		/// </summary>
		IInteractiveSession InteractiveSession { get; }

		/// <summary>
		/// The memory container for stored variables and macros.
		/// </summary>
		ISessionState SessionState { get; }
	}
}