using System;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Core.Output.Formatting;

namespace UShell.Runtime.Core.Execution.Context
{
	internal sealed class ProgressReporter : IProgressReporter
	{
		private readonly string _taskName;
		private readonly IConsolePrinter _printer;
		private readonly Guid _id;

		private bool _isCompleted;

		public ProgressReporter(string taskName, IConsolePrinter printer)
		{
			_taskName = string.IsNullOrWhiteSpace(taskName) ? "Task" : taskName;
			_printer = printer ?? throw new ArgumentNullException(nameof(printer));
			_id = Guid.NewGuid();

			_printer.Print(new LogEntry(GenerateBar(0f, "Initializing..."), LogType.Standard, _id));
		}

		public void Report(float progress, string status = "")
		{
			if (_isCompleted) return;
			_printer.UpdatePrint(_id, new LogEntry(GenerateBar(progress, status), LogType.Standard, _id));
		}

		public void Fail(string reason)
		{
			if (_isCompleted) return;
			_isCompleted = true;
			_printer.UpdatePrint(_id, new LogEntry($"{_taskName}: {reason}", LogType.Error, _id));
		}

		public void Dispose()
		{
			if (_isCompleted) return;
			_isCompleted = true;
			_printer.UpdatePrint(_id, new LogEntry(GenerateBar(1f, "Completed"), LogType.Success, _id));
		}

		private string GenerateBar(float progress, string status)
		{
			const int totalChars = 20;
			float normalizedProgress = Math.Clamp(progress, 0f, 1f);
			int filledCount = (int)Math.Round(normalizedProgress * totalChars);
			int emptyCount = totalChars - filledCount;

			string filledPart = new string('=', filledCount);
			string emptyPart = new string(' ', emptyCount);
			string combinedBar = $"[{filledPart}{emptyPart}]";

			string coloredName = RichText.Color(_taskName.PadRight(15), ShellPalette.SyntaxCommand);
			string coloredBar = RichText.Color(combinedBar, ShellPalette.AccentMuted);
			string percentage = $"{normalizedProgress * 100f,3:F0}%";

			return $"{coloredName} {coloredBar} {percentage} - {status}";
		}
	}
}