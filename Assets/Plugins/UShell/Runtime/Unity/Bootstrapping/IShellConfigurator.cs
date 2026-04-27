using UShell.Runtime.Core.Bootstrapping;

namespace UShell.Runtime.Unity.Bootstrapping
{
	/// <summary>
	/// Represents an extension module that injects commands, profiles, or settings into the <see cref="ShellBuilder"/>.
	/// </summary>
	/// <remarks>
	/// Create a <see cref="UnityEngine.MonoBehaviour"/> implementing this interface and attach it 
	/// as a child to the main Bootstrapper GameObject to dynamically inject game-specific commands.
	/// </remarks>
	public interface IShellConfigurator
	{
		/// <summary>
		/// Executes configuration logic using the provided builder and context.
		/// </summary>
		/// <param name="builder">The builder used to attach profiles and parsers.</param>
		/// <param name="context">The initialized dependency context (e.g., history, printer).</param>
		void Configure(ShellBuilder builder, ShellBootstrapContext context);
	}
}