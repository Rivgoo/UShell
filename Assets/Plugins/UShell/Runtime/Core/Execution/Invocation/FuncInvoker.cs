#nullable enable
using System;
using UShell.Runtime.Core.Execution.Context;

namespace UShell.Runtime.Core.Execution.Invocation
{
	internal sealed class FuncInvoker<TResult> : ICommandInvoker
	{
		private readonly Func<TResult> _func;
		public FuncInvoker(Func<TResult> func) => _func = func ?? throw new ArgumentNullException(nameof(func));
		public object? Invoke(ICommandContext? context, object?[] args) => _func();
	}

	internal sealed class FuncInvoker<T1, TResult> : ICommandInvoker
	{
		private readonly Func<T1, TResult> _func;
		public FuncInvoker(Func<T1, TResult> func) => _func = func ?? throw new ArgumentNullException(nameof(func));
		public object? Invoke(ICommandContext? context, object?[] args) => _func((T1)args[0]!);
	}

	internal sealed class FuncInvoker<T1, T2, TResult> : ICommandInvoker
	{
		private readonly Func<T1, T2, TResult> _func;
		public FuncInvoker(Func<T1, T2, TResult> func) => _func = func ?? throw new ArgumentNullException(nameof(func));
		public object? Invoke(ICommandContext? context, object?[] args) => _func((T1)args[0]!, (T2)args[1]!);
	}

	internal sealed class FuncInvoker<T1, T2, T3, TResult> : ICommandInvoker
	{
		private readonly Func<T1, T2, T3, TResult> _func;
		public FuncInvoker(Func<T1, T2, T3, TResult> func) => _func = func ?? throw new ArgumentNullException(nameof(func));
		public object? Invoke(ICommandContext? context, object?[] args) => _func((T1)args[0]!, (T2)args[1]!, (T3)args[2]!);
	}

	internal sealed class FuncInvoker<T1, T2, T3, T4, TResult> : ICommandInvoker
	{
		private readonly Func<T1, T2, T3, T4, TResult> _func;
		public FuncInvoker(Func<T1, T2, T3, T4, TResult> func) => _func = func ?? throw new ArgumentNullException(nameof(func));
		public object? Invoke(ICommandContext? context, object?[] args) => _func((T1)args[0]!, (T2)args[1]!, (T3)args[2]!, (T4)args[3]!);
	}

	internal sealed class FuncInvoker<T1, T2, T3, T4, T5, TResult> : ICommandInvoker
	{
		private readonly Func<T1, T2, T3, T4, T5, TResult> _func;
		public FuncInvoker(Func<T1, T2, T3, T4, T5, TResult> func) => _func = func ?? throw new ArgumentNullException(nameof(func));
		public object? Invoke(ICommandContext? context, object?[] args) => _func((T1)args[0]!, (T2)args[1]!, (T3)args[2]!, (T4)args[3]!, (T5)args[4]!);
	}
}