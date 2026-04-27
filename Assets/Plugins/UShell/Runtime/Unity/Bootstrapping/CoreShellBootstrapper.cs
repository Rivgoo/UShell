#nullable enable
using System;
using System.Collections.Generic;
using UShell.Runtime.Core;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Bootstrapping;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Execution.Context;
using UShell.Runtime.Core.Output.Reporting;
using UShell.Runtime.Core.Parsing.Types;
using UShell.Runtime.Core.Registry;
using UShell.Runtime.Unity.Output;
using UShell.Runtime.Unity.Parsing.Types;

namespace UShell.Runtime.Unity.Bootstrapping
{
	/// <summary>
	/// A pure C# wrapper around the core <see cref="ShellBuilder"/> tailored specifically for Unity integration.
	/// </summary>
	/// <remarks>
	/// This bootstrapper automatically injects Unity-specific parsers (Vector3, GameObject, Color, etc.) 
	/// and sets up the reporting decorator to catch unhandled exceptions.
	/// </remarks>
	public sealed class CoreShellBootstrapper
	{
		private readonly EnvironmentTag _environment;
		private readonly UnityConsolePrinter _printer;
		private readonly RegistryProxy _registryProxy;
		private readonly List<IShellConfigurator> _configurators = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="CoreShellBootstrapper"/> class.
		/// </summary>
		/// <param name="environment">The active environment (e.g., Development, Release).</param>
		/// <param name="mirrorLogsToUnityConsole">Whether logs should be forwarded to <see cref="UnityEngine.Debug"/>.</param>
		public CoreShellBootstrapper(EnvironmentTag environment, bool mirrorLogsToUnityConsole = true)
		{
			_environment = environment;
			_printer = new UnityConsolePrinter(mirrorLogsToUnityConsole);
			_registryProxy = new RegistryProxy();
		}

		/// <summary>
		/// Adds a static profile instance to the shell.
		/// </summary>
		public CoreShellBootstrapper AddProfile(IShellProfile profile)
		{
			if (profile == null) throw new ArgumentNullException(nameof(profile));
			return AddConfigurator(new DelegateConfigurator((builder, _) => builder.AddProfile(profile)));
		}

		/// <summary>
		/// Adds a factory delegate that resolves a profile instance using the provided context.
		/// </summary>
		public CoreShellBootstrapper AddProfile(Func<ShellBootstrapContext, IShellProfile> profileFactory)
		{
			if (profileFactory == null) throw new ArgumentNullException(nameof(profileFactory));
			return AddConfigurator(new DelegateConfigurator((builder, context) => builder.AddProfile(profileFactory(context))));
		}

		/// <summary>
		/// Registers a custom Unity type parser.
		/// </summary>
		public CoreShellBootstrapper AddTypeParser<T>(ITypeParser<T> parser)
		{
			if (parser == null) throw new ArgumentNullException(nameof(parser));
			return AddConfigurator(new DelegateConfigurator((builder, _) => builder.AddTypeParser(parser)));
		}

		/// <summary>
		/// Attaches a custom configurator that modifies the underlying <see cref="ShellBuilder"/>.
		/// </summary>
		public CoreShellBootstrapper AddConfigurator(IShellConfigurator configurator)
		{
			if (configurator == null) throw new ArgumentNullException(nameof(configurator));
			_configurators.Add(configurator);
			return this;
		}

		/// <summary>
		/// Executes the configuration chain, wiring up Unity parsers, decorators, and profiles.
		/// </summary>
		/// <returns>A structured container with the initialized shell core.</returns>
		public BootstrapResult Build()
		{
			var interactiveSession = new InteractiveSession(_printer);
			var builder = new ShellBuilder(_printer, _environment, interactiveSession);
			var context = new ShellBootstrapContext(_printer, _registryProxy, builder.History, interactiveSession, builder.SessionState, _environment);

			RegisterBuiltInDependencies(builder, context);

			foreach (IShellConfigurator configurator in _configurators)
			{
				configurator.Configure(builder, context);
			}

			IShellCore core = builder.Build();

			var reportingExecutor = new ReportingCommandExecutor(core.Executor, _printer);
			IShellCore decoratedCore = new ShellCore(reportingExecutor, core.Registry, core.History, core.InteractiveSession, core.SessionState);

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
		}

		private sealed class DelegateConfigurator : IShellConfigurator
		{
			private readonly Action<ShellBuilder, ShellBootstrapContext> _configureAction;
			public DelegateConfigurator(Action<ShellBuilder, ShellBootstrapContext> configureAction)
			{
				_configureAction = configureAction;
			}
			public void Configure(ShellBuilder builder, ShellBootstrapContext context)
			{
				_configureAction(builder, context);
			}
		}
	}
}