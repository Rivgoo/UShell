#nullable enable
using System;
using System.Collections.Generic;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands;

namespace UShell.Runtime.Core.Registry
{
	/// <summary>
	/// A deferred proxy wrapper for the <see cref="ICommandRegistry"/> used during bootstrapping.
	/// </summary>
	/// <remarks>
	/// Commands like <c>help</c> require access to the registry to list commands. However, the registry 
	/// cannot be built until all profiles (including the one containing <c>help</c>) are parsed. 
	/// This proxy breaks the circular dependency by allowing profiles to hold a reference that is resolved later.
	/// </remarks>
	public sealed class RegistryProxy : ICommandRegistry
	{
		/// <summary>
		/// The underlying registry instance, assigned dynamically once bootstrapping completes.
		/// </summary>
		public ICommandRegistry? Target { get; set; }

		/// <inheritdoc/>
		public IReadOnlyCollection<CommandSignature> GetAllCommands()
		{
			return Target?.GetAllCommands() ?? Array.Empty<CommandSignature>();
		}

		/// <inheritdoc/>
		public IReadOnlyList<CommandSuggestion> GetSuggestions(string input)
		{
			return Target?.GetSuggestions(input) ?? Array.Empty<CommandSuggestion>();
		}

		/// <inheritdoc/>
		public bool TryGetCommand(string name, out CommandSignature signature)
		{
			if (Target != null)
			{
				return Target.TryGetCommand(name, out signature);
			}

			signature = null!;
			return false;
		}

		/// <inheritdoc/>
		public string GetCompactSignature(CommandSignature signature)
		{
			return Target?.GetCompactSignature(signature) ?? string.Empty;
		}
	}
}