using System;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core.Parsing.Types
{
	public abstract class TypeParser<T> : ITypeParser<T>
	{
		public Type TargetType => typeof(T);

		public ExecutionResult<object> Parse(string input)
		{
			ExecutionResult<T> result = ParseTyped(input);

			if (result.IsSuccess)
			{
				return ExecutionResult<object>.Success(result.Value!);
			}

			return ExecutionResult<object>.Failure(result.ErrorMessage);
		}

		public abstract ExecutionResult<T> ParseTyped(string input);
	}
}
