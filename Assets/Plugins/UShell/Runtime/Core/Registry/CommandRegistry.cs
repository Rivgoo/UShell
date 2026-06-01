#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Exceptions;
using UShell.Runtime.Core.Output.Formatting;
using UShell.Runtime.Core.Parsing.Lexing;
using UShell.Runtime.Core.Suggestions;

namespace UShell.Runtime.Core.Registry
{
	/// <summary>
	/// The core implementation of <see cref="ICommandRegistry"/> mapping string keys to command signatures.
	/// </summary>
	/// <remarks>
	/// This class acts as the source of truth for the active environment. It provides highly-optimized 
	/// fuzzy-matching lookups for generating suggestions. Supports dynamic runtime registration.
	/// </remarks>
	public sealed class CommandRegistry : ICommandRegistry
	{
		private readonly Dictionary<string, CommandSignature> _commands = new(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<CommandSignature, string> _signatureCache = new();
		private readonly Dictionary<IShellProfile, List<CommandSignature>> _profileSignatures = new();

		/// <summary>
		/// Initializes a new empty command registry.
		/// </summary>
		public CommandRegistry()
		{
		}

		/// <inheritdoc/>
		public bool TryGetCommand(string name, out CommandSignature signature)
		{
			return _commands.TryGetValue(name, out signature!);
		}

		/// <inheritdoc/>
		public IReadOnlyCollection<CommandSignature> GetAllCommands() => _commands.Values;

		/// <inheritdoc/>
		public string GetCompactSignature(CommandSignature signature)
		{
			if (!_signatureCache.TryGetValue(signature, out string compact))
			{
				compact = ProfileFormatter.FormatCompactSignature(signature);
				_signatureCache[signature] = compact;
			}
			return compact;
		}

		/// <summary>
		/// Dynamically registers a profile and maps its commands into the active registry.
		/// </summary>
		/// <param name="profile">The profile instance defining the commands.</param>
		/// <param name="signatures">The compiled signatures for this profile.</param>
		/// <exception cref="DuplicateCommandException">Thrown if any command name or alias already exists in the registry.</exception>
		public void RegisterProfile(IShellProfile profile, IReadOnlyList<CommandSignature> signatures)
		{
			if (profile == null) throw new ArgumentNullException(nameof(profile));
			if (signatures == null) throw new ArgumentNullException(nameof(signatures));

			if (_profileSignatures.ContainsKey(profile)) return;

			foreach (CommandSignature sig in signatures)
			{
				if (_commands.ContainsKey(sig.Name)) throw new DuplicateCommandException(sig.Name);

				foreach (string alias in sig.Aliases)
				{
					if (_commands.ContainsKey(alias)) throw new DuplicateCommandException(alias);
				}
			}

			_profileSignatures[profile] = new List<CommandSignature>(signatures);

			foreach (CommandSignature sig in signatures)
			{
				_commands[sig.Name] = sig;
				foreach (string alias in sig.Aliases)
				{
					_commands[alias] = sig;
				}
			}

			_signatureCache.Clear();
		}

		/// <summary>
		/// Removes a profile and all of its associated commands and aliases from the registry.
		/// </summary>
		/// <param name="profile">The profile to remove.</param>
		/// <returns><c>true</c> if the profile was found and removed; otherwise, <c>false</c>.</returns>
		public bool UnregisterProfile(IShellProfile profile)
		{
			if (profile == null) throw new ArgumentNullException(nameof(profile));

			if (!_profileSignatures.TryGetValue(profile, out List<CommandSignature>? signatures))
			{
				return false;
			}

			foreach (CommandSignature sig in signatures)
			{
				_commands.Remove(sig.Name);
				foreach (string alias in sig.Aliases)
				{
					_commands.Remove(alias);
				}
			}

			_profileSignatures.Remove(profile);
			_signatureCache.Clear();
			return true;
		}

		/// <summary>
		/// Retrieves all tracked profile instances matching the specified type.
		/// </summary>
		public IReadOnlyList<IShellProfile> GetProfilesByType(Type type)
		{
			var list = new List<IShellProfile>();
			foreach (IShellProfile profile in _profileSignatures.Keys)
			{
				if (type.IsAssignableFrom(profile.GetType()))
				{
					list.Add(profile);
				}
			}
			return list;
		}

		/// <inheritdoc/>
		public IReadOnlyList<CommandSuggestion> GetSuggestions(string input)
		{
			if (string.IsNullOrWhiteSpace(input)) return Array.Empty<CommandSuggestion>();

			var tokens = new List<Token>();
			var lexer = new Lexer(input.AsSpan());

			while (true)
			{
				var t = lexer.GetNextToken();
				if (t.Type == TokenType.EndOfFile) break;
				if (t.Type == TokenType.Unknown) continue;
				tokens.Add(t);
			}

			if (tokens.Count == 0) return Array.Empty<CommandSuggestion>();

			bool endsWithSpace = char.IsWhiteSpace(input[input.Length - 1]);

			if (tokens.Count == 1 && !endsWithSpace)
			{
				return FuzzySearchCommands(input);
			}

			string cmdName = input.Substring(tokens[0].Position, tokens[0].Length);
			if (!_commands.TryGetValue(cmdName, out var signature))
			{
				return Array.Empty<CommandSuggestion>();
			}

			int positionalIndex = 0;
			string? activeNamedParam = null;
			int argTokensStart = 1;
			int activeTokenIdx = endsWithSpace ? tokens.Count : tokens.Count - 1;

			for (int i = argTokensStart; i < activeTokenIdx; i++)
			{
				var t = tokens[i];
				if (t.Type == TokenType.Minus)
				{
					if (i + 1 < activeTokenIdx && tokens[i + 1].Type == TokenType.Identifier)
					{
						activeNamedParam = input.Substring(tokens[i + 1].Position, tokens[i + 1].Length);
						i++;
					}
					else
					{
						activeNamedParam = null;
					}
				}
				else if (t.Type == TokenType.Identifier || t.Type == TokenType.String || t.Type == TokenType.Number || t.Type == TokenType.Variable)
				{
					if (activeNamedParam != null)
					{
						activeNamedParam = null;
					}
					else
					{
						positionalIndex++;
					}
				}
			}

			if (!endsWithSpace)
			{
				var activeToken = tokens[activeTokenIdx];

				if (activeToken.Type == TokenType.Minus ||
				   (activeToken.Type == TokenType.Identifier && activeTokenIdx > 0 && tokens[activeTokenIdx - 1].Type == TokenType.Minus))
				{
					string partialName = activeToken.Type == TokenType.Identifier ? input.Substring(activeToken.Position, activeToken.Length) : "";
					int baseLen = activeToken.Type == TokenType.Identifier ? tokens[activeTokenIdx - 1].Position : activeToken.Position;
					string baseInput = input.Substring(0, baseLen);

					return FuzzySearchParameterNames(signature, partialName, baseInput);
				}

				string partialValue = input.Substring(activeToken.Position, activeToken.Length);

				if (activeToken.Type == TokenType.String && partialValue.Length > 0)
				{
					partialValue = partialValue.Trim('"');
				}

				return FuzzySearchParameterValues(signature, activeNamedParam, positionalIndex, partialValue, input.Substring(0, activeToken.Position));
			}
			else
			{
				return FuzzySearchParameterValues(signature, activeNamedParam, positionalIndex, "", input);
			}
		}

		private IReadOnlyList<CommandSuggestion> FuzzySearchCommands(string partialName)
		{
			var results = new List<CommandSuggestion>();
			foreach (var kvp in _commands)
			{
				int score = FuzzyMatcher.Score(partialName, kvp.Key);
				if (score >= 0)
				{
					results.Add(new CommandSuggestion(kvp.Key, kvp.Key, kvp.Value, "", score));
				}
			}
			results.Sort((a, b) => b.Score.CompareTo(a.Score));
			return results;
		}

		private IReadOnlyList<CommandSuggestion> FuzzySearchParameterNames(CommandSignature sig, string partialName, string baseInput)
		{
			var results = new List<CommandSuggestion>();
			foreach (var p in sig.Parameters)
			{
				int score = FuzzyMatcher.Score(partialName, p.Name);
				if (score >= 0)
				{
					results.Add(new CommandSuggestion(baseInput + "-" + p.Name, "-" + p.Name, null, "Parameter", score));
				}
			}
			results.Sort((a, b) => b.Score.CompareTo(a.Score));
			return results;
		}

		private IReadOnlyList<CommandSuggestion> FuzzySearchParameterValues(CommandSignature sig, string? namedParam, int posIndex, string partialVal, string baseInput)
		{
			CommandParameter? targetParam = null;

			if (namedParam != null)
			{
				targetParam = sig.Parameters.FirstOrDefault(p => p.Name.Equals(namedParam, StringComparison.OrdinalIgnoreCase));
			}
			else if (posIndex < sig.Parameters.Count)
			{
				targetParam = sig.Parameters[posIndex];
			}

			if (targetParam?.SuggestionProvider == null) return Array.Empty<CommandSuggestion>();

			var ctx = new SuggestionContext(partialVal);
			var rawSuggestions = targetParam.SuggestionProvider.GetSuggestions(ctx);

			var results = new List<CommandSuggestion>();

			foreach (var s in rawSuggestions)
			{
				int score = FuzzyMatcher.Score(partialVal, s);
				if (score >= 0)
				{
					string safeValue = s.Contains(' ') ? $"\"{s}\"" : s;
					results.Add(new CommandSuggestion(baseInput + safeValue, s, null, targetParam.Name, score));
				}
			}

			results.Sort((a, b) => b.Score.CompareTo(a.Score));
			return results;
		}
	}
}