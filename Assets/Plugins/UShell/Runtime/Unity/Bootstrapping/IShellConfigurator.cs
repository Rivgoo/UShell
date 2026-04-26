using UShell.Runtime.Core.Bootstrapping;

namespace UShell.Runtime.Unity.Bootstrapping
{
	public interface IShellConfigurator
	{
		void Configure(ShellBuilder builder, ShellBootstrapContext context);
	}
}