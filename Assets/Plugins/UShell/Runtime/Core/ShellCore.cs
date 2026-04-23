using System;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core
{
	public sealed class ShellCore : IShellCore
	{
		public ICommandExecutor Executor { get; }
		public ICommandRegistry Registry { get; }

		public ShellCore(ICommandExecutor executor, ICommandRegistry registry)
		{
			Executor = executor ?? throw new ArgumentNullException(nameof(executor));
			Registry = registry ?? throw new ArgumentNullException(nameof(registry));
		}
	}
}