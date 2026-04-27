#nullable enable
using System;
using System.Collections.Generic;
using UShell.Runtime.Core;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Unity.BuiltIn;
using UShell.Runtime.Unity.Inputs;
using UShell.Runtime.Unity.UI;

namespace UShell.Runtime.Unity
{
	internal sealed class ConsoleRuntimeEngine : IDisposable
	{
		private readonly IShellCore _core;
		private readonly ConsoleView _view;
		private readonly IInputProvider _input;
		private readonly IConsolePrinter _printer;

		public ConsoleRuntimeEngine(
			IShellCore core,
			ConsoleView view,
			IInputProvider input,
			IConsolePrinter printer)
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

			_printer.OnLogAdded += _view.AddLogEntry;
			_printer.OnLogUpdated += _view.UpdateLogEntry;

			_core.InteractiveSession.OnStateChanged += UpdateInputState;

			ConsoleManagementProfile.OnClearRequested += _view.ClearLogs;
			ConsoleManagementProfile.OnCloseRequested += CloseConsole;
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

			_printer.OnLogAdded -= _view.AddLogEntry;
			_printer.OnLogUpdated -= _view.UpdateLogEntry;

			_core.InteractiveSession.OnStateChanged -= UpdateInputState;

			ConsoleManagementProfile.OnClearRequested -= _view.ClearLogs;
			ConsoleManagementProfile.OnCloseRequested -= CloseConsole;

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
			if (_core.InteractiveSession.IsBusy)
			{
				_core.InteractiveSession.Cancel();
			}
			else if (_view.IsVisible)
			{
				CloseConsole();
			}
		}

		private void Submit()
		{
			if (!_view.IsVisible) return;

			string input = _view.CurrentInput;

			if (_core.InteractiveSession.IsWaitingForPrompt)
			{
				_printer.Print(new LogEntry($"> {input}", LogType.Standard));
				_core.InteractiveSession.SubmitInput(input);
				_view.ClearInput();
				return;
			}

			if (_core.InteractiveSession.IsBusy)
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(input))
			{
				_view.RefocusInput();
				return;
			}

			_printer.Print(new LogEntry(input, LogType.Standard));

			_core.History.Add(input);
			_core.History.ResetNavigation();

			_view.ClearInput();
			_view.RenderAutocomplete(Array.Empty<CommandSuggestion>());
			_view.RenderSignatures(Array.Empty<string>());

			_core.Executor.Execute(input);

			if (_view.IsVisible)
				_view.RefocusInput();
		}

		private void InputChanged(string input)
		{
			if (_core.InteractiveSession.IsBusy)
			{
				_view.RenderAutocomplete(Array.Empty<CommandSuggestion>());
				_view.RenderSignatures(Array.Empty<string>());
				return;
			}

			if (string.IsNullOrWhiteSpace(input))
			{
				_view.RenderAutocomplete(Array.Empty<CommandSuggestion>());
				_view.RenderSignatures(Array.Empty<string>());
				return;
			}

			ReadOnlySpan<char> firstWord = GetFirstWord(input);
			bool hasSpace = input.AsSpan().TrimStart().IndexOf(' ') >= 0;

			var suggestions = _core.Registry.GetSuggestions(firstWord);
			_view.RenderAutocomplete(suggestions);

			var signatureStrings = new List<string>(5);

			if (hasSpace)
			{
				if (_core.Registry.TryGetCommand(firstWord.ToString(), out CommandSignature exactCmd))
				{
					signatureStrings.Add(_core.Registry.GetCompactSignature(exactCmd));
				}
			}
			else
			{
				int count = Math.Min(suggestions.Count, 5);
				for (int i = 0; i < count; i++)
				{
					signatureStrings.Add(_core.Registry.GetCompactSignature(suggestions[i].Signature));
				}
			}

			_view.RenderSignatures(signatureStrings);
		}

		private void HistoryUp()
		{
			if (!_view.IsVisible || _core.InteractiveSession.IsBusy) return;

			string? previous = _core.History.GetPrevious(_view.CurrentInput);
			if (previous != null)
			{
				_view.SetInputText(previous);
			}
		}

		private void HistoryDown()
		{
			if (!_view.IsVisible || _core.InteractiveSession.IsBusy) return;

			string? next = _core.History.GetNext();
			if (next != null)
			{
				_view.SetInputText(next);
			}
		}

		private void TabComplete()
		{
			if (!_view.IsVisible || _core.InteractiveSession.IsBusy) return;

			string suggestion = _view.GetSelectedSuggestionName();
			if (string.IsNullOrEmpty(suggestion)) return;

			string completedText = suggestion + " ";
			_view.SetInputText(completedText);
			InputChanged(completedText);
		}

		private void UpdateInputState()
		{
			if (_core.InteractiveSession.IsWaitingForPrompt)
			{
				_view.SetInputMode(ConsoleInputMode.Prompt);
			}
			else if (_core.InteractiveSession.IsBusy)
			{
				_view.SetInputMode(ConsoleInputMode.Locked);
			}
			else
			{
				_view.SetInputMode(ConsoleInputMode.Standard);
			}

			if (_view.IsVisible)
			{
				_view.RefocusInput();
			}
		}

		private static ReadOnlySpan<char> GetFirstWord(string input)
		{
			ReadOnlySpan<char> trimmed = input.AsSpan().TrimStart();
			int idx = trimmed.IndexOf(' ');
			return idx > 0 ? trimmed.Slice(0, idx) : trimmed;
		}
	}
}