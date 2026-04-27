using System;
using System.Threading;
using System.Threading.Tasks;

namespace UShell.Runtime.Core.Execution.Context
{
	/// <summary>
	/// Manages the lifecycle and state of the active interactive command session.
	/// </summary>
	/// <remarks>
	/// When an interactive command runs, it locks the shell interface. The session orchestrates 
	/// timeouts, UI state changes, and routing user input back to awaiting asynchronous prompts.
	/// </remarks>
	public interface IInteractiveSession
	{
		/// <summary>
		/// Indicates whether the shell is currently locked by a running interactive command.
		/// </summary>
		bool IsBusy { get; }

		/// <summary>
		/// Indicates whether the active interactive command has suspended execution and is actively waiting for user input.
		/// </summary>
		bool IsWaitingForPrompt { get; }

		/// <summary>
		/// Fired when an interactive command requests input, broadcasting the prompt message.
		/// </summary>
		event Action<string> OnPromptRequested;

		/// <summary>
		/// Fired when the session transitions between idle, busy, or prompting states.
		/// </summary>
		event Action OnStateChanged;

		/// <summary>
		/// Begins a new interactive session, locking the shell and starting the timeout countdown.
		/// </summary>
		/// <param name="timeout">The maximum allowed duration before the session is force-cancelled.</param>
		/// <returns>A cancellation token tied to the session timeout and user abort actions.</returns>
		/// <exception cref="InvalidOperationException">Thrown if another session is already active.</exception>
		CancellationToken StartSession(TimeSpan timeout);

		/// <summary>
		/// Terminates the active session, freeing the shell and cancelling any pending prompts.
		/// </summary>
		void EndSession();

		/// <summary>
		/// Halts the session and issues a request for string input from the user.
		/// </summary>
		/// <param name="message">The text to display to the user.</param>
		/// <returns>A task that completes when the user submits their input.</returns>
		Task<string> RequestPromptAsync(string message);

		/// <summary>
		/// Fulfills a pending prompt request with the provided user input.
		/// </summary>
		/// <param name="input">The raw text entered by the user.</param>
		void SubmitInput(string input);

		/// <summary>
		/// Manually triggers a cancellation of the active session (e.g., when the user presses Escape).
		/// </summary>
		void Cancel();
	}
}