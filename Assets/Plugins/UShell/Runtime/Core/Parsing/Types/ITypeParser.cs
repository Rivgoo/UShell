using System;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core.Parsing.Types
{
	public interface ITypeParser
	{
		Type TargetType { get; }
		ExecutionResult<object> Parse(string input);
	}

	public interface ITypeParser<T> : ITypeParser
	{
		ExecutionResult<T> ParseTyped(string input);
	}
}