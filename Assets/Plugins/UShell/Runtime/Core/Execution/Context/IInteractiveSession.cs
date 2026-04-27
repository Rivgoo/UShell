using System;
using System.Threading;
using System.Threading.Tasks;

namespace UShell.Runtime.Core.Execution.Context
{
	public interface IInteractiveSession
	{
		bool IsBusy { get; }
		bool IsWaitingForPrompt { get; }

		event Action<string> OnPromptRequested;
		event Action OnStateChanged;

		CancellationToken StartSession(TimeSpan timeout);
		void EndSession();
		Task<string> RequestPromptAsync(string message);
		void SubmitInput(string input);
		void Cancel();
	}
}