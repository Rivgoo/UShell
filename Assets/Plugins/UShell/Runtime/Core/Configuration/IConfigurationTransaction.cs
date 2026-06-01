using System;
using System.Threading;
using System.Threading.Tasks;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Parsing.Types;

namespace UShell.Runtime.Core.Configuration
{
	/// <summary>
	/// Represents a pending Unit of Work for mutating the shell's configuration (adding/removing profiles and parsers) at runtime.
	/// </summary>
	/// <remarks>
	/// Provides a fluent API for chaining additions and removals. Mutations are applied transactionally 
	/// when <see cref="ApplyAsync"/> is invoked, ensuring no race conditions occur during active command execution.
	/// </remarks>
	public interface IConfigurationTransaction
	{
		/// <summary>
		/// Stages a new profile to be added to the registry.
		/// </summary>
		IConfigurationTransaction AddProfile(IShellProfile profile);

		/// <summary>
		/// Stages a new custom type parser to be added to the registry.
		/// </summary>
		IConfigurationTransaction AddTypeParser<T>(ITypeParser<T> parser);

		/// <summary>
		/// Stages all profiles matching the generic type parameter <typeparamref name="TProfile"/> to be removed from the registry.
		/// </summary>
		IConfigurationTransaction RemoveProfile<TProfile>() where TProfile : IShellProfile;

		/// <summary>
		/// Stages all profiles matching the specified type to be removed from the registry.
		/// </summary>
		IConfigurationTransaction RemoveProfile(Type profileType);

		/// <summary>
		/// Stages a specific profile instance to be removed from the registry.
		/// </summary>
		IConfigurationTransaction RemoveProfile(IShellProfile profile);

		/// <summary>
		/// Stages the parser mapped to <typeparamref name="TTarget"/> to be removed from the registry.
		/// </summary>
		IConfigurationTransaction RemoveTypeParser<TTarget>();

		/// <summary>
		/// Stages the parser mapped to the specified target type to be removed from the registry.
		/// </summary>
		IConfigurationTransaction RemoveTypeParser(Type targetType);

		/// <summary>
		/// Asynchronously applies all staged mutations. 
		/// </summary>
		/// <remarks>
		/// This method will automatically wait for any currently running commands to finish, 
		/// close the shell UI, lock the session, apply the changes, and release the lock.
		/// </remarks>
		Task ApplyAsync(CancellationToken cancellationToken = default);
	}
}