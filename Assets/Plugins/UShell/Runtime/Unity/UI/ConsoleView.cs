using System;
using System.Collections.Generic;
using UnityEngine;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Unity.UI.Components;
using UShell.Runtime.Unity.UI.Configuration;

namespace UShell.Runtime.Unity.UI
{
	public sealed class ConsoleView : MonoBehaviour, IDisposable
	{
		public event Action<string> OnInputChanged = delegate { };

		[SerializeField] private UShellScrollView _scrollView = null!;
		[SerializeField] private UShellInputField _inputField = null!;
		[SerializeField] private UShellSuggestionsContainer _suggestionsContainer = null!;
		[SerializeField] private UShellLogStatsView _statsView = null!;
		[SerializeField] private Canvas _canvas = null!;

		private IReadOnlyList<CommandSuggestion> _currentSuggestions = Array.Empty<CommandSuggestion>();

		public bool IsVisible => _canvas.enabled;
		public string CurrentInput => _inputField.CurrentText;

		public void Initialize(UShellUIConfiguration configuration)
		{
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));

			_scrollView.Initialize(configuration);
			_inputField.Initialize(configuration);
			_suggestionsContainer.Initialize(configuration);

			_inputField.OnInputChanged += HandleInputChanged;
			_scrollView.OnStatsChanged += HandleStatsChanged;

			_statsView.UpdateStats(0, 0, 0);
			Hide();
		}

		public void Dispose()
		{
			_inputField.OnInputChanged -= HandleInputChanged;
			_scrollView.OnStatsChanged -= HandleStatsChanged;
		}

		public void Show()
		{
			_canvas.enabled = true;
			_inputField.ActivateFocus();
		}

		public void Hide()
		{
			_canvas.enabled = false;
			_inputField.DeactivateFocus();
		}

		public void RefocusInput()
		{
			_inputField.Refocus();
		}

		public void AddLogEntry(LogEntry log)
		{
			_scrollView.AddLog(log);
		}

		public void ClearLogs()
		{
			_scrollView.Clear();
		}

		public void SetInputText(string text)
		{
			_inputField.SetInputText(text);
		}

		public void ClearInput()
		{
			_inputField.ClearInput();
		}

		public void RenderAutocomplete(IReadOnlyList<CommandSuggestion> suggestions)
		{
			_currentSuggestions = suggestions;

			string bestSuggestion = suggestions.Count > 0 ? suggestions[0].MatchText : string.Empty;
			_inputField.RenderAutocomplete(bestSuggestion);
		}

		public void RenderSignatures(IReadOnlyList<string> signatures)
		{
			_suggestionsContainer.Render(signatures);
		}

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
	}
}