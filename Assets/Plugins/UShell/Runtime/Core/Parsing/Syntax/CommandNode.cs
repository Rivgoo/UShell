using System;
using System.Collections.Generic;

namespace UShell.Runtime.Core.Parsing.Syntax
{
	public sealed class CommandNode : SyntaxNode
	{
		public string CommandName { get; }
		public IReadOnlyList<SyntaxNode> Arguments { get; }

		public CommandNode(int startIndex, int length, string commandName, IReadOnlyList<SyntaxNode> arguments) : base(startIndex, length)
		{
			if (string.IsNullOrWhiteSpace(commandName))
			{
				throw new ArgumentException("Command name cannot be empty.", nameof(commandName));
			}

			CommandName = commandName;
			Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
		}
	}
}