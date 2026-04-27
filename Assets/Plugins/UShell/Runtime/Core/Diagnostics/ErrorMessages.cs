namespace UShell.Runtime.Core.Diagnostics
{
	internal static class ErrorMessages
	{
		public static string GetTemplate(ShellErrorCode code)
		{
			return code switch
			{
				ShellErrorCode.Syntax_EmptyInput => "Input is empty.",
				ShellErrorCode.Syntax_ExpectedCommandOrVariable => "Expected command name or macro/variable assignment, but got '{0}'.",
				ShellErrorCode.Syntax_ExpectedArgumentName => "Expected argument name after '-'.",
				ShellErrorCode.Syntax_UnrecognizedSymbol => "Unrecognized or unclosed symbol.",
				ShellErrorCode.Syntax_UnexpectedToken => "Unexpected token '{0}'.",
				ShellErrorCode.Syntax_UnclosedArray => "Unclosed array: expected ']'.",
				ShellErrorCode.Syntax_ExpectedCommaOrBracket => "Expected ',' or ']'.",
				ShellErrorCode.Syntax_MaxDepthExceeded => "Maximum parsing depth exceeded. Expression is too complex.",
				ShellErrorCode.Syntax_UnclosedString => "Unclosed string literal.",

				ShellErrorCode.Bind_UnknownParameter => "Unknown parameter '{0}'.",
				ShellErrorCode.Bind_ParameterAlreadyBound => "Parameter '{0}' is already bound.",
				ShellErrorCode.Bind_PositionalAfterNamed => "Positional arguments cannot appear after named arguments.",
				ShellErrorCode.Bind_TooManyArguments => "Too many arguments provided.",
				ShellErrorCode.Bind_TypeMismatch => "Cannot parse '{0}' as type '{1}'.",
				ShellErrorCode.Bind_MissingRequiredParameter => "Missing required parameter '{0}'.",
				ShellErrorCode.Bind_ExpectedScalarGotArray => "Expected a single value for parameter '{0}', but got an array.",
				ShellErrorCode.Bind_ExpectedArrayGotScalar => "Expected an array for parameter '{0}', but got a single value.",
				ShellErrorCode.Bind_MultidimensionalArrayNotSupported => "Multidimensional or nested arrays are not supported.",
				ShellErrorCode.Bind_NoParserRegistered => "No type parser registered for type '{0}'.",
				ShellErrorCode.Bind_CustomError => "{0}",

				ShellErrorCode.Execute_CommandNotFound => "Unknown command '{0}'. Type 'help' to list commands.",
				ShellErrorCode.Execute_Exception => "Execution Error: {0}",
				ShellErrorCode.Execute_ExpressionError => "Expression Error: {0}",
				ShellErrorCode.Execute_MacroOrSessionVariableNotFound => "Macro or session variable '{0}' is not defined.",

				_ => "Unknown error."
			};
		}
	}
}