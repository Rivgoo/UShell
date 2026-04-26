using UnityEngine;
using UShell.Runtime.Core.Commands;

namespace UShell.Runtime.Unity.Bootstrapping
{
	[RequireComponent(typeof(UShellManager))]
	[DisallowMultipleComponent]
	public sealed class ComponentShellBootstrapper : MonoBehaviour
	{
		[SerializeField] private EnvironmentTag _activeEnvironment = EnvironmentTag.Development;
		[SerializeField] private bool _mirrorLogsToUnityConsole = false;

		private void Awake()
		{
			var bootstrapper = new CoreShellBootstrapper(_activeEnvironment, _mirrorLogsToUnityConsole);

			IShellConfigurator[] configurators = GetComponentsInChildren<IShellConfigurator>();

			foreach (IShellConfigurator configurator in configurators)
				bootstrapper.AddConfigurator(configurator);

			BootstrapResult result = bootstrapper.Build();

			var manager = GetComponent<UShellManager>();
			manager.Initialize(result);
		}
	}
}