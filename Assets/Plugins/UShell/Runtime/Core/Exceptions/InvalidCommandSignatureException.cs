namespace UShell.Runtime.Core.Exceptions
{
	/// <summary>
	/// Thrown when a command name or parameter name contains invalid characters (like spaces, quotes, or brackets).
	/// </summary>
	public sealed class InvalidCommandSignatureException : ShellConfigurationException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidCommandSignatureException"/> class.
		/// </summary>
		public InvalidCommandSignatureException(string message) : base(message) { }
	}
}