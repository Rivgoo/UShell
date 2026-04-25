#nullable enable
using System;
using System.Collections.Generic;
using UShell.Runtime.Core.Commands;

namespace UShell.Runtime.Core.Registry.Autocomplete
{
	internal sealed class AutocompleteTrie
	{
		private readonly TrieNode _root = new();

		public void Insert(string word, CommandSignature command)
		{
			TrieNode current = _root;

			foreach (char character in word)
			{
				current = current.GetOrAddChild(char.ToLowerInvariant(character));
			}

			current.Command = command;
		}

		public IReadOnlyList<CommandSignature> GetSuggestions(ReadOnlySpan<char> prefix)
		{
			TrieNode? current = FindNodeByPrefix(prefix);

			if (current == null)
			{
				return Array.Empty<CommandSignature>();
			}

			var seenCommands = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var results = new List<CommandSignature>();

			CollectCommandsRecursive(current, seenCommands, results);

			results.Sort(static (a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

			return results;
		}

		private TrieNode? FindNodeByPrefix(ReadOnlySpan<char> prefix)
		{
			TrieNode? current = _root;

			for (int i = 0; i < prefix.Length; i++)
			{
				if (current == null || !current.TryGetChild(char.ToLowerInvariant(prefix[i]), out current))
				{
					return null;
				}
			}

			return current;
		}

		private static void CollectCommandsRecursive(TrieNode node, HashSet<string> seen, List<CommandSignature> results)
		{
			if (node.Command != null && seen.Add(node.Command.Name))
			{
				results.Add(node.Command);
			}

			foreach (TrieNode child in node.GetChildren())
			{
				CollectCommandsRecursive(child, seen, results);
			}
		}
	}
}