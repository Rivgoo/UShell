using System;
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

		public ExecutionResult Execute(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return ExecutionResult.Success();

			ExecutionResult result = _inner.Execute(input);

			if (!result.IsSuccess)
				PrintFailure(input, result);

			return result;
		}

		private void PrintFailure(string input, ExecutionResult result)
		{
			string msg = result.ErrorPosition >= 0
				? ErrorVisualizer.GenerateErrorPointer(input, result.ErrorPosition, result.ErrorMessage)
				: result.ErrorMessage;

			_printer.Print(new LogEntry(msg, LogType.Error));
		}
	}
}