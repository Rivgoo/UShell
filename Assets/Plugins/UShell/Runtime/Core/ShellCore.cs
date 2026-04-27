using System;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Execution.Context;
using UShell.Runtime.Core.History;

namespace UShell.Runtime.Core
{
	/// <summary>
	/// The default container tying all fundamental subsystems of the shell together.
	/// </summary>
	/// <remarks>
	/// Use <see cref="UShell.Runtime.Core.Bootstrapping.ShellBuilder"/> to instantiate this object.
	/// </remarks>
	public sealed class ShellCore : IShellCore
	{
		/// <inheritdoc/>
		public ICommandExecutor Executor { get; }

		/// <inheritdoc/>
		public ICommandRegistry Registry { get; }

		/// <inheritdoc/>
		public ICommandHistory History { get; }

		/// <inheritdoc/>
		public IInteractiveSession InteractiveSession { get; }

		/// <inheritdoc/>
		public ISessionState SessionState { get; }

		/// <summary>
		/// Initializes a new shell core instance encapsulating the resolved runtime components.
		/// </summary>
		public ShellCore(
			ICommandExecutor executor,
			ICommandRegistry registry,
			ICommandHistory history,
			IInteractiveSession interactiveSession,
			ISessionState sessionState)
		{
			Executor = executor ?? throw new ArgumentNullException(nameof(executor));
			Registry = registry ?? throw new ArgumentNullException(nameof(registry));
			History = history ?? throw new ArgumentNullException(nameof(history));
			InteractiveSession = interactiveSession ?? throw new ArgumentNullException(nameof(interactiveSession));
			SessionState = sessionState ?? throw new ArgumentNullException(nameof(sessionState));
		}
	}
}