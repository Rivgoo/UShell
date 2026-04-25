using System;
using System.Collections.Generic;
using UnityEngine;
using UShell.Runtime.Core;
using UShell.Runtime.Core.Bootstrapping;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Output.Reporting;
using UShell.Runtime.Core.Registry;
using UShell.Runtime.Unity.BuiltIn;
using UShell.Runtime.Unity.Inputs;
using UShell.Runtime.Unity.Output;
using UShell.Runtime.Unity.Parsing.Types;
using UShell.Runtime.Unity.UI;
using UShell.Runtime.Unity.UI.Configuration;

namespace UShell.Runtime.Unity.Bootstrapping
{
	[RequireComponent(typeof(ConsoleView))]
	[DisallowMultipleComponent]
	public sealed class UShellManager : MonoBehaviour
	{
		[SerializeField] private EnvironmentTag _activeEnvironment = EnvironmentTag.Development;
		[SerializeField] private bool _mirrorLogsToUnityConsole = true;
		[SerializeField] private UShellUIConfiguration _configuration = null!;

		private ConsoleController _controller = null!;
		private UnityConsolePrinter _printer = null!;

		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
			ValidateDependencies();
			InitializeConsole();
		}

		private void OnDestroy()
		{
			_controller?.Dispose();
		}

		private void ValidateDependencies()
		{
			if (_configuration == null)
			{
				throw new MissingReferenceException("UShellUIConfiguration is not assigned in UShellManager.");
			}
		}

		private void InitializeConsole()
		{
			_printer = new UnityConsolePrinter(_mirrorLogsToUnityConsole);
			var registryProxy = new RegistryProxy();

			Func<IReadOnlyList<string>> historyProvider = () => _controller?.History ?? Array.Empty<string>();

			var builtInProfile = BuiltInProfileConfigurator.Create(
				_printer, 
				registryProxy, 
				Application.version, 
				_activeEnvironment, 
				historyProvider);

			IShellCore core = new ShellBuilder(_activeEnvironment)
				.AddTypeParser(new Vector2Parser())
				.AddTypeParser(new Vector3Parser())
				.AddTypeParser(new Vector4Parser())
				.AddTypeParser(new QuaternionParser())
				.AddTypeParser(new ColorParser())
				.AddTypeParser(new GameObjectParser())
				.AddProfile(builtInProfile)
				.Build();

			registryProxy.Target = core.Registry;

			var reportingExecutor = new ReportingCommandExecutor(core.Executor, _printer);
			IShellCore decoratedCore = new ShellCore(reportingExecutor, core.Registry);

			var view = GetComponent<ConsoleView>();
			view.Initialize(_configuration);

			IInputProvider inputProvider = GetInputProvider();

			_controller = new ConsoleController(decoratedCore, view, inputProvider, _printer);
		}

		private IInputProvider GetInputProvider()
		{
			var provider = GetComponent<InputSystemProvider>();
			if (provider != null) return provider;

			return gameObject.AddComponent<InputSystemProvider>();
		}
	}
}