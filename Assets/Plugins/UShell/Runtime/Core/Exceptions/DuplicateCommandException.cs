namespace UShell.Runtime.Core.Exceptions
{
	public sealed class DuplicateCommandException : ShellConfigurationException
	{
		public DuplicateCommandException(string commandName)
			: base($"Command or alias '{commandName}' is already registered.") { }
	}
}