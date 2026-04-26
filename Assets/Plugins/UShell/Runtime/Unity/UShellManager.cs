#nullable enable
using System;
using UnityEngine;
using UShell.Runtime.Unity.Inputs;
using UShell.Runtime.Unity.UI;

namespace UShell.Runtime.Unity.Bootstrapping
{
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

		public void Initialize(BootstrapResult bootstrapResult)
		{
			if (bootstrapResult == null) throw new ArgumentNullException(nameof(bootstrapResult));
			if (_isInitialized) return;

			bootstrapResult.RegistryProxy.Target = bootstrapResult.Core.Registry;

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