using System;

namespace UShell.Runtime.Core.Parsing.Syntax
{
	public sealed class NamedArgumentNode : SyntaxNode
	{
		public string Name { get; }
		public SyntaxNode Value { get; }

		public NamedArgumentNode(int startIndex, int length, string name, SyntaxNode value) : base(startIndex, length)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Argument name cannot be empty.", nameof(name));
			}

			Name = name;
			Value = value ?? throw new ArgumentNullException(nameof(value));
		}
	}
}