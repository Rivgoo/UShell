#nullable enable
using System;
using System.Globalization;
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
		public event Action<string> OnExecuting = delegate { };
		public event Action<string, ExecutionResult<object?>> OnExecuted = delegate { };

		private readonly ICommandRegistry _registry;
		private readonly IArgumentBinder _binder;
		private readonly IInteractiveSession _session;
		private readonly IConsolePrinter _printer;
		private readonly ISessionState _sessionState;

		internal CommandExecutor(
			ICommandRegistry registry,
			IArgumentBinder binder,
			IInteractiveSession session,
			IConsolePrinter printer,
			ISessionState sessionState)
		{
			_registry = registry ?? throw new ArgumentNullException(nameof(registry));
			_binder = binder ?? throw new ArgumentNullException(nameof(binder));
			_session = session ?? throw new ArgumentNullException(nameof(session));
			_printer = printer ?? throw new ArgumentNullException(nameof(printer));
			_sessionState = sessionState ?? throw new ArgumentNullException(nameof(sessionState));
		}

		public ExecutionResult<object?> Execute(string input)
		{
			OnExecuting.Invoke(input);
			ExecutionResult<object?> result = ExecuteInternal(input, 0);
			OnExecuted.Invoke(input, result);
			return result;
		}

		private ExecutionResult<object?> ExecuteInternal(string input, int depth)
		{
			if (depth > 20)
			{
				return ExecutionResult<object?>.Failure(
					ShellError.Create(ShellErrorCode.Execute_Exception, 0, "Macro expansion recursion depth exceeded."));
			}

			var parsed = Parser.Parse(input);
			if (!parsed.IsSuccess)
			{
				return ExecutionResult<object?>.Failure(parsed.Error!.Value);
			}

			return ExecuteNode(parsed.Value, depth);
		}

		private ExecutionResult<object?> ExecuteNode(SyntaxNode node, int depth)
		{
			if (node is AssignmentNode assignNode)
			{
				ExecutionResult<object?> result;

				if (assignNode.RightSide is CommandNode cmdNode)
				{
					result = ExecuteCommand(cmdNode);
				}
				else
				{
					result = ResolveSimpleNode(assignNode.RightSide);
				}

				if (result.IsSuccess)
				{
					_sessionState.SetValue(assignNode.VariableName, result.Value);
				}

				return result;
			}

			if (node is CommandNode command)
			{
				return ExecuteCommand(command);
			}

			if (node is VariableNode varNode)
			{
				if (_sessionState.TryGetValue(varNode.Name, out object? val))
				{
					if (val is string strVal)
					{
						return ExecuteInternal(strVal, depth + 1);
					}

					return ExecutionResult<object?>.Success(val);
				}

				return ExecutionResult<object?>.Failure(
					ShellError.Create(ShellErrorCode.Execute_MacroOrSessionVariableNotFound, node.StartIndex, varNode.Name));
			}

			return ResolveSimpleNode(node);
		}

		private ExecutionResult<object?> ExecuteCommand(CommandNode node)
		{
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

		private ExecutionResult<object?> ResolveSimpleNode(SyntaxNode node)
		{
			if (node is LiteralNode literal)
			{
				string val = literal.Value;
				if (int.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out int i)) return ExecutionResult<object?>.Success(i);
				if (float.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) return ExecutionResult<object?>.Success(f);
				if (bool.TryParse(val, out bool b)) return ExecutionResult<object?>.Success(b);
				return ExecutionResult<object?>.Success(val);
			}

			if (node is StringNode str) return ExecutionResult<object?>.Success(str.Value);

			if (node is VariableNode varNode)
			{
				if (_sessionState.TryGetValue(varNode.Name, out object? val))
				{
					return ExecutionResult<object?>.Success(val);
				}
				return ExecutionResult<object?>.Failure(
					ShellError.Create(ShellErrorCode.Execute_MacroOrSessionVariableNotFound, node.StartIndex, varNode.Name));
			}

			if (node is ArrayNode arrayNode)
			{
				var elements = new object?[arrayNode.Elements.Count];
				for (int i = 0; i < arrayNode.Elements.Count; i++)
				{
					var res = ResolveSimpleNode(arrayNode.Elements[i]);
					if (!res.IsSuccess) return res;
					elements[i] = res.Value;
				}
				return ExecutionResult<object?>.Success(elements);
			}

			return ExecutionResult<object?>.Failure(
				ShellError.Create(ShellErrorCode.Execute_Exception, node.StartIndex, "Cannot resolve this expression as a value."));
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