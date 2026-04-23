namespace UShell.Runtime.Core.Parsing.Lexing
{
	public enum TokenType : byte
	{
		Unknown,
		EndOfFile,
		Identifier,
		Number,
		String,
		Minus,
		LBracket,
		RBracket,
		Comma 
	}
}