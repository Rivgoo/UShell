namespace UShell.Runtime.Core.Diagnostics
{
	/// <summary>
	/// Defines specific diagnostic codes for parsing, binding, and execution failures.
	/// </summary>
	public enum ShellErrorCode
	{
		/// <summary>No error.</summary>
		None = 0,

		// --- Syntax (Parser) Errors 1xx ---

		/// <summary>The user submitted an empty string.</summary>
		Syntax_EmptyInput = 100,
		/// <summary>The AST expected an identifier but received something else.</summary>
		Syntax_ExpectedCommandOrVariable = 101,
		/// <summary>A hyphen '-' was placed but no valid parameter name followed.</summary>
		Syntax_ExpectedArgumentName = 102,
		/// <summary>An illegal or unclosed character sequence was detected.</summary>
		Syntax_UnrecognizedSymbol = 103,
		/// <summary>A token appeared in an illegal structural position.</summary>
		Syntax_UnexpectedToken = 104,
		/// <summary>An array started with '[' but was never closed.</summary>
		Syntax_UnclosedArray = 105,
		/// <summary>An array item was not followed by a comma or closing bracket.</summary>
		Syntax_ExpectedCommaOrBracket = 106,
		/// <summary>Nested macro expressions exceeded the maximum allowed depth.</summary>
		Syntax_MaxDepthExceeded = 107,
		/// <summary>A string literal started with '"' but was never closed.</summary>
		Syntax_UnclosedString = 108,

		// --- Binding Errors 2xx ---

		/// <summary>The user provided a named argument that does not exist on the command.</summary>
		Bind_UnknownParameter = 200,
		/// <summary>The user provided the same argument twice.</summary>
		Bind_ParameterAlreadyBound = 201,
		/// <summary>A positional argument appeared after a named argument.</summary>
		Bind_PositionalAfterNamed = 202,
		/// <summary>The user passed more positional arguments than the command accepts.</summary>
		Bind_TooManyArguments = 203,
		/// <summary>The provided text could not be parsed into the expected .NET type.</summary>
		Bind_TypeMismatch = 204,
		/// <summary>A mandatory parameter without a default value was omitted.</summary>
		Bind_MissingRequiredParameter = 205,
		/// <summary>The command expected a single value but received an array.</summary>
		Bind_ExpectedScalarGotArray = 206,
		/// <summary>The command expected an array but received a single scalar value.</summary>
		Bind_ExpectedArrayGotScalar = 207,
		/// <summary>Nested multidimensional arrays are passed, which is unsupported.</summary>
		Bind_MultidimensionalArrayNotSupported = 208,
		/// <summary>A command requires a type that has no registered parser.</summary>
		Bind_NoParserRegistered = 209,
		/// <summary>A custom error explicitly thrown by a <see cref="UShell.Runtime.Core.Parsing.Types.ITypeParser"/>.</summary>
		Bind_CustomError = 210,

		// --- Execution Errors 3xx ---

		/// <summary>The requested command is not registered in the current environment.</summary>
		Execute_CommandNotFound = 300,
		/// <summary>The underlying command delegate threw an unhandled .NET exception.</summary>
		Execute_Exception = 301,
		/// <summary>An error occurred evaluating a math expression.</summary>
		Execute_ExpressionError = 302,
		/// <summary>The user referenced a macro/variable via '$' that does not exist in the session state.</summary>
		Execute_MacroOrSessionVariableNotFound = 303
	}
}