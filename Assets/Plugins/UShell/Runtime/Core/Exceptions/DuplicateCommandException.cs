namespace UShell.Runtime.Core.Exceptions
{
	/// <summary>
	/// Thrown when attempting to register a command or alias that conflicts with a name already existing in the registry.
	/// </summary>
	public sealed class DuplicateCommandException : ShellConfigurationException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DuplicateCommandException"/> class.
		/// </summary>
		/// <param name="commandName">The name or alias that caused the collision.</param>
		public DuplicateCommandException(string commandName)
			: base($"Command or alias '{commandName}' is already registered.") { }
	}
}