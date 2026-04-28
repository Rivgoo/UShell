using System;

namespace UShell.Runtime.Core.Abstractions
{
	/// <summary>
	/// The base implementation of <see cref="IShellController"/> acting as an event aggregator 
	/// for lifecycle commands.
	/// </summary>
	public sealed class ShellController : IShellController
	{
		/// <inheritdoc/>
		public event Action OnClearRequested = delegate { };

		/// <inheritdoc/>
		public event Action OnCloseRequested = delegate { };

		/// <inheritdoc/>
		public void RequestClear() => OnClearRequested.Invoke();

		/// <inheritdoc/>
		public void RequestClose() => OnCloseRequested.Invoke();
	}
}