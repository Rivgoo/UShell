using System.Threading;
using System.Threading.Tasks;

namespace UShell.Runtime.Core.Execution.Context
{
	public interface ICommandContext
	{
		CancellationToken Token { get; }

		Task<bool> ConfirmAsync(string message);
		Task<string> PromptAsync(string message);
		IProgressReporter CreateProgressBar(string taskName);

		void Print(string message);
		void PrintSuccess(string message);
		void PrintWarning(string message);
		void PrintError(string message);
	}
}