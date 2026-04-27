using System;

namespace UShell.Runtime.Core.Execution.Context
{
	/// <summary>
	/// A disposable handle used to visualize background progress in the shell.
	/// </summary>
	/// <remarks>
	/// Always wrap instances of <see cref="IProgressReporter"/> in a <c>using</c> block. 
	/// Disposing the reporter automatically marks the progress bar as 100% completed and styles it as a success.
	/// </remarks>
	public interface IProgressReporter : IDisposable
	{
		/// <summary>
		/// Updates the visual progress bar and text status in the console.
		/// </summary>
		/// <param name="progress">A normalized float between 0.0 and 1.0 representing completion percentage.</param>
		/// <param name="status">Optional brief text appended to the progress bar (e.g., "Downloading...").</param>
		void Report(float progress, string status = "");

		/// <summary>
		/// Marks the underlying task as failed, turning the progress bar red and halting further updates.
		/// </summary>
		/// <param name="reason">The explanation for the failure.</param>
		void Fail(string reason);
	}
}