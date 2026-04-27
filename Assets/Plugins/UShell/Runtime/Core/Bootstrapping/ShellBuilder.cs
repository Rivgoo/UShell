using System;
using System.Collections.Generic;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Commands.Fluent;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Execution.Binding;
using UShell.Runtime.Core.Execution.Context;
using UShell.Runtime.Core.History;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Core.Parsing.Types;
using UShell.Runtime.Core.Parsing.Types.BuiltIn;
using UShell.Runtime.Core.Registry;

namespace UShell.Runtime.Core.Bootstrapping
{
	/// <summary>
	/// The primary configuration orchestrator used to assemble, wire, and instantiate a functional <see cref="IShellCore"/>.
	/// </summary>
	/// <remarks>
	/// Developers interact with this class during the application startup phase to register custom profiles, 
	/// type parsers, and set environment restrictions before calling <see cref="Build"/>.
	/// </remarks>
	public sealed class ShellBuilder
	{
		private readonly IConsolePrinter _printer;
		private readonly EnvironmentTag _activeEnvironment;
		private readonly IInteractiveSession _interactiveSession;
		private readonly List<IShellProfile> _profiles = new();
		private readonly TypeParserRegistry _parserRegistry = new();
		private readonly CommandHistory _history;
		private readonly SessionState _sessionState = new();

		/// <summary>
		/// Provides direct access to the history instance being configured.
		/// </summary>
		public ICommandHistory History => _history;

		/// <summary>
		/// Provides direct access to the session state instance being configured.
		/// </summary>
		public ISessionState SessionState => _sessionState;

		/// <summary>
		/// Initializes a new instance of the builder with required dependencies.
		/// </summary>
		public ShellBuilder(
			IConsolePrinter printer,
			EnvironmentTag activeEnvironment,
			IInteractiveSession interactiveSession,
			int historyCapacity = 150)
		{
			_printer = printer ?? throw new ArgumentNullException(nameof(printer));
			_activeEnvironment = activeEnvironment;
			_interactiveSession = interactiveSession ?? throw new ArgumentNullException(nameof(interactiveSession));
			_history = new CommandHistory(historyCapacity);

			_parserRegistry.Register(new IntParser());
			_parserRegistry.Register(new FloatParser());
			_parserRegistry.Register(new BoolParser());
			_parserRegistry.Register(new StringParser());
		}

		/// <summary>
		/// Appends a new module of commands to the shell.
		/// </summary>
		/// <param name="profile">The initialized profile instance.</param>
		/// <returns>The current builder instance for chaining.</returns>
		public ShellBuilder AddProfile(IShellProfile profile)
		{
			if (profile == null) throw new ArgumentNullException(nameof(profile));

			_profiles.Add(profile);
			return this;
		}

		/// <summary>
		/// Registers a custom parser that teaches the shell how to convert strings into a specific object type.
		/// </summary>
		/// <typeparam name="T">The target type to parse.</typeparam>
		/// <param name="parser">The parser logic.</param>
		/// <returns>The current builder instance for chaining.</returns>
		public ShellBuilder AddTypeParser<T>(ITypeParser<T> parser)
		{
			if (parser == null) throw new ArgumentNullException(nameof(parser));

			_parserRegistry.Register(parser);
			return this;
		}

		/// <summary>
		/// Validates all profiles, builds command signatures, filters them by the active environment, 
		/// and wires the final dependency graph.
		/// </summary>
		/// <returns>A fully initialized, ready-to-use shell core.</returns>
		public IShellCore Build()
		{
			var commandBuilder = new ShellCommandBuilder();

			foreach (var profile in _profiles)
			{
				profile.RegisterCommands(commandBuilder);
			}

			IReadOnlyList<CommandSignature> validSignatures = commandBuilder.BuildAll(_activeEnvironment);
			var commandRegistry = new CommandRegistry(validSignatures);
			var argumentBinder = new ArgumentBinder(_parserRegistry, _sessionState);

			var executor = new CommandExecutor(commandRegistry, argumentBinder, _interactiveSession, _printer, _sessionState);

			return new ShellCore(executor, commandRegistry, _history, _interactiveSession, _sessionState);
		}
	}
}