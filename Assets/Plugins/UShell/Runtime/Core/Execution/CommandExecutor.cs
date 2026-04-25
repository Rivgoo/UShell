#nullable enable
using System;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution.Binding;
using UShell.Runtime.Core.Parsing;
using UShell.Runtime.Core.Parsing.Syntax;

namespace UShell.Runtime.Core.Execution
{
	public sealed class CommandExecutor : ICommandExecutor
	{
		private readonly ICommandRegistry _registry;
		private readonly IArgumentBinder _binder;

		public CommandExecutor(ICommandRegistry registry, IArgumentBinder binder)
		{
			_registry = registry ?? throw new ArgumentNullException(nameof(registry));
			_binder = binder ?? throw new ArgumentNullException(nameof(binder));
		}

		public ExecutionResult<object?> Execute(string input)
		{
			var parsed = Parser.Parse(input);
			if (!parsed.IsSuccess)
			{
				return ExecutionResult<object?>.Failure(parsed.Error!.Value);
			}

			CommandNode node = parsed.Value;

			if (!_registry.TryGetCommand(node.CommandName, out CommandSignature sig))
			{
				return ExecutionResult<object?>.Failure(
					ShellError.Create(ShellErrorCode.Execute_CommandNotFound, node.StartIndex, node.CommandName));
			}

			var bound = _binder.BindArguments(sig.Parameters, node.Arguments);
			if (!bound.IsSuccess)
			{
				return ExecutionResult<object?>.Failure(bound.Error!.Value);
			}

			return Invoke(sig, bound.Value, node.StartIndex);
		}

		private static ExecutionResult<object?> Invoke(CommandSignature sig, object?[] args, int startIndex)
		{
			try
			{
				object? result = sig.Invoker.Invoke(args);
				return ExecutionResult<object?>.Success(result);
			}
			catch (Exception ex)
			{
				Exception inner = ex.InnerException ?? ex;
				return ExecutionResult<object?>.Failure(
					ShellError.Create(ShellErrorCode.Execute_Exception, startIndex, inner.Message));
			}
		}
	}
}