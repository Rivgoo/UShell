#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Commands.Fluent;
using UShell.Runtime.Core.Execution.Context;
using UShell.Runtime.Core.Parsing.Types;
using UShell.Runtime.Core.Registry;

namespace UShell.Runtime.Core.Configuration
{
	internal sealed class ConfigurationTransaction : IConfigurationTransaction
	{
		private readonly CommandRegistry _commandRegistry;
		private readonly ITypeParserRegistry _parserRegistry;
		private readonly IInteractiveSession _session;
		private readonly IShellController _controller;
		private readonly EnvironmentTag _activeEnvironment;

		private readonly List<IShellProfile> _profilesToAdd = new();
		private readonly List<Type> _profilesToRemove = new();
		private readonly List<IShellProfile> _exactProfilesToRemove = new();

		private readonly List<ITypeParser> _parsersToAdd = new();
		private readonly List<Type> _parsersToRemove = new();

		public ConfigurationTransaction(
			CommandRegistry commandRegistry,
			ITypeParserRegistry parserRegistry,
			IInteractiveSession session,
			IShellController controller,
			EnvironmentTag activeEnvironment)
		{
			_commandRegistry = commandRegistry ?? throw new ArgumentNullException(nameof(commandRegistry));
			_parserRegistry = parserRegistry ?? throw new ArgumentNullException(nameof(parserRegistry));
			_session = session ?? throw new ArgumentNullException(nameof(session));
			_controller = controller ?? throw new ArgumentNullException(nameof(controller));
			_activeEnvironment = activeEnvironment;
		}

		public IConfigurationTransaction AddProfile(IShellProfile profile)
		{
			if (profile != null) _profilesToAdd.Add(profile);
			return this;
		}

		public IConfigurationTransaction AddTypeParser<T>(ITypeParser<T> parser)
		{
			if (parser != null) _parsersToAdd.Add(parser);
			return this;
		}

		public IConfigurationTransaction RemoveProfile<TProfile>() where TProfile : IShellProfile
		{
			return RemoveProfile(typeof(TProfile));
		}

		public IConfigurationTransaction RemoveProfile(Type profileType)
		{
			if (profileType != null) _profilesToRemove.Add(profileType);
			return this;
		}

		public IConfigurationTransaction RemoveProfile(IShellProfile profile)
		{
			if (profile != null) _exactProfilesToRemove.Add(profile);
			return this;
		}

		public IConfigurationTransaction RemoveTypeParser<TTarget>()
		{
			return RemoveTypeParser(typeof(TTarget));
		}

		public IConfigurationTransaction RemoveTypeParser(Type targetType)
		{
			if (targetType != null) _parsersToRemove.Add(targetType);
			return this;
		}

		public async Task ApplyAsync(CancellationToken cancellationToken = default)
		{
			while (true)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (!_session.IsBusy)
				{
					try
					{
						// Acquire an exclusive lock on the shell to prevent overlapping execution or other mutations
						_session.StartSession(TimeSpan.FromSeconds(30));
						break;
					}
					catch (InvalidOperationException)
					{
						// Catch race condition if another process locked it exactly between check and start
						continue;
					}
				}

				await Task.Yield();
			}

			try
			{
				// Request UI to close to ensure user isn't typing while we rebuild
				_controller.RequestClose();

				// 1. Remove Type Parsers
				foreach (Type type in _parsersToRemove)
				{
					_parserRegistry.TryRemoveParser(type);
				}

				// 2. Resolve and Remove Profiles
				var resolvedProfilesToRemove = new HashSet<IShellProfile>();

				foreach (Type type in _profilesToRemove)
				{
					IReadOnlyList<IShellProfile> matches = _commandRegistry.GetProfilesByType(type);
					foreach (IShellProfile match in matches)
					{
						resolvedProfilesToRemove.Add(match);
					}
				}

				foreach (IShellProfile profile in _exactProfilesToRemove)
				{
					resolvedProfilesToRemove.Add(profile);
				}

				foreach (IShellProfile profile in resolvedProfilesToRemove)
				{
					_commandRegistry.UnregisterProfile(profile);
				}

				// 3. Add New Type Parsers
				foreach (ITypeParser parser in _parsersToAdd)
				{
					_parserRegistry.Register(parser, forceOverride: true);
				}

				// 4. Add New Profiles
				foreach (IShellProfile profile in _profilesToAdd)
				{
					var commandBuilder = new ShellCommandBuilder();
					profile.RegisterCommands(commandBuilder);

					IReadOnlyList<CommandSignature> signatures = commandBuilder.BuildAll(_activeEnvironment);
					_commandRegistry.RegisterProfile(profile, signatures);
				}
			}
			finally
			{
				_session.EndSession(); // Release the lock
			}
		}
	}
}