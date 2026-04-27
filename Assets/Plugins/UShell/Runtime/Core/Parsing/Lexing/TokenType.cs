namespace UShell.Runtime.Core.Parsing.Lexing
{
	public enum TokenType : byte
	{
		Unknown,
		EndOfFile,
		Identifier,
		Number,
		String,
		Variable,
		Equals,
		Minus,
		LBracket,
		RBracket,
		Comma
	}
}