using System;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.History;

namespace UShell.Runtime.Core
{
	public sealed class ShellCore : IShellCore
	{
		public ICommandExecutor Executor { get; }
		public ICommandRegistry Registry { get; }
		public ICommandHistory History { get; }

		public ShellCore(ICommandExecutor executor, ICommandRegistry registry, ICommandHistory history)
		{
			Executor = executor ?? throw new ArgumentNullException(nameof(executor));
			Registry = registry ?? throw new ArgumentNullException(nameof(registry));
			History = history ?? throw new ArgumentNullException(nameof(history));
		}
	}
}