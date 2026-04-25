namespace UShell.Runtime.Core.Diagnostics
{
	public enum ShellErrorCode
	{
		None = 0,

		Syntax_EmptyInput = 100,
		Syntax_ExpectedCommandName = 101,
		Syntax_ExpectedArgumentName = 102,
		Syntax_UnrecognizedSymbol = 103,
		Syntax_UnexpectedToken = 104,
		Syntax_UnclosedArray = 105,
		Syntax_ExpectedCommaOrBracket = 106,
		Syntax_MaxDepthExceeded = 107,
		Syntax_UnclosedString = 108,

		Bind_UnknownParameter = 200,
		Bind_ParameterAlreadyBound = 201,
		Bind_PositionalAfterNamed = 202,
		Bind_TooManyArguments = 203,
		Bind_TypeMismatch = 204,
		Bind_MissingRequiredParameter = 205,
		Bind_ExpectedScalarGotArray = 206,
		Bind_ExpectedArrayGotScalar = 207,
		Bind_MultidimensionalArrayNotSupported = 208,
		Bind_NoParserRegistered = 209,
		Bind_CustomError = 210,

		Execute_CommandNotFound = 300,
		Execute_Exception = 301,
		Execute_ExpressionError = 302
	}
}