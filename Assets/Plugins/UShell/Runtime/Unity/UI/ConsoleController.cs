using System;
using System.Collections.Generic;
using UShell.Runtime.Core;
using UShell.Runtime.Unity.BuiltIn;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Unity.Inputs;
using UShell.Runtime.Unity.Output;

namespace UShell.Runtime.Unity.UI
{
	internal sealed class ConsoleController : IDisposable
	{
		private readonly IShellCore _core;
		private readonly ConsoleView _view;
		private readonly IInputProvider _input;
		private readonly UnityConsolePrinter _printer;

		private readonly List<string> _history = new(50);
		private int _historyIndex = -1;

		public IReadOnlyList<string> History => _history;

		public ConsoleController(
			IShellCore core,
			ConsoleView view,
			IInputProvider input,
			UnityConsolePrinter printer)
		{
			_core = core ?? throw new ArgumentNullException(nameof(core));
			_view = view ?? throw new ArgumentNullException(nameof(view));
			_input = input ?? throw new ArgumentNullException(nameof(input));
			_printer = printer ?? throw new ArgumentNullException(nameof(printer));

			BindEvents();
		}

		public void Dispose()
		{
			UnbindEvents();
		}

		private void BindEvents()
		{
			_input.OnToggleConsole += Toggle;
			_input.OnSubmit += Submit;
			_input.OnHistoryUp += HistoryUp;
			_input.OnHistoryDown += HistoryDown;
			_input.OnAutocomplete += TabComplete;
			_input.OnEscape += Escape;

			_view.OnInputChanged += InputChanged;
			_printer.OnLogAdded += LogAdded;

			BuiltInShellProfile.OnClearRequested += _view.ClearLogs;
			BuiltInShellProfile.OnCloseRequested += CloseConsole;
		}

		private void UnbindEvents()
		{
			_input.OnToggleConsole -= Toggle;
			_input.OnSubmit -= Submit;
			_input.OnHistoryUp -= HistoryUp;
			_input.OnHistoryDown -= HistoryDown;
			_input.OnAutocomplete -= TabComplete;
			_input.OnEscape -= Escape;

			_view.OnInputChanged -= InputChanged;
			_printer.OnLogAdded -= LogAdded;

			BuiltInShellProfile.OnClearRequested -= _view.ClearLogs;
			BuiltInShellProfile.OnCloseRequested -= CloseConsole;

			_view.Dispose();
		}

		private void Toggle()
		{
			if (_view.IsVisible) CloseConsole();
			else OpenConsole();
		}

		private void OpenConsole()
		{
			_view.Show();
			_input.SetUIInputActive(true);
		}

		private void CloseConsole()
		{
			_view.Hide();
			_input.SetUIInputActive(false);
		}

		private void Escape()
		{
			if (_view.IsVisible)
			{
				CloseConsole();
			}
		}

		private void Submit()
		{
			if (!_view.IsVisible) return;

			string input = _view.CurrentInput;
			if (string.IsNullOrWhiteSpace(input))
			{
				_view.RefocusInput();
				return;
			}

			_printer.Print(new LogEntry(input, LogType.Standard));
			AddToHistory(input);
			_view.ClearInput();

			_core.Executor.Execute(input);
			_view.RefocusInput();
		}

		private void InputChanged(string input)
		{
			_historyIndex = -1;

			if (string.IsNullOrWhiteSpace(input))
			{
				_view.RenderAutocomplete(Array.Empty<CommandSignature>());
				return;
			}

			ReadOnlySpan<char> firstWord = GetFirstWord(input);
			var suggestions = _core.Registry.GetSuggestions(firstWord);

			_view.RenderAutocomplete(suggestions);
		}

		private void HistoryUp()
		{
			if (!_view.IsVisible || _history.Count == 0) return;

			_historyIndex = _historyIndex <= 0 ? _history.Count - 1 : _historyIndex - 1;
			_view.SetInputText(_history[_historyIndex]);
		}

		private void HistoryDown()
		{
			if (!_view.IsVisible || _history.Count == 0 || _historyIndex == -1) return;

			if (_historyIndex < _history.Count - 1)
			{
				_historyIndex++;
				_view.SetInputText(_history[_historyIndex]);
			}
			else
			{
				_historyIndex = -1;
				_view.ClearInput();
			}
		}

		private void TabComplete()
		{
			if (!_view.IsVisible) return;

			string suggestion = _view.GetSelectedSuggestionName();
			if (string.IsNullOrEmpty(suggestion)) return;

			string completedText = suggestion + " ";
			_view.SetInputText(completedText);
			InputChanged(completedText);
		}

		private void LogAdded(LogEntry log)
		{
			_view.AddLogEntry(log);
		}

		private void AddToHistory(string input)
		{
			_history.Remove(input);
			_history.Add(input);

			if (_history.Count > 100) _history.RemoveAt(0);
			_historyIndex = -1;
		}

		private static ReadOnlySpan<char> GetFirstWord(string input)
		{
			int idx = input.IndexOf(' ');
			return idx > 0 ? input.AsSpan(0, idx) : input.AsSpan();
		}
	}
}