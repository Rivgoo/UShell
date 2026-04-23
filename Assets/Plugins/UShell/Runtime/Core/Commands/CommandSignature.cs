#nullable enable
using System;
using System.Collections.Generic;
using UShell.Runtime.Core.Execution.Invocation;

namespace UShell.Runtime.Core.Commands
{
	public sealed class CommandSignature
	{
		public string Name { get; }
		public string Description { get; }
		public IReadOnlyList<string> Aliases { get; }
		public EnvironmentTag Tags { get; }
		public IReadOnlyList<CommandParameter> Parameters { get; }
		public ICommandInvoker Invoker { get; }

		public CommandSignature(
			string name,
			string description,
			IReadOnlyList<string> aliases,
			EnvironmentTag tags,
			IReadOnlyList<CommandParameter> parameters,
			ICommandInvoker invoker)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Command name cannot be empty.", nameof(name));
			}

			Name = name;
			Description = description ?? string.Empty;
			Aliases = aliases ?? Array.Empty<string>();
			Tags = tags;
			Parameters = parameters ?? Array.Empty<CommandParameter>();
			Invoker = invoker ?? throw new ArgumentNullException(nameof(invoker));
		}
	}
}