using System;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Execution.Context;
using UShell.Runtime.Core.History;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Core.Registry;

namespace UShell.Runtime.Unity.Bootstrapping
{
	/// <summary>
	/// Provides access to early-bound core dependencies during the shell configuration phase.
	/// </summary>
	/// <remarks>
	/// Profiles often require access to the History, Printer, or Session State. This context 
	/// serves as a dependency injection container passed to factory methods.
	/// </remarks>
	public sealed class ShellBootstrapContext
	{
		/// <summary>The active console printer.</summary>
		public IConsolePrinter Printer { get; }

		/// <summary>The deferred registry access proxy.</summary>
		public RegistryProxy RegistryProxy { get; }

		/// <summary>The command history instance.</summary>
		public ICommandHistory History { get; }

		/// <summary>The interactive session controller.</summary>
		public IInteractiveSession InteractiveSession { get; }

		/// <summary>The session-wide macro and variable store.</summary>
		public ISessionState SessionState { get; }

		/// <summary>The declared runtime environment (e.g., Editor, Release).</summary>
		public EnvironmentTag ActiveEnvironment { get; }

		/// <summary>The controller handling programmatic lifecycle requests.</summary>
		public IShellController Controller { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ShellBootstrapContext"/> class.
		/// </summary>
		public ShellBootstrapContext(
			IConsolePrinter printer,
			RegistryProxy registryProxy,
			ICommandHistory history,
			IInteractiveSession interactiveSession,
			ISessionState sessionState,
			EnvironmentTag activeEnvironment,
			IShellController controller)
		{
			Printer = printer ?? throw new ArgumentNullException(nameof(printer));
			RegistryProxy = registryProxy ?? throw new ArgumentNullException(nameof(registryProxy));
			History = history ?? throw new ArgumentNullException(nameof(history));
			InteractiveSession = interactiveSession ?? throw new ArgumentNullException(nameof(interactiveSession));
			SessionState = sessionState ?? throw new ArgumentNullException(nameof(sessionState));
			ActiveEnvironment = activeEnvironment;
			Controller = controller ?? throw new ArgumentNullException(nameof(controller));
		}
	}
}