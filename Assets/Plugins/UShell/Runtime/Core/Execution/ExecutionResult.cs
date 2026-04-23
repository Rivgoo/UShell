using System;

namespace UShell.Runtime.Core.Execution
{
	public readonly struct ExecutionResult
	{
		public bool IsSuccess { get; }
		public string ErrorMessage { get; }
		public int ErrorPosition { get; }

		private ExecutionResult(bool isSuccess, string errorMessage, int errorPosition)
		{
			IsSuccess = isSuccess;
			ErrorMessage = errorMessage;
			ErrorPosition = errorPosition;
		}

		public static ExecutionResult Success() =>
			new ExecutionResult(true, string.Empty, -1);

		public static ExecutionResult Failure(string message, int errorPosition = -1)
		{
			if (string.IsNullOrWhiteSpace(message))
				throw new ArgumentException("Failure message cannot be null or empty.", nameof(message));
			return new ExecutionResult(false, message, errorPosition);
		}
	}

	public readonly struct ExecutionResult<T>
	{
		public bool IsSuccess { get; }
		public string ErrorMessage { get; }
		public int ErrorPosition { get; }
		private readonly T _value;

		public T Value
		{
			get
			{
				if (!IsSuccess)
					throw new InvalidOperationException("Cannot access value of a failed result.");
				return _value;
			}
		}

		private ExecutionResult(bool isSuccess, T value, string errorMessage, int errorPosition)
		{
			IsSuccess = isSuccess;
			_value = value;
			ErrorMessage = errorMessage;
			ErrorPosition = errorPosition;
		}

		public static ExecutionResult<T> Success(T value) =>
			new ExecutionResult<T>(true, value, string.Empty, -1);

		public static ExecutionResult<T> Failure(string message, int errorPosition = -1)
		{
			if (string.IsNullOrWhiteSpace(message))
				throw new ArgumentException("Failure message cannot be null or empty.", nameof(message));
			return new ExecutionResult<T>(false, default!, message, errorPosition);
		}
	}
}