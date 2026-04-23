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
			foreach (char ch in word)
				current = current.GetOrAddChild(char.ToLowerInvariant(ch));
			current.Command = command;
		}

		public IReadOnlyList<CommandSignature> GetSuggestions(ReadOnlySpan<char> prefix)
		{
			TrieNode? current = _root;
			for (int i = 0; i < prefix.Length; i++)
				if (!current.TryGetChild(char.ToLowerInvariant(prefix[i]), out current))
					return Array.Empty<CommandSignature>();

			var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var results = new List<CommandSignature>();
			CollectCommands(current!, seen, results);
			results.Sort(static (a, b) =>
				string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
			return results;
		}

		private static void CollectCommands(TrieNode node, HashSet<string> seen, List<CommandSignature> results)
		{
			if (node.Command != null && seen.Add(node.Command.Name))
				results.Add(node.Command);
			foreach (TrieNode child in node.GetChildren())
				CollectCommands(child, seen, results);
		}
	}
}