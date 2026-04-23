using UShell.Runtime.Core.Commands.Fluent;

namespace UShell.Runtime.Core.Abstractions
{
	public interface IShellProfile
	{
		void RegisterCommands(ICommandBuilder builder);
	}
}