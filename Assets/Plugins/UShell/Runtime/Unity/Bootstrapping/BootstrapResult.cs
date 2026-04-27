using System;
using UShell.Runtime.Core;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Output;

namespace UShell.Runtime.Unity.Bootstrapping
{
	/// <summary>
	/// Represents the final output of the shell bootstrapping phase, containing the initialized 
	/// subsystems required to connect the shell to the Unity environment.
	/// </summary>
	public sealed class BootstrapResult
	{
		/// <summary>
		/// The initialized and wired core engine containing the command registry, history, and executor.
		/// </summary>
		public IShellCore Core { get; }

		/// <summary>
		/// The printer instance responsible for dispatching shell logs to the Unity console and custom UI.
		/// </summary>
		public IConsolePrinter Printer { get; }

		/// <summary>
		/// The deferred registry proxy used to resolve circular dependencies during command registration.
		/// </summary>
		public ICommandRegistry CommandRegistry { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="BootstrapResult"/> class.
		/// </summary>
		public BootstrapResult(
			IShellCore core,
			IConsolePrinter printer,
			ICommandRegistry commandRegistry)
		{
			Core = core ?? throw new ArgumentNullException(nameof(core));
			Printer = printer ?? throw new ArgumentNullException(nameof(printer));
			CommandRegistry = commandRegistry ?? throw new ArgumentNullException(nameof(commandRegistry));
		}
	}
}