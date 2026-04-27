using UShell.Runtime.Core.Commands.Fluent;

namespace UShell.Runtime.Core.Abstractions
{
	/// <summary>
	/// Represents a modular collection of commands that can be registered into the shell.
	/// </summary>
	/// <remarks>
	/// Implement this interface (or inherit from <see cref="ShellProfile"/>) to group 
	/// related functionality together (e.g., <c>AudioProfile</c>, <c>NetworkProfile</c>).
	/// Profiles are added during the bootstrapping phase of the shell.
	/// </remarks>
	public interface IShellProfile
	{
		/// <summary>
		/// Invoked during bootstrapping to register all commands associated with this profile.
		/// </summary>
		/// <param name="builder">The command builder used to configure new commands.</param>
		void RegisterCommands(ICommandBuilder builder);
	}
}