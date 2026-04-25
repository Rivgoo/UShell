namespace UShell.Runtime.Core.Exceptions
{
	public sealed class InvalidCommandSignatureException : ShellConfigurationException
	{
		public InvalidCommandSignatureException(string message) : base(message) { }
	}
}