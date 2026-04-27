#nullable enable
using System;
using System.Threading.Tasks;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Output.Formatting;

namespace UShell.Runtime.Core.Output.Reporting
{
	public sealed class ReportingCommandExecutor : ICommandExecutor
	{
		private readonly ICommandExecutor _inner;
		private readonly IConsolePrinter _printer;

		public ReportingCommandExecutor(ICommandExecutor inner, IConsolePrinter printer)
		{
			_inner = inner ?? throw new ArgumentNullException(nameof(inner));
			_printer = printer ?? throw new ArgumentNullException(nameof(printer));
		}

		public ExecutionResult<object?> Execute(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				return ExecutionResult<object?>.Success(null);
			}

			ExecutionResult<object?> result = _inner.Execute(input);

			if (!result.IsSuccess)
			{
				PrintFailure(input, result.Error!.Value);
			}
			else if (result.Value != null)
			{
				HandleSuccessOutput(result.Value);
			}

			return result;
		}

		private void HandleSuccessOutput(object value)
		{
			if (value is Task task)
			{
				_ = AwaitAndReportTask(task);
			}
			else
			{
				_printer.Print(new LogEntry(value.ToString(), LogType.Success));
			}
		}

		private async Task AwaitAndReportTask(Task task)
		{
			try
			{
				await task;
			}
			catch (OperationCanceledException)
			{
				string msg = RichText.Color("Command execution was aborted (Timeout or User Cancellation).", ShellPalette.ErrorMuted);
				_printer.Print(new LogEntry(msg, LogType.Error));
			}
			catch (Exception ex)
			{
				_printer.Print(new LogEntry($"Async command failed: {ex.Message}", LogType.Error));
			}
		}

		private void PrintFailure(string input, ShellError error)
		{
			string msg = error.Position >= 0
				? ErrorVisualizer.GenerateErrorPointer(input, error.Position, error.Message)
				: error.Message;

			_printer.Print(new LogEntry(msg, LogType.Error));
		}
	}
}