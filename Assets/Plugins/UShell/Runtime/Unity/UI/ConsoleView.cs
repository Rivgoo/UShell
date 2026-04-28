using System;
using System.Collections.Generic;
using UnityEngine;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Unity.UI.Components;
using UShell.Runtime.Unity.UI.Configuration;

namespace UShell.Runtime.Unity.UI
{
	/// <summary>
	/// The primary UI controller that orchestrates the visual elements of the UShell console.
	/// </summary>
	/// <remarks>
	/// Attach this to the root of the console's Canvas. It acts as the facade for logging, 
	/// input handling, and autocompletion visuals.
	/// </remarks>
	[DisallowMultipleComponent]
	public sealed class ConsoleView : MonoBehaviour, IDisposable
	{
		/// <summary>Fired whenever the text in the input field changes.</summary>
		public event Action<string> OnInputChanged = delegate { };

		/// <summary>Fired when the console UI is requested to be displayed.</summary>
		public event Action OnShow = delegate { };

		/// <summary>Fired when the console UI is requested to be hidden.</summary>
		public event Action OnHide = delegate { };

		/// <summary>Fired when the visual log buffer has been cleared.</summary>
		public event Action OnCleared = delegate { };

		[SerializeField] private UShellUIConfiguration _configuration = null!;

		[SerializeField, Space] private UShellScrollView _scrollView = null!;
		[SerializeField] private UShellInputField _inputField = null!;
		[SerializeField] private UShellSuggestionsContainer _suggestionsContainer = null!;
		[SerializeField] private UShellLogStatsView _statsView = null!;
		[SerializeField] private Canvas _canvas = null!;

		private IReadOnlyList<CommandSuggestion> _currentSuggestions = Array.Empty<CommandSuggestion>();

		/// <summary>Returns true if the console is currently open and visible to the user.</summary>
		public bool IsVisible => _canvas.enabled;

		/// <summary>Gets the raw text currently typed into the input field.</summary>
		public string CurrentInput => _inputField.CurrentText;

		/// <summary>Gets the amount of log items currently rendered on the screen.</summary>
		public int TotalLogsCount => _scrollView.TotalLogsCount;

		/// <summary>
		/// Validates dependencies, applies the visual configuration, and prepares the UI for interaction.
		/// </summary>
		public void Initialize()
		{
			if (!ValidateDependencies())
				return;

			_scrollView.Initialize(_configuration);
			_inputField.Initialize(_configuration);
			_suggestionsContainer.Initialize(_configuration);

			_inputField.OnInputChanged += HandleInputChanged;
			_scrollView.OnStatsChanged += HandleStatsChanged;

			_statsView.UpdateStats(0, 0, 0);
			Hide();
		}

		/// <summary>
		/// Cleans up event subscriptions attached during initialization.
		/// </summary>
		public void Dispose()
		{
			_inputField.OnInputChanged -= HandleInputChanged;
			_scrollView.OnStatsChanged -= HandleStatsChanged;
		}

		/// <summary>
		/// Displays the console overlay and forces focus into the input field.
		/// </summary>
		public void Show()
		{
			_canvas.enabled = true;
			_inputField.ActivateFocus();
			OnShow.Invoke();
		}

		/// <summary>
		/// Hides the console overlay and removes focus from the input field.
		/// </summary>
		public void Hide()
		{
			_canvas.enabled = false;
			_inputField.DeactivateFocus();
			OnHide.Invoke();
		}

		/// <summary>
		/// Modifies the visual state of the input field (e.g., standard, locked, prompting).
		/// </summary>
		public void SetInputMode(ConsoleInputMode mode)
		{
			_inputField.SetMode(mode);
		}

		/// <summary>
		/// Forces the input field to regain cursor focus, provided the console is visible.
		/// </summary>
		public void RefocusInput()
		{
			_inputField.Refocus();
		}

		/// <summary>
		/// Instantiates and appends a new log entry to the scroll view.
		/// </summary>
		public void AddLogEntry(LogEntry log)
		{
			_scrollView.AddLog(log);
		}

		/// <summary>
		/// Locates an existing log by its ID and overwrites its content.
		/// </summary>
		public void UpdateLogEntry(Guid id, LogEntry log)
		{
			_scrollView.UpdateLog(id, log);
		}

		/// <summary>
		/// Destroys all visual log entries and resets stat counters.
		/// </summary>
		public void ClearLogs()
		{
			_scrollView.Clear();
			OnCleared.Invoke();
		}

		/// <summary>
		/// Overwrites the text in the input field without triggering a submission.
		/// </summary>
		public void SetInputText(string text)
		{
			_inputField.SetInputText(text);
		}

		/// <summary>
		/// Empties the input field.
		/// </summary>
		public void ClearInput()
		{
			_inputField.ClearInput();
		}

		/// <summary>
		/// Updates the ghost text in the input field based on the top suggestion.
		/// </summary>
		public void RenderAutocomplete(IReadOnlyList<CommandSuggestion> suggestions)
		{
			_currentSuggestions = suggestions;

			string bestSuggestion = suggestions.Count > 0 ? suggestions[0].MatchText : string.Empty;
			_inputField.RenderAutocomplete(bestSuggestion);
		}

		/// <summary>
		/// Renders a list of text blocks (like command signatures) below the input field.
		/// </summary>
		public void RenderSignatures(IReadOnlyList<string> signatures)
		{
			_suggestionsContainer.Render(signatures);
		}

		/// <summary>
		/// Gets the string value of the top-ranked suggestion currently displayed.
		/// </summary>
		public string GetSelectedSuggestionName()
		{
			return _currentSuggestions.Count > 0 ? _currentSuggestions[0].MatchText : string.Empty;
		}

		private void HandleInputChanged(string input)
		{
			OnInputChanged.Invoke(input);
		}

		private void HandleStatsChanged(int infoCount, int warningCount, int errorCount)
		{
			_statsView.UpdateStats(infoCount, warningCount, errorCount);
		}

		private bool ValidateDependencies()
		{
			if (_configuration == null)
			{
				Debug.LogError("UShellUIConfiguration is not assigned in ConsoleView.");
				return false;
			}

			if (_scrollView == null)
			{
				Debug.LogError("UShellScrollView is not assigned in ConsoleView.");
				return false;
			}

			if (_inputField == null)
			{
				Debug.LogError("UShellInputField is not assigned in ConsoleView.");
				return false;
			}

			if (_suggestionsContainer == null)
			{
				Debug.LogError("UShellSuggestionsContainer is not assigned in ConsoleView.");
				return false;
			}

			if (_statsView == null)
			{
				Debug.LogError("UShellLogStatsView is not assigned in ConsoleView.");
				return false;
			}

			if (_canvas == null)
			{
				Debug.LogError("Canvas is not assigned in ConsoleView.");
				return false;
			}

			return true;
		}
	}
}