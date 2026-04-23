using System;

namespace UShell.Runtime.Core.Parsing.Syntax
{
	public sealed class StringNode : SyntaxNode
	{
		public string Value { get; }

		public StringNode(int startIndex, int length, string value) : base(startIndex, length)
		{
			Value = value ?? throw new ArgumentNullException(nameof(value));
		}
	}
}