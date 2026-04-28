#nullable enable
using System;

namespace UShell.Runtime.Core.Execution
{
	/// <summary>
	/// The primary entry point for executing raw text input as a shell command.
	/// </summary>
	/// <remarks>
	/// Responsible for orchestrating the parsing (AST generation), argument binding, 
	/// variable expansion, and finally, the invocation of the command delegate.
	/// </remarks>
	public interface ICommandExecutor
	{
		/// <summary>
		/// Fired immediately before a raw command string begins the parsing and execution pipeline.
		/// </summary>
		event Action<string> OnExecuting;

		/// <summary>
		/// Fired immediately after a command has completed execution, yielding the final result.
		/// </summary>
		event Action<string, ExecutionResult<object?>> OnExecuted;

		/// <summary>
		/// Parses and executes the provided shell expression.
		/// </summary>
		/// <param name="input">The raw string input from the user (e.g., <c>spawn -count 5 "enemy"</c>).</param>
		/// <returns>
		/// An <see cref="ExecutionResult{T}"/> containing the returned object from the command if successful, 
		/// or a <see cref="UShell.Runtime.Core.Diagnostics.ShellError"/> if parsing, binding, or execution failed.
		/// </returns>
		ExecutionResult<object?> Execute(string input);
	}
}