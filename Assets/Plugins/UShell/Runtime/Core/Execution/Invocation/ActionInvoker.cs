#nullable enable
using System;

namespace UShell.Runtime.Core.Execution.Invocation
{
	internal sealed class ActionInvoker : ICommandInvoker
	{
		private readonly Action _action;

		public ActionInvoker(Action action)
		{
			_action = action ?? throw new ArgumentNullException(nameof(action));
		}

		public void Invoke(object?[] args)
		{
			_action();
		}
	}

	internal sealed class ActionInvoker<T1> : ICommandInvoker
	{
		private readonly Action<T1> _action;

		public ActionInvoker(Action<T1> action)
		{
			_action = action ?? throw new ArgumentNullException(nameof(action));
		}

		public void Invoke(object?[] args)
		{
			_action((T1)args[0]!);
		}
	}

	internal sealed class ActionInvoker<T1, T2> : ICommandInvoker
	{
		private readonly Action<T1, T2> _action;

		public ActionInvoker(Action<T1, T2> action)
		{
			_action = action ?? throw new ArgumentNullException(nameof(action));
		}

		public void Invoke(object?[] args)
		{
			_action((T1)args[0]!, (T2)args[1]!);
		}
	}

	internal sealed class ActionInvoker<T1, T2, T3> : ICommandInvoker
	{
		private readonly Action<T1, T2, T3> _action;

		public ActionInvoker(Action<T1, T2, T3> action)
		{
			_action = action ?? throw new ArgumentNullException(nameof(action));
		}

		public void Invoke(object?[] args)
		{
			_action((T1)args[0]!, (T2)args[1]!, (T3)args[2]!);
		}
	}

	internal sealed class ActionInvoker<T1, T2, T3, T4> : ICommandInvoker
	{
		private readonly Action<T1, T2, T3, T4> _action;

		public ActionInvoker(Action<T1, T2, T3, T4> action)
		{
			_action = action ?? throw new ArgumentNullException(nameof(action));
		}

		public void Invoke(object?[] args)
		{
			_action((T1)args[0]!, (T2)args[1]!, (T3)args[2]!, (T4)args[3]!);
		}
	}

	internal sealed class ActionInvoker<T1, T2, T3, T4, T5> : ICommandInvoker
	{
		private readonly Action<T1, T2, T3, T4, T5> _action;

		public ActionInvoker(Action<T1, T2, T3, T4, T5> action)
		{
			_action = action ?? throw new ArgumentNullException(nameof(action));
		}

		public void Invoke(object?[] args)
		{
			_action((T1)args[0]!, (T2)args[1]!, (T3)args[2]!, (T4)args[3]!, (T5)args[4]!);
		}
	}
}