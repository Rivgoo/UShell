using System.Threading;
using System.Threading.Tasks;

namespace UShell.Runtime.Core.Execution.Context
{
	/// <summary>
	/// Provides runtime capabilities and interaction handles to an executing interactive command.
	/// </summary>
	/// <remarks>
	/// Injected automatically into commands registered via <c>ExecutesInteractiveAsync</c>. 
	/// It allows a command to halt and request user input, spawn progress bars, and print live logs 
	/// without breaking the execution flow.
	/// </remarks>
	public interface ICommandContext
	{
		/// <summary>
		/// A cancellation token triggered if the command exceeds its configured timeout, 
		/// or if the user manually aborts the execution (e.g., by pressing Escape).
		/// </summary>
		CancellationToken Token { get; }

		/// <summary>
		/// Suspends the command and prompts the user in the console with a Yes/No question.
		/// </summary>
		/// <param name="message">The question to ask the user.</param>
		/// <returns>A task resolving to <c>true</c> if the user answers yes, or <c>false</c> if no.</returns>
		/// <exception cref="System.OperationCanceledException">Thrown if the session times out or is aborted.</exception>
		Task<bool> ConfirmAsync(string message);

		/// <summary>
		/// Suspends the command and waits for the user to type a string response.
		/// </summary>
		/// <param name="message">The prompt message displayed before the input caret.</param>
		/// <returns>A task resolving to the string typed by the user.</returns>
		/// <exception cref="System.OperationCanceledException">Thrown if the session times out or is aborted.</exception>
		Task<string> PromptAsync(string message);

		/// <summary>
		/// Spawns a new visual progress bar in the console log stream.
		/// </summary>
		/// <remarks>
		/// The progress bar stays active until the returned reporter is disposed. 
		/// Disposing the reporter automatically marks the task as 100% completed.
		/// </remarks>
		/// <param name="taskName">The label displayed next to the progress bar.</param>
		/// <returns>A disposable reporter object used to update the progress visually.</returns>
		IProgressReporter CreateProgressBar(string taskName);

		/// <summary>
		/// Prints a standard text message directly to the console during command execution.
		/// </summary>
		void Print(string message);

		/// <summary>
		/// Prints a success message (usually colored green) directly to the console.
		/// </summary>
		void PrintSuccess(string message);

		/// <summary>
		/// Prints a warning message (usually colored yellow/amber) directly to the console.
		/// </summary>
		void PrintWarning(string message);

		/// <summary>
		/// Prints an error message (usually colored red) directly to the console.
		/// </summary>
		void PrintError(string message);
	}
}