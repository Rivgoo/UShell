using System;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Configuration;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Execution.Context;
using UShell.Runtime.Core.History;
using UShell.Runtime.Core.Parsing.Types;
using UShell.Runtime.Core.Registry;

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

		private readonly ITypeParserRegistry _parserRegistry;
		private readonly EnvironmentTag _activeEnvironment;
		private readonly IShellController _controller;

		/// <summary>
		/// Initializes a new shell core instance encapsulating the resolved runtime components.
		/// </summary>
		public ShellCore(
			ICommandExecutor executor,
			ICommandRegistry registry,
			ICommandHistory history,
			IInteractiveSession interactiveSession,
			ISessionState sessionState,
			ITypeParserRegistry parserRegistry,
			EnvironmentTag activeEnvironment,
			IShellController controller)
		{
			Executor = executor ?? throw new ArgumentNullException(nameof(executor));
			Registry = registry ?? throw new ArgumentNullException(nameof(registry));
			History = history ?? throw new ArgumentNullException(nameof(history));
			InteractiveSession = interactiveSession ?? throw new ArgumentNullException(nameof(interactiveSession));
			SessionState = sessionState ?? throw new ArgumentNullException(nameof(sessionState));
			_parserRegistry = parserRegistry ?? throw new ArgumentNullException(nameof(parserRegistry));
			_activeEnvironment = activeEnvironment;
			_controller = controller ?? throw new ArgumentNullException(nameof(controller));
		}

		/// <inheritdoc/>
		public IConfigurationTransaction BeginConfigurationTransaction()
		{
			return new ConfigurationTransaction(
				(CommandRegistry)Registry,
				_parserRegistry,
				InteractiveSession,
				_controller,
				_activeEnvironment);
		}
	}
}