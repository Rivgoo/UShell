using System;

namespace UShell.Runtime.Core.Exceptions
{
	/// <summary>
	/// The base exception type thrown when a fatal misconfiguration occurs during the shell's bootstrap phase.
	/// </summary>
	/// <remarks>
	/// Catching this generally indicates a developer error in command registration, 
	/// such as type mismatches in the fluent builder, missing required methods, or invalid structures.
	/// </remarks>
	public class ShellConfigurationException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ShellConfigurationException"/> class.
		/// </summary>
		public ShellConfigurationException(string message) : base(message) { }
	}
}