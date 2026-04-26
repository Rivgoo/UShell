using System;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.History;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Core.Registry;

namespace UShell.Runtime.Unity.Bootstrapping
{
	public sealed class ShellBootstrapContext
	{
		public IConsolePrinter Printer { get; }
		public RegistryProxy RegistryProxy { get; }
		public ICommandHistory History { get; }
		public EnvironmentTag ActiveEnvironment { get; }

		public ShellBootstrapContext(
			IConsolePrinter printer,
			RegistryProxy registryProxy,
			ICommandHistory history,
			EnvironmentTag activeEnvironment)
		{
			Printer = printer ?? throw new ArgumentNullException(nameof(printer));
			RegistryProxy = registryProxy ?? throw new ArgumentNullException(nameof(registryProxy));
			History = history ?? throw new ArgumentNullException(nameof(history));
			ActiveEnvironment = activeEnvironment;
		}
	}
}