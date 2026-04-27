using System;

namespace UShell.Runtime.Core.Parsing.Syntax
{
	public sealed class VariableNode : SyntaxNode
	{
		public string Name { get; }

		public VariableNode(int startIndex, int length, string name) : base(startIndex, length)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Variable name cannot be empty.", nameof(name));
			}

			Name = name;
		}
	}
}