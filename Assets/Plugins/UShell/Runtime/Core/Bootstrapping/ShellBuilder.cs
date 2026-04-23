using System;
using System.Collections.Generic;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Commands.Fluent;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Execution.Binding;
using UShell.Runtime.Core.Parsing.Types;
using UShell.Runtime.Core.Parsing.Types.BuiltIn;
using UShell.Runtime.Core.Registry;

namespace UShell.Runtime.Core.Bootstrapping
{
	public sealed class ShellBuilder
	{
		private readonly EnvironmentTag _activeEnvironment;
		private readonly List<IShellProfile> _profiles = new();
		private readonly TypeParserRegistry _parserRegistry = new();

		public ShellBuilder(EnvironmentTag activeEnvironment)
		{
			_activeEnvironment = activeEnvironment;

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
			var executor = new CommandExecutor(commandRegistry, argumentBinder);

			return new ShellCore(executor, commandRegistry);
		}
	}
}