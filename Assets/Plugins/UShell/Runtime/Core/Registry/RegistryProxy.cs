#nullable enable
using System;
using System.Collections.Generic;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands;

namespace UShell.Runtime.Core.Registry
{
	public sealed class RegistryProxy : ICommandRegistry
	{
		public ICommandRegistry? Target { get; set; }

		public IReadOnlyCollection<CommandSignature> GetAllCommands()
		{
			return Target?.GetAllCommands() ?? Array.Empty<CommandSignature>();
		}

		public IReadOnlyList<CommandSignature> GetSuggestions(ReadOnlySpan<char> prefix)
		{
			return Target?.GetSuggestions(prefix) ?? Array.Empty<CommandSignature>();
		}

		public bool TryGetCommand(string name, out CommandSignature signature)
		{
			if (Target != null)
			{
				return Target.TryGetCommand(name, out signature);
			}

			signature = null!;
			return false;
		}
	}
}