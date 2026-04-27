#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Core.Output.Formatting;

namespace UShell.Runtime.Core.Execution.Context
{
	/// <summary>
	/// The default implementation of <see cref="IInteractiveSession"/> that handles 
	/// timeout tokens and dispatches prompt visuals to a <see cref="IConsolePrinter"/>.
	/// </summary>
	public sealed class InteractiveSession : IInteractiveSession
	{
		/// <inheritdoc/>
		public event Action<string> OnPromptRequested = delegate { };

		/// <inheritdoc/>
		public event Action OnStateChanged = delegate { };

		private readonly IConsolePrinter _printer;
		private CancellationTokenSource? _cancellationTokenSource;
		private TaskCompletionSource<string>? _promptCompletionSource;

		/// <inheritdoc/>
		public bool IsBusy { get; private set; }

		/// <inheritdoc/>
		public bool IsWaitingForPrompt => _promptCompletionSource != null && !_promptCompletionSource.Task.IsCompleted;

		/// <summary>
		/// Initializes a new instance of the session binding it to a specific printer.
		/// </summary>
		public InteractiveSession(IConsolePrinter printer)
		{
			_printer = printer ?? throw new ArgumentNullException(nameof(printer));
		}

		/// <inheritdoc/>
		public CancellationToken StartSession(TimeSpan timeout)
		{
			if (IsBusy)
			{
				throw new InvalidOperationException("Session is already busy computing another interactive command.");
			}

			IsBusy = true;
			_cancellationTokenSource = new CancellationTokenSource(timeout);

			NotifyStateChanged();
			return _cancellationTokenSource.Token;
		}

		/// <inheritdoc/>
		public void EndSession()
		{
			if (!IsBusy) return;

			IsBusy = false;
			_cancellationTokenSource?.Dispose();
			_cancellationTokenSource = null;

			_promptCompletionSource?.TrySetCanceled();
			_promptCompletionSource = null;

			NotifyStateChanged();
		}

		/// <inheritdoc/>
		public void Cancel()
		{
			_cancellationTokenSource?.Cancel();
			_promptCompletionSource?.TrySetCanceled();
		}

		/// <inheritdoc/>
		public void SubmitInput(string input)
		{
			_promptCompletionSource?.TrySetResult(input ?? string.Empty);
		}

		/// <inheritdoc/>
		public Task<string> RequestPromptAsync(string message)
		{
			if (_cancellationTokenSource == null)
			{
				throw new InvalidOperationException("Cannot request prompt outside of an active interactive session.");
			}

			_promptCompletionSource = new TaskCompletionSource<string>();

			string promptPrefix = RichText.Color("?", ShellPalette.WarningBright);
			_printer.Print(new LogEntry($"{promptPrefix} {message}", LogType.Standard));

			OnPromptRequested.Invoke(message);
			NotifyStateChanged();

			CancellationTokenRegistration registration = _cancellationTokenSource.Token.Register(() =>
			{
				_promptCompletionSource.TrySetCanceled();
			});

			return _promptCompletionSource.Task.ContinueWith(task =>
			{
				registration.Dispose();
				_promptCompletionSource = null;
				NotifyStateChanged();
				return task.Result;
			}, TaskContinuationOptions.ExecuteSynchronously);
		}

		private void NotifyStateChanged()
		{
			OnStateChanged.Invoke();
		}
	}
}