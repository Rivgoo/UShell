namespace UShell.Runtime.Core.Exceptions
{
	/// <summary>
	/// Thrown when attempting to register a <see cref="UShell.Runtime.Core.Parsing.Types.ITypeParser"/> 
	/// for a Type that already has an active parser assigned to it.
	/// </summary>
	public sealed class ParserRegistrationException : ShellConfigurationException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserRegistrationException"/> class.
		/// </summary>
		public ParserRegistrationException(string message) : base(message) { }
	}
}