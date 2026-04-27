using UnityEngine;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Unity.BuiltIn;
using UShell.Runtime.Unity.Output;

namespace UShell.Runtime.Unity.Bootstrapping
{
	[RequireComponent(typeof(UShellManager))]
	[DisallowMultipleComponent]
	public sealed class ComponentShellBootstrapper : MonoBehaviour
	{
		[Header("Bootstrapper settings")]
		[SerializeField] private EnvironmentTag _activeEnvironment = EnvironmentTag.Development;
		[SerializeField] private bool _mirrorLogsToUnityConsole = false;

		[Header("Built-in profiles")]
		[SerializeField] private bool _includeConsoleManagementProfile = true;
		[SerializeField] private bool _includeMathUtilityProfile = true;
		[SerializeField] private bool _includeEnvironmentInfoProfile = true;
		[SerializeField] private bool _includeRuntimeDiagnosticsProfile = true;

		private void Awake()
		{
			var bootstrapper = new CoreShellBootstrapper(_activeEnvironment, _mirrorLogsToUnityConsole);

			IShellConfigurator[] configurators = GetComponentsInChildren<IShellConfigurator>();

			foreach (IShellConfigurator configurator in configurators)
				bootstrapper.AddConfigurator(configurator);

			RegisterBuiltInDependencies(bootstrapper);

			BootstrapResult result = bootstrapper.Build();

			var manager = GetComponent<UShellManager>();
			manager.Initialize(result);
		}

		private void RegisterBuiltInDependencies(CoreShellBootstrapper bootstrapper)
		{
			if (_includeConsoleManagementProfile)
				bootstrapper.AddProfile(context => new ConsoleManagementProfile(context.Printer, context.RegistryProxy, context.History, context.SessionState));
			if (_includeEnvironmentInfoProfile)
				bootstrapper.AddProfile(context => new EnvironmentInfoProfile(context.Printer, Application.version, context.ActiveEnvironment));
			if (_includeMathUtilityProfile)
				bootstrapper.AddProfile(context => new MathUtilityProfile(context.Printer));
			if (_includeRuntimeDiagnosticsProfile)
				bootstrapper.AddProfile(context => new RuntimeDiagnosticsProfile((UnityConsolePrinter)context.Printer));
		}
	}
}