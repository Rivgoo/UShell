using System;

namespace UShell.Runtime.Core.Abstractions
{
	/// <summary>
	/// Provides a decoupled mechanism for core commands to request high-level lifecycle 
	/// changes (like closing the UI or clearing logs) without directly depending on the view.
	/// </summary>
	public interface IShellController
	{
		/// <summary>Fired when a command requests the console history to be visually cleared.</summary>
		event Action OnClearRequested;

		/// <summary>Fired when a command requests the console window to be closed.</summary>
		event Action OnCloseRequested;

		/// <summary>Issues a request to clear the console output.</summary>
		void RequestClear();

		/// <summary>Issues a request to hide/close the console interface.</summary>
		void RequestClose();
	}
}