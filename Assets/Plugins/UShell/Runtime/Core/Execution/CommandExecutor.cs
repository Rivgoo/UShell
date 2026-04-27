#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution.Binding;
using UShell.Runtime.Core.Execution.Context;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Core.Parsing;
using UShell.Runtime.Core.Parsing.Syntax;

namespace UShell.Runtime.Core.Execution
{
	public sealed class CommandExecutor : ICommandExecutor
	{
		private readonly ICommandRegistry _registry;
		private readonly IArgumentBinder _binder;
		private readonly IInteractiveSession _session;
		private readonly IConsolePrinter _printer;

		internal CommandExecutor(
			ICommandRegistry registry,
			IArgumentBinder binder,
			IInteractiveSession session,
			IConsolePrinter printer)
		{
			_registry = registry ?? throw new ArgumentNullException(nameof(registry));
			_binder = binder ?? throw new ArgumentNullException(nameof(binder));
			_session = session ?? throw new ArgumentNullException(nameof(session));
			_printer = printer ?? throw new ArgumentNullException(nameof(printer));
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

		private ExecutionResult<object?> Invoke(CommandSignature sig, object?[] args, int startIndex)
		{
			ICommandContext? context = null;

			if (sig.IsInteractive)
			{
				CancellationToken token = _session.StartSession(sig.Timeout!.Value);
				context = new CommandContext(_session, _printer, token);
			}

			try
			{
				object? result = sig.Invoker.Invoke(context, args);

				if (sig.IsInteractive && result is Task interactiveTask)
				{
					result = WrapInteractiveTaskAsync(interactiveTask, _session);
				}

				return ExecutionResult<object?>.Success(result);
			}
			catch (Exception ex)
			{
				if (sig.IsInteractive) _session.EndSession();

				Exception inner = ex.InnerException ?? ex;
				return ExecutionResult<object?>.Failure(
					ShellError.Create(ShellErrorCode.Execute_Exception, startIndex, inner.Message));
			}
		}

		private static async Task WrapInteractiveTaskAsync(Task task, IInteractiveSession session)
		{
			try
			{
				await task;
			}
			finally
			{
				session.EndSession();
			}
		}
	}
}