namespace UShell.Runtime.Core.Parsing.Syntax
{
	public abstract class SyntaxNode
	{
		public int StartIndex { get; }
		public int Length { get; }

		protected SyntaxNode(int startIndex, int length)
		{
			StartIndex = startIndex;
			Length = length;
		}
	}
}