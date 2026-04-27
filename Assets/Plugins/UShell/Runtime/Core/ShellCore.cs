using System;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Execution.Context;
using UShell.Runtime.Core.History;

namespace UShell.Runtime.Core
{
	public sealed class ShellCore : IShellCore
	{
		public ICommandExecutor Executor { get; }
		public ICommandRegistry Registry { get; }
		public ICommandHistory History { get; }
		public IInteractiveSession InteractiveSession { get; }
		public ISessionState SessionState { get; }

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