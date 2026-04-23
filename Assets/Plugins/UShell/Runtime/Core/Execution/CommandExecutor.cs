using System;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands;
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

		public ExecutionResult Execute(string input)
		{
			var parsed = Parser.Parse(input);
			if (!parsed.IsSuccess)
				return ExecutionResult.Failure($"Syntax Error: {parsed.ErrorMessage}", parsed.ErrorPosition);

			CommandNode node = parsed.Value;

			if (!_registry.TryGetCommand(node.CommandName, out CommandSignature sig))
				return ExecutionResult.Failure(
					$"Unknown command '{node.CommandName}'. Type 'help' to list commands.", node.StartIndex);

			var bound = _binder.BindArguments(sig.Parameters, node.Arguments);
			if (!bound.IsSuccess)
				return ExecutionResult.Failure($"Argument Error: {bound.ErrorMessage}", bound.ErrorPosition);

			return Invoke(sig, bound.Value, node.StartIndex);
		}

		private static ExecutionResult Invoke(CommandSignature sig, object?[] args, int startIndex)
		{
			try
			{
				sig.Invoker.Invoke(args);
				return ExecutionResult.Success();
			}
			catch (Exception ex)
			{
				Exception inner = ex.InnerException ?? ex;
				return ExecutionResult.Failure($"Execution Error: {inner.Message}", startIndex);
			}
		}
	}
}