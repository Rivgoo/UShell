using System;

namespace UShell.Runtime.Unity.Inputs
{
	public interface IInputProvider
	{
		event Action OnToggleConsole;

		event Action OnSubmit;
		event Action OnHistoryUp;
		event Action OnHistoryDown;
		event Action OnAutocomplete;
		event Action OnEscape;

		void SetUIInputActive(bool active);
	}
}