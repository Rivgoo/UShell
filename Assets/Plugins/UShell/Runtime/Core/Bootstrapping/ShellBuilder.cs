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
	public sealed class ShellBuilder
	{
		private readonly IConsolePrinter _printer;
		private readonly EnvironmentTag _activeEnvironment;
		private readonly IInteractiveSession _interactiveSession;
		private readonly List<IShellProfile> _profiles = new();
		private readonly TypeParserRegistry _parserRegistry = new();
		private readonly CommandHistory _history;

		public ICommandHistory History => _history;

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

		public ShellBuilder AddProfile(IShellProfile profile)
		{
			if (profile == null) throw new ArgumentNullException(nameof(profile));

			_profiles.Add(profile);
			return this;
		}

		public ShellBuilder AddTypeParser<T>(ITypeParser<T> parser)
		{
			if (parser == null) throw new ArgumentNullException(nameof(parser));

			_parserRegistry.Register(parser);
			return this;
		}

		public IShellCore Build()
		{
			var commandBuilder = new ShellCommandBuilder();

			foreach (var profile in _profiles)
			{
				profile.RegisterCommands(commandBuilder);
			}

			IReadOnlyList<CommandSignature> validSignatures = commandBuilder.BuildAll(_activeEnvironment);
			var commandRegistry = new CommandRegistry(validSignatures);
			var argumentBinder = new ArgumentBinder(_parserRegistry);

			var executor = new CommandExecutor(commandRegistry, argumentBinder, _interactiveSession, _printer);

			return new ShellCore(executor, commandRegistry, _history, _interactiveSession);
		}
	}
}