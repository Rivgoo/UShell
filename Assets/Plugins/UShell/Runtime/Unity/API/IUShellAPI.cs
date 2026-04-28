#nullable enable
using System;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Output;

namespace UShell.Runtime.Unity.API
{
	/// <summary>
	/// Provides a centralized, read-only state and event aggregator for interacting with the UShell console programmatically.
	/// </summary>
	/// <remarks>
	/// Use this interface to react to console visibility changes (e.g., disabling player input), 
	/// trace executed commands, or force the console to process logic from C# scripts.
	/// </remarks>
	public interface IUShellAPI
	{
		/// <summary>Returns <c>true</c> if the console UI is currently active and visible to the user.</summary>
		bool IsVisible { get; }

		/// <summary>Returns <c>true</c> if a command is currently locking the shell via an asynchronous interactive session.</summary>
		bool IsExecutingInteractiveCommand { get; }

		/// <summary>Gets the exact raw text currently typed into the console input field.</summary>
		string CurrentInputText { get; }

		/// <summary>Gets the current total number of visual log entries existing in the console view.</summary>
		int TotalLogsCount { get; }

		/// <summary>Fired when the console UI transitions from hidden to visible.</summary>
		event Action OnConsoleOpened;

		/// <summary>Fired when the console UI transitions from visible to hidden.</summary>
		event Action OnConsoleClosed;

		/// <summary>Fired when the console's historical log buffer has been entirely wiped.</summary>
		event Action OnConsoleCleared;

		/// <summary>Fired whenever the user modifies the text inside the active input field.</summary>
		event Action<string> OnInputTextChanged;

		/// <summary>Fired the exact moment before a command string enters the parsing and execution pipeline.</summary>
		event Action<string> OnCommandExecuting;

		/// <summary>Fired instantly after a command yields its execution result.</summary>
		event Action<string, ExecutionResult<object?>> OnCommandExecuted;

		/// <summary>Fired whenever a new log entry is broadcasted to the console output.</summary>
		event Action<LogEntry> OnLogAdded;

		/// <summary>Programmatically forces the console UI to open and acquire focus.</summary>
		void Show();

		/// <summary>Programmatically forces the console UI to close and relinquish focus.</summary>
		void Hide();

		/// <summary>Programmatically triggers a full clearing of all historical log entries.</summary>
		void Clear();

		/// <summary>Programmatically forces the shell to execute a string as if the user typed it.</summary>
		/// <param name="rawCommand">The full command syntax string (e.g., "spawn -count 5").</param>
		void ExecuteCommand(string rawCommand);
	}
}