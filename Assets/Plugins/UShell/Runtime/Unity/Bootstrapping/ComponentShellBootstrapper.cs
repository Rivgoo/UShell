#nullable enable
using UnityEngine;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Unity.BuiltIn;
using UShell.Runtime.Unity.Output;

namespace UShell.Runtime.Unity.Bootstrapping
{
	/// <summary>
	/// A <see cref="MonoBehaviour"/> that acts as the entry point for initializing the UShell environment 
	/// via the Unity Inspector. 
	/// </summary>
	/// <remarks>
	/// This component collects configuration settings, registers built-in profiles based on Inspector toggles, 
	/// searches for custom <see cref="IShellConfigurator"/> components on child objects, and finally 
	/// bootstraps the <see cref="UShellManager"/>.
	/// </remarks>
	[RequireComponent(typeof(UShellManager))]
	[DisallowMultipleComponent]
	public sealed class ComponentShellBootstrapper : MonoBehaviour
	{
		[Header("Bootstrapper settings")]
		[Tooltip("Defines the current environment tag. Commands restricted to other environments will be ignored.")]
		[SerializeField] private EnvironmentTag _activeEnvironment = EnvironmentTag.Development;

		[Tooltip("If true, all UShell output logs will also be printed to the standard Unity Editor Console.")]
		[SerializeField] private bool _mirrorLogsToUnityConsole = false;

		[Header("Core Profiles")]
		[Tooltip("Includes essential console commands like 'help', 'clear', 'history', and 'alias'.")]
		[SerializeField] private bool _includeConsoleManagementProfile = true;

		[Tooltip("Includes commands for modifying App/Screen/Time settings like 'screen.resolution', 'time.scale', 'gfx.fps'.")]
		[SerializeField] private bool _includeApplicationSettingsProfile = true;

		[Tooltip("Includes scene management commands like 'scene.load', 'scene.active', 'scene.list'.")]
		[SerializeField] private bool _includeSceneManagementProfile = true;

		[Header("Utility Profiles")]
		[Tooltip("Includes mathematical commands like 'eval', 'random', and unit 'convert'.")]
		[SerializeField] private bool _includeMathUtilityProfile = true;

		[Tooltip("Includes system info commands like 'info', 'platform', and 'game.version'.")]
		[SerializeField] private bool _includeEnvironmentInfoProfile = true;

		[Tooltip("Includes GameObject commands like 'go.teleport', 'go.active', 'go.info'.")]
		[SerializeField] private bool _includeGameObjectProfile = true;

		[Header("Diagnostic & Developer Profiles")]
		[Tooltip("Includes deep Unity diagnostic commands like 'stats', 'mem', 'gc', and 'tag.find'.")]
		[SerializeField] private bool _includeRuntimeDiagnosticsProfile = true;

		[Tooltip("Includes interactive HTTP commands for testing APIs: 'http.get', 'http.post', etc.")]
		[SerializeField] private bool _includeHttpProfile = true;

		[Tooltip("Includes file system commands like 'file.read', 'file.write', and 'screenshot'.")]
		[SerializeField] private bool _includeFileIOProfile = true;

		private void Awake()
		{
			var bootstrapper = new CoreShellBootstrapper(_activeEnvironment, _mirrorLogsToUnityConsole);

			IShellConfigurator[] configurators = GetComponentsInChildren<IShellConfigurator>();

			foreach (IShellConfigurator configurator in configurators)
			{
				bootstrapper.AddConfigurator(configurator);
			}

			RegisterBuiltInDependencies(bootstrapper);

			BootstrapResult result = bootstrapper.Build();

			var manager = GetComponent<UShellManager>();
			manager.Initialize(result);
		}

		private void RegisterBuiltInDependencies(CoreShellBootstrapper bootstrapper)
		{
			// Core
			if (_includeConsoleManagementProfile)
				bootstrapper.AddProfile(context => new ConsoleManagementProfile(context.Printer, context.RegistryProxy, context.History, context.SessionState, context.Controller));

			if (_includeApplicationSettingsProfile)
				bootstrapper.AddProfile(context => new ApplicationSettingsProfile(context.Printer));

			if (_includeSceneManagementProfile)
				bootstrapper.AddProfile(context => new SceneManagementProfile(context.Printer));

			// Utilities
			if (_includeEnvironmentInfoProfile)
				bootstrapper.AddProfile(context => new EnvironmentInfoProfile(context.Printer, Application.version, context.ActiveEnvironment));

			if (_includeMathUtilityProfile)
				bootstrapper.AddProfile(context => new MathUtilityProfile(context.Printer));

			if (_includeGameObjectProfile)
				bootstrapper.AddProfile(context => new GameObjectProfile(context.Printer));

			// Diagnostics & Developer Tools
			if (_includeRuntimeDiagnosticsProfile)
				bootstrapper.AddProfile(context => new RuntimeDiagnosticsProfile((UnityConsolePrinter)context.Printer));

			if (_includeHttpProfile)
				bootstrapper.AddProfile(context => new HttpProfile(context.Printer));

			if (_includeFileIOProfile)
				bootstrapper.AddProfile(context => new FileIOProfile(context.Printer));
		}
	}
}