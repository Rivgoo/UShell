namespace UShell.Runtime.Core.Diagnostics
{
	/// <summary>
	/// A structured diagnostic error payload generated during parsing, binding, or execution.
	/// </summary>
	/// <remarks>
	/// Errors are translated visually in the UI, often drawing arrows pointing to the exact character 
	/// in the user's input string that triggered the failure, based on <see cref="Position"/>.
	/// </remarks>
	public readonly struct ShellError
	{
		/// <summary>
		/// The specific enumerated error code defining the category of the failure.
		/// </summary>
		public ShellErrorCode Code { get; }

		/// <summary>
		/// The formatted human-readable explanation of the error.
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// The zero-based index in the raw input string where the error was detected. 
		/// Used to visually underline or point to the faulty syntax.
		/// </summary>
		public int Position { get; }

		private ShellError(ShellErrorCode code, string message, int position)
		{
			Code = code;
			Message = message;
			Position = position;
		}

		/// <summary>
		/// Factory method to create a new error by formatting a localized template string based on the code.
		/// </summary>
		public static ShellError Create(ShellErrorCode code, int position, params object[] args)
		{
			string template = ErrorMessages.GetTemplate(code);
			string formattedMessage = args.Length > 0 ? string.Format(template, args) : template;
			return new ShellError(code, formattedMessage, position);
		}

		/// <summary>
		/// Creates a copy of this error, overriding the character position.
		/// </summary>
		public ShellError WithPosition(int newPosition)
		{
			return new ShellError(Code, Message, newPosition);
		}
	}
}