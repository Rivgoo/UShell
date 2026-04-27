#nullable enable
using System;
using System.Threading.Tasks;
using UShell.Runtime.Core.Execution.Context;

namespace UShell.Runtime.Core.Execution.Invocation
{
	internal sealed class InteractiveFuncInvoker : ICommandInvoker
	{
		private readonly Func<ICommandContext, Task> _func;
		public InteractiveFuncInvoker(Func<ICommandContext, Task> func) => _func = func ?? throw new ArgumentNullException(nameof(func));
		public object? Invoke(ICommandContext? context, object?[] args) => _func(context!);
	}

	internal sealed class InteractiveFuncInvoker<T1> : ICommandInvoker
	{
		private readonly Func<ICommandContext, T1, Task> _func;
		public InteractiveFuncInvoker(Func<ICommandContext, T1, Task> func) => _func = func ?? throw new ArgumentNullException(nameof(func));
		public object? Invoke(ICommandContext? context, object?[] args) => _func(context!, (T1)args[0]!);
	}

	internal sealed class InteractiveFuncInvoker<T1, T2> : ICommandInvoker
	{
		private readonly Func<ICommandContext, T1, T2, Task> _func;
		public InteractiveFuncInvoker(Func<ICommandContext, T1, T2, Task> func) => _func = func ?? throw new ArgumentNullException(nameof(func));
		public object? Invoke(ICommandContext? context, object?[] args) => _func(context!, (T1)args[0]!, (T2)args[1]!);
	}

	internal sealed class InteractiveFuncInvoker<T1, T2, T3> : ICommandInvoker
	{
		private readonly Func<ICommandContext, T1, T2, T3, Task> _func;
		public InteractiveFuncInvoker(Func<ICommandContext, T1, T2, T3, Task> func) => _func = func ?? throw new ArgumentNullException(nameof(func));
		public object? Invoke(ICommandContext? context, object?[] args) => _func(context!, (T1)args[0]!, (T2)args[1]!, (T3)args[2]!);
	}

	internal sealed class InteractiveFuncInvoker<T1, T2, T3, T4> : ICommandInvoker
	{
		private readonly Func<ICommandContext, T1, T2, T3, T4, Task> _func;
		public InteractiveFuncInvoker(Func<ICommandContext, T1, T2, T3, T4, Task> func) => _func = func ?? throw new ArgumentNullException(nameof(func));
		public object? Invoke(ICommandContext? context, object?[] args) => _func(context!, (T1)args[0]!, (T2)args[1]!, (T3)args[2]!, (T4)args[3]!);
	}

	internal sealed class InteractiveFuncInvoker<T1, T2, T3, T4, T5> : ICommandInvoker
	{
		private readonly Func<ICommandContext, T1, T2, T3, T4, T5, Task> _func;
		public InteractiveFuncInvoker(Func<ICommandContext, T1, T2, T3, T4, T5, Task> func) => _func = func ?? throw new ArgumentNullException(nameof(func));
		public object? Invoke(ICommandContext? context, object?[] args) => _func(context!, (T1)args[0]!, (T2)args[1]!, (T3)args[2]!, (T4)args[3]!, (T5)args[4]!);
	}
}