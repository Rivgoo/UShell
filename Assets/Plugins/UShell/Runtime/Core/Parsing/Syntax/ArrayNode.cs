using System;
using System.Collections.Generic;

namespace UShell.Runtime.Core.Parsing.Syntax
{
	public sealed class ArrayNode : SyntaxNode
	{
		public IReadOnlyList<SyntaxNode> Elements { get; }

		public ArrayNode(int startIndex, int length, IReadOnlyList<SyntaxNode> elements) : base(startIndex, length)
		{
			Elements = elements ?? throw new ArgumentNullException(nameof(elements));
		}
	}
}