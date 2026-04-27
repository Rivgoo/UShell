namespace UShell.Runtime.Core.Commands.Fluent
{
	/// <summary>
	/// The entry point for registering and configuring commands within a <see cref="UShell.Runtime.Core.Abstractions.IShellProfile"/>.
	/// </summary>
	public interface ICommandBuilder
	{
		/// <summary>
		/// Initializes the configuration chain for a new command with the specified canonical name.
		/// </summary>
		/// <param name="name">
		/// The primary name of the command. Cannot contain spaces, quotes, brackets, or commas.
		/// </param>
		/// <returns>A configurator instance used to define parameters, aliases, and execution logic.</returns>
		/// <exception cref="UShell.Runtime.Core.Exceptions.ShellConfigurationException">
		/// Thrown if the command name is invalid or empty.
		/// </exception>
		/// <example>
		/// <code>
		/// builder.WithName("spawn.enemy")
		///        .WithDescription("Spawns an enemy at the current look position.")
		///        .Executes(SpawnEnemy);
		/// </code>
		/// </example>
		ICommandConfigurator WithName(string name);
	}
}