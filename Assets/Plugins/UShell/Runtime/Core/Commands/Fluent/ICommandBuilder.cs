namespace UShell.Runtime.Core.Commands.Fluent
{
	public interface ICommandBuilder
	{
		ICommandConfigurator WithName(string name);
	}
}