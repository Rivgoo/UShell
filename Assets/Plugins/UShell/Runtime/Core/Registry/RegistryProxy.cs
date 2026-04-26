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

		public IReadOnlyList<CommandSuggestion> GetSuggestions(ReadOnlySpan<char> prefix)
		{
			return Target?.GetSuggestions(prefix) ?? Array.Empty<CommandSuggestion>();
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

		public string GetCompactSignature(CommandSignature signature)
		{
			return Target?.GetCompactSignature(signature) ?? string.Empty;
		}
	}
}