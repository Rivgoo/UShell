#nullable enable
using System;
using System.Collections.Generic;
using UShell.Runtime.Core.Execution.Invocation;

namespace UShell.Runtime.Core.Commands
{
	/// <summary>
	/// Represents the complete metadata, requirements, and execution contract of a registered shell command.
	/// </summary>
	public sealed class CommandSignature
	{
		/// <summary>
		/// The primary canonical name of the command.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The human-readable summary of what the command does, displayed in help menus.
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// A collection of alternative names that resolve to this command.
		/// </summary>
		public IReadOnlyList<string> Aliases { get; }

		/// <summary>
		/// The environments where this command is allowed to execute.
		/// </summary>
		public EnvironmentTag Tags { get; }

		/// <summary>
		/// The ordered list of parameters this command expects.
		/// </summary>
		public IReadOnlyList<CommandParameter> Parameters { get; }

		/// <summary>
		/// The wrapped delegate responsible for invoking the command logic.
		/// </summary>
		public ICommandInvoker Invoker { get; }

		/// <summary>
		/// The maximum permitted execution time. Only applicable if <see cref="IsInteractive"/> is <c>true</c>.
		/// </summary>
		public TimeSpan? Timeout { get; }

		/// <summary>
		/// Indicates if this command locks the shell to request prompts or await asynchronous interactions.
		/// </summary>
		public bool IsInteractive { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandSignature"/> class.
		/// </summary>
		public CommandSignature(
			string name,
			string description,
			IReadOnlyList<string> aliases,
			EnvironmentTag tags,
			IReadOnlyList<CommandParameter> parameters,
			ICommandInvoker invoker,
			TimeSpan? timeout = null,
			bool isInteractive = false)
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
			Timeout = timeout;
			IsInteractive = isInteractive;
		}
	}
}