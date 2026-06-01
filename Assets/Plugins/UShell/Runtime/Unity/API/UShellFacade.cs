#nullable enable
using System;
using UShell.Runtime.Core;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Configuration;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Unity.UI;

namespace UShell.Runtime.Unity.API
{
	/// <summary>
	/// The concrete implementation of <see cref="IUShellAPI"/> that safely proxies requests 
	/// and events between the developer and the deeply-nested core shell mechanics.
	/// </summary>
	public sealed class UShellFacade : IUShellAPI
	{
		private readonly IShellCore _core;
		private readonly ConsoleView _view;
		private readonly IConsolePrinter _printer;
		private readonly IShellController _controller;

		/// <summary>
		/// Provides direct access to the underlying shell core engine. 
		/// Useful for advanced power-user integrations.
		/// </summary>
		public IShellCore Core => _core;

		/// <summary>
		/// Provides direct access to the console view layout controller.
		/// </summary>
		public ConsoleView View => _view;

		/// <summary>
		/// Provides direct access to the active console printer instance.
		/// </summary>
		public IConsolePrinter Printer => _printer;

		/// <summary>
		/// Provides direct access to the shell lifecycle event aggregation controller.
		/// </summary>
		public IShellController Controller => _controller;

		/// <inheritdoc/>
		public bool IsVisible => _view.IsVisible;

		/// <inheritdoc/>
		public bool IsExecutingInteractiveCommand => _core.InteractiveSession.IsBusy;

		/// <inheritdoc/>
		public string CurrentInputText => _view.CurrentInput;

		/// <inheritdoc/>
		public int TotalLogsCount => _view.TotalLogsCount;

		/// <inheritdoc/>
		public event Action OnConsoleOpened
		{
			add => _view.OnShow += value;
			remove => _view.OnShow -= value;
		}

		/// <inheritdoc/>
		public event Action OnConsoleClosed
		{
			add => _view.OnHide += value;
			remove => _view.OnHide -= value;
		}

		/// <inheritdoc/>
		public event Action OnConsoleCleared
		{
			add => _view.OnCleared += value;
			remove => _view.OnCleared -= value;
		}

		/// <inheritdoc/>
		public event Action<string> OnInputTextChanged
		{
			add => _view.OnInputChanged += value;
			remove => _view.OnInputChanged -= value;
		}

		/// <inheritdoc/>
		public event Action<string> OnCommandExecuting
		{
			add => _core.Executor.OnExecuting += value;
			remove => _core.Executor.OnExecuting -= value;
		}

		/// <inheritdoc/>
		public event Action<string, ExecutionResult<object?>> OnCommandExecuted
		{
			add => _core.Executor.OnExecuted += value;
			remove => _core.Executor.OnExecuted -= value;
		}

		/// <inheritdoc/>
		public event Action<LogEntry> OnLogAdded
		{
			add => _printer.OnLogAdded += value;
			remove => _printer.OnLogAdded -= value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UShellFacade"/> wrapper.
		/// </summary>
		public UShellFacade(IShellCore core, ConsoleView view, IConsolePrinter printer, IShellController controller)
		{
			_core = core ?? throw new ArgumentNullException(nameof(core));
			_view = view ?? throw new ArgumentNullException(nameof(view));
			_printer = printer ?? throw new ArgumentNullException(nameof(printer));
			_controller = controller ?? throw new ArgumentNullException(nameof(controller));
		}

		/// <inheritdoc/>
		public void Show() => _view.Show();

		/// <inheritdoc/>
		public void Hide() => _view.Hide();

		/// <inheritdoc/>
		public void Clear() => _controller.RequestClear();

		/// <inheritdoc/>
		public void ExecuteCommand(string rawCommand) => _core.Executor.Execute(rawCommand);

		/// <inheritdoc/>
		public IConfigurationTransaction BeginConfiguration() => _core.BeginConfigurationTransaction();
	}
}