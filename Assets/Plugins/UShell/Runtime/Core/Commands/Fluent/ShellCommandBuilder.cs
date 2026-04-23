using System.Collections.Generic;
using UShell.Runtime.Core.Commands.Exceptions;

namespace UShell.Runtime.Core.Commands.Fluent
{
	internal sealed class ShellCommandBuilder : ICommandBuilder
	{
		private readonly List<CommandConfigurator> _configurators = new();

		public ICommandConfigurator WithName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ShellConfigurationException("Command name cannot be empty.");
			}

			var configurator = new CommandConfigurator(name);
			_configurators.Add(configurator);
			return configurator;
		}

		public IReadOnlyList<CommandSignature> BuildAll(EnvironmentTag activeEnvironment)
		{
			var signatures = new List<CommandSignature>(_configurators.Count);

			foreach (var configurator in _configurators)
			{
				CommandSignature signature = configurator.Build();

				if ((signature.Tags & activeEnvironment) != 0 || signature.Tags == EnvironmentTag.Any)
				{
					signatures.Add(signature);
				}
			}

			return signatures.AsReadOnly();
		}
	}
}