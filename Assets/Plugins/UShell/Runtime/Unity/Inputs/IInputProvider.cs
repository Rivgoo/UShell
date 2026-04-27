using System;

namespace UShell.Runtime.Unity.Inputs
{
	/// <summary>
	/// Defines a hardware-agnostic contract for providing keybind triggers to the console shell.
	/// </summary>
	/// <remarks>
	/// Implement this interface if you need to use the Old Input Manager, Rewired, or any custom 
	/// input solution instead of the provided New Input System binding.
	/// </remarks>
	public interface IInputProvider
	{
		/// <summary>Fired when the user presses the console toggle hotkey (e.g., Backquote/Tilde).</summary>
		event Action OnToggleConsole;

		/// <summary>Fired when the user submits their command (e.g., Enter).</summary>
		event Action OnSubmit;

		/// <summary>Fired when the user requests an older history entry (e.g., Up Arrow).</summary>
		event Action OnHistoryUp;

		/// <summary>Fired when the user requests a newer history entry (e.g., Down Arrow).</summary>
		event Action OnHistoryDown;

		/// <summary>Fired when the user requests autocompletion of the current text (e.g., Tab).</summary>
		event Action OnAutocomplete;

		/// <summary>Fired when the user attempts to abort an action or close the shell (e.g., Escape).</summary>
		event Action OnEscape;

		/// <summary>
		/// Commands the provider to enable or disable internal UI action maps (excluding the toggle hotkey).
		/// </summary>
		/// <param name="active">True to enable interactive binds, False to disable.</param>
		void SetUIInputActive(bool active);
	}
}