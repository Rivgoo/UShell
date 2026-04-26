using System;
using System.Collections.Generic;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Exceptions;
using UShell.Runtime.Core.Output.Formatting;
using UShell.Runtime.Core.Registry.Autocomplete;

namespace UShell.Runtime.Core.Registry
{
	public sealed class CommandRegistry : ICommandRegistry
	{
		private readonly Dictionary<string, CommandSignature> _commands = new(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<CommandSignature, string> _signatureCache = new();
		private readonly AutocompleteTrie _trie = new();

		public CommandRegistry(IReadOnlyList<CommandSignature> signatures)
		{
			foreach (CommandSignature sig in signatures)
			{
				RegisterSignature(sig);
			}
		}

		public bool TryGetCommand(string name, out CommandSignature signature) => _commands.TryGetValue(name, out signature!);

		public IReadOnlyList<CommandSuggestion> GetSuggestions(ReadOnlySpan<char> prefix)
		{
			if (prefix.IsEmpty || prefix.IsWhiteSpace())
			{
				return Array.Empty<CommandSuggestion>();
			}
			return _trie.GetSuggestions(prefix);
		}

		public IReadOnlyCollection<CommandSignature> GetAllCommands() => _commands.Values;

		public string GetCompactSignature(CommandSignature signature)
		{
			if (!_signatureCache.TryGetValue(signature, out string compact))
			{
				compact = ProfileFormatter.FormatCompactSignature(signature);
				_signatureCache[signature] = compact;
			}
			return compact;
		}

		internal void MergeSignatures(IReadOnlyList<CommandSignature> signatures)
		{
			foreach (CommandSignature sig in signatures)
			{
				RegisterSignature(sig);
			}
		}

		private void RegisterSignature(CommandSignature sig)
		{
			RegisterKey(sig.Name, sig);

			foreach (string alias in sig.Aliases)
			{
				RegisterKey(alias, sig);
			}
		}

		private void RegisterKey(string key, CommandSignature sig)
		{
			if (_commands.ContainsKey(key))
			{
				throw new DuplicateCommandException(key);
			}

			_commands[key] = sig;
			_trie.Insert(key, sig);
		}
	}
}