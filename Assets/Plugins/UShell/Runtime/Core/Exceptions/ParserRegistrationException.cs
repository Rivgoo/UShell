namespace UShell.Runtime.Core.Exceptions
{
	public sealed class ParserRegistrationException : ShellConfigurationException
	{
		public ParserRegistrationException(string message) : base(message) { }
	}
}