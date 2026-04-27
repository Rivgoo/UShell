#nullable enable
using System;
using UnityEngine;
using UShell.Runtime.Core.Registry;
using UShell.Runtime.Unity.Inputs;
using UShell.Runtime.Unity.UI;

namespace UShell.Runtime.Unity.Bootstrapping
{
	/// <summary>
	/// The persistent root <see cref="MonoBehaviour"/> that binds the Core Shell, Input, and UI together.
	/// </summary>
	/// <remarks>
	/// This GameObject is automatically marked as <c>DontDestroyOnLoad</c> and guarantees that the console 
	/// survives across scene transitions.
	/// </remarks>
	[DisallowMultipleComponent]
	public sealed class UShellManager : MonoBehaviour
	{
		private ConsoleRuntimeEngine? _engine;
		private bool _isInitialized;

		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
		}

		private void OnDestroy()
		{
			_engine?.Dispose();
		}

		/// <summary>
		/// Bootstraps the manager with the built dependencies and resolves proxy references.
		/// </summary>
		/// <param name="bootstrapResult">The configured data from the Bootstrapper.</param>
		/// <exception cref="ArgumentNullException">Thrown if the result is null.</exception>
		public void Initialize(BootstrapResult bootstrapResult)
		{
			if (bootstrapResult == null) throw new ArgumentNullException(nameof(bootstrapResult));
			if (_isInitialized) return;

			((RegistryProxy)bootstrapResult.CommandRegistry).Target = bootstrapResult.Core.Registry;

			ConsoleView view = GetConsoleView();
			view.Initialize();

			IInputProvider inputProvider = GetInputProvider();

			_engine = new ConsoleRuntimeEngine(bootstrapResult.Core, view, inputProvider, bootstrapResult.Printer);

			_isInitialized = true;
		}

		private IInputProvider GetInputProvider()
		{
			if (TryGetComponent<InputSystemProvider>(out var provider))
				return provider;

			return gameObject.AddComponent<InputSystemProvider>();
		}

		private ConsoleView GetConsoleView()
		{
			ConsoleView view = GetComponentInChildren<ConsoleView>(true);

			if (view == null)
			{
				throw new MissingComponentException("No ConsoleView found in children of UShellManager. Please ensure a ConsoleView is present in the scene.");
			}

			return view;
		}
	}
}