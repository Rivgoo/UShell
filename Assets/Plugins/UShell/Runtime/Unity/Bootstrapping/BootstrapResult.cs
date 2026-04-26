using System;
using UShell.Runtime.Core;
using UShell.Runtime.Core.Registry;
using UShell.Runtime.Unity.Output;

namespace UShell.Runtime.Unity.Bootstrapping
{
	public sealed class BootstrapResult
	{
		public IShellCore Core { get; }
		public UnityConsolePrinter Printer { get; }
		public RegistryProxy RegistryProxy { get; }

		public BootstrapResult(
			IShellCore core,
			UnityConsolePrinter printer,
			RegistryProxy registryProxy)
		{
			Core = core ?? throw new ArgumentNullException(nameof(core));
			Printer = printer ?? throw new ArgumentNullException(nameof(printer));
			RegistryProxy = registryProxy ?? throw new ArgumentNullException(nameof(registryProxy));
		}
	}
}