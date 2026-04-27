using System;

namespace UShell.Runtime.Core.Parsing.Syntax
{
	public sealed class AssignmentNode : SyntaxNode
	{
		public string VariableName { get; }
		public SyntaxNode RightSide { get; }

		public AssignmentNode(int startIndex, int length, string variableName, SyntaxNode rightSide) : base(startIndex, length)
		{
			if (string.IsNullOrWhiteSpace(variableName))
			{
				throw new ArgumentException("Variable name cannot be empty.", nameof(variableName));
			}

			VariableName = variableName;
			RightSide = rightSide ?? throw new ArgumentNullException(nameof(rightSide));
		}
	}
}