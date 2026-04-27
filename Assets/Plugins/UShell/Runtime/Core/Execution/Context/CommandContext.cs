#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using UShell.Runtime.Core.Output;

namespace UShell.Runtime.Core.Execution.Context
{
	internal sealed class CommandContext : ICommandContext
	{
		private readonly IInteractiveSession _session;
		private readonly IConsolePrinter _printer;

		public CancellationToken Token { get; }

		public CommandContext(IInteractiveSession session, IConsolePrinter printer, CancellationToken token)
		{
			_session = session ?? throw new ArgumentNullException(nameof(session));
			_printer = printer ?? throw new ArgumentNullException(nameof(printer));
			Token = token;
		}

		public async Task<bool> ConfirmAsync(string message)
		{
			string fullMessage = $"{message} (y/n)";

			while (true)
			{
				Token.ThrowIfCancellationRequested();

				string response = await _session.RequestPromptAsync(fullMessage);
				string lower = response.Trim().ToLowerInvariant();

				if (lower == "y" || lower == "yes") return true;
				if (lower == "n" || lower == "no") return false;
			}
		}

		public Task<string> PromptAsync(string message)
		{
			return _session.RequestPromptAsync(message);
		}

		public IProgressReporter CreateProgressBar(string taskName)
		{
			return new ProgressReporter(taskName, _printer);
		}

		public void Print(string message) => _printer.Print(new LogEntry(message, LogType.Standard));
		public void PrintSuccess(string message) => _printer.Print(new LogEntry(message, LogType.Success));
		public void PrintWarning(string message) => _printer.Print(new LogEntry(message, LogType.Warning));
		public void PrintError(string message) => _printer.Print(new LogEntry(message, LogType.Error));
	}
}