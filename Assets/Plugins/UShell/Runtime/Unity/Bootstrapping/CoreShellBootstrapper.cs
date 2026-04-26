#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UShell.Runtime.Core;
using UShell.Runtime.Core.Bootstrapping;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Output.Reporting;
using UShell.Runtime.Core.Registry;
using UShell.Runtime.Unity.BuiltIn;
using UShell.Runtime.Unity.Output;
using UShell.Runtime.Unity.Parsing.Types;

namespace UShell.Runtime.Unity.Bootstrapping
{
	public sealed class CoreShellBootstrapper
	{
		private readonly EnvironmentTag _environment;
		private readonly UnityConsolePrinter _printer;
		private readonly RegistryProxy _registryProxy;
		private readonly List<IShellConfigurator> _configurators = new();

		public CoreShellBootstrapper(EnvironmentTag environment, bool mirrorLogsToUnityConsole = true)
		{
			_environment = environment;
			_printer = new UnityConsolePrinter(mirrorLogsToUnityConsole);
			_registryProxy = new RegistryProxy();
		}

		public CoreShellBootstrapper AddConfigurator(IShellConfigurator configurator)
		{
			if (configurator == null) throw new ArgumentNullException(nameof(configurator));

			_configurators.Add(configurator);
			return this;
		}

		public BootstrapResult Build()
		{
			var builder = new ShellBuilder(_environment);
			var context = new ShellBootstrapContext(_printer, _registryProxy, builder.History, _environment);

			RegisterBuiltInDependencies(builder, context);

			foreach (IShellConfigurator configurator in _configurators)
			{
				configurator.Configure(builder, context);
			}

			IShellCore core = builder.Build();

			var reportingExecutor = new ReportingCommandExecutor(core.Executor, _printer);
			IShellCore decoratedCore = new ShellCore(reportingExecutor, core.Registry, core.History);

			return new BootstrapResult(decoratedCore, _printer, _registryProxy);
		}

		private static void RegisterBuiltInDependencies(ShellBuilder builder, ShellBootstrapContext context)
		{
			builder.AddTypeParser(new Vector2Parser());
			builder.AddTypeParser(new Vector3Parser());
			builder.AddTypeParser(new Vector4Parser());
			builder.AddTypeParser(new QuaternionParser());
			builder.AddTypeParser(new ColorParser());
			builder.AddTypeParser(new GameObjectParser());

			builder.AddProfile(new ConsoleManagementProfile(context.Printer, context.RegistryProxy, context.History));
			builder.AddProfile(new EnvironmentInfoProfile(context.Printer, Application.version, context.ActiveEnvironment));
			builder.AddProfile(new MathUtilityProfile(context.Printer));
			builder.AddProfile(new RuntimeDiagnosticsProfile((UnityConsolePrinter)context.Printer));
		}
	}
}