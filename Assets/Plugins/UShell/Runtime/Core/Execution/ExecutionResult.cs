using System;
using UShell.Runtime.Core.Diagnostics;

namespace UShell.Runtime.Core.Execution
{
	/// <summary>
	/// Represents the outcome of an operation (parsing, binding, or execution) without relying on thrown exceptions.
	/// </summary>
	public readonly struct ExecutionResult
	{
		/// <summary>
		/// The underlying shell error payload if the operation failed; otherwise, null.
		/// </summary>
		public ShellError? Error { get; }

		/// <summary>
		/// Returns <c>true</c> if the operation succeeded and <see cref="Error"/> is null.
		/// </summary>
		public bool IsSuccess => !Error.HasValue;

		private ExecutionResult(ShellError? error)
		{
			Error = error;
		}

		/// <summary>
		/// Generates a successful result.
		/// </summary>
		public static ExecutionResult Success() => new ExecutionResult(null);

		/// <summary>
		/// Generates a failed result containing the specified error payload.
		/// </summary>
		public static ExecutionResult Failure(ShellError error) => new ExecutionResult(error);
	}

	/// <summary>
	/// Represents the outcome of an operation that yields a specific value upon success.
	/// </summary>
	/// <typeparam name="T">The type of the expected result payload.</typeparam>
	public readonly struct ExecutionResult<T>
	{
		/// <summary>
		/// The underlying shell error payload if the operation failed; otherwise, null.
		/// </summary>
		public ShellError? Error { get; }

		/// <summary>
		/// Returns <c>true</c> if the operation succeeded and a <see cref="Value"/> is available.
		/// </summary>
		public bool IsSuccess => !Error.HasValue;

		private readonly T _value;

		/// <summary>
		/// Gets the successful payload of the operation.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if accessed when <see cref="IsSuccess"/> is <c>false</c>.</exception>
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

		/// <summary>
		/// Generates a successful result encapsulating the provided value.
		/// </summary>
		public static ExecutionResult<T> Success(T value) => new ExecutionResult<T>(value, null);

		/// <summary>
		/// Generates a failed result containing the specified error payload.
		/// </summary>
		public static ExecutionResult<T> Failure(ShellError error) => new ExecutionResult<T>(default!, error);
	}
}