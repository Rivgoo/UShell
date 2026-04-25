using System;
using UShell.Runtime.Core.Diagnostics;

namespace UShell.Runtime.Core.Execution
{
	public readonly struct ExecutionResult
	{
		public ShellError? Error { get; }
		public bool IsSuccess => !Error.HasValue;

		private ExecutionResult(ShellError? error)
		{
			Error = error;
		}

		public static ExecutionResult Success() => new ExecutionResult(null);

		public static ExecutionResult Failure(ShellError error) => new ExecutionResult(error);
	}

	public readonly struct ExecutionResult<T>
	{
		public ShellError? Error { get; }
		public bool IsSuccess => !Error.HasValue;

		private readonly T _value;

		public T Value
		{
			get
			{
				if (!IsSuccess)
				{
					throw new InvalidOperationException("Cannot access value of a failed result.");
				}
				return _value;
			}
		}

		private ExecutionResult(T value, ShellError? error)
		{
			_value = value;
			Error = error;
		}

		public static ExecutionResult<T> Success(T value) => new ExecutionResult<T>(value, null);

		public static ExecutionResult<T> Failure(ShellError error) => new ExecutionResult<T>(default!, error);
	}
}