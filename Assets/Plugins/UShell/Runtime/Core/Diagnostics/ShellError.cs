namespace UShell.Runtime.Core.Diagnostics
{
	public readonly struct ShellError
	{
		public ShellErrorCode Code { get; }
		public string Message { get; }
		public int Position { get; }

		private ShellError(ShellErrorCode code, string message, int position)
		{
			Code = code;
			Message = message;
			Position = position;
		}

		public static ShellError Create(ShellErrorCode code, int position, params object[] args)
		{
			string template = ErrorMessages.GetTemplate(code);
			string formattedMessage = args.Length > 0 ? string.Format(template, args) : template;
			return new ShellError(code, formattedMessage, position);
		}

		public ShellError WithPosition(int newPosition)
		{
			return new ShellError(Code, Message, newPosition);
		}
	}
}