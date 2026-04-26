#nullable enable
using System.Collections.Generic;
using UShell.Runtime.Core.Commands;

namespace UShell.Runtime.Core.Registry.Autocomplete
{
	internal sealed class TrieNode
	{
		private Dictionary<char, TrieNode>? _children;

		public CommandSignature? Command { get; set; }
		public string? MatchedWord { get; set; }

		public TrieNode GetOrAddChild(char c)
		{
			_children ??= new Dictionary<char, TrieNode>();

			if (!_children.TryGetValue(c, out TrieNode? node))
			{
				node = new TrieNode();
				_children[c] = node;
			}

			return node;
		}

		public bool TryGetChild(char c, out TrieNode? node)
		{
			node = null;
			return _children != null && _children.TryGetValue(c, out node);
		}

		public IEnumerable<TrieNode> GetChildren()
		{
			if (_children == null)
			{
				yield break;
			}

			foreach (TrieNode child in _children.Values)
			{
				yield return child;
			}
		}
	}
}