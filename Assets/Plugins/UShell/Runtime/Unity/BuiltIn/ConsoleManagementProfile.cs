#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Commands.Fluent;
using UShell.Runtime.Core.History;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Core.Output.Formatting;

namespace UShell.Runtime.Unity.BuiltIn
{
	public sealed class ConsoleManagementProfile : ShellProfile
	{
		public static event Action? OnClearRequested;
		public static event Action? OnCloseRequested;

		private readonly ICommandRegistry _registry;
		private readonly ICommandHistory _history;

		public ConsoleManagementProfile(
			IConsolePrinter printer,
			ICommandRegistry registry,
			ICommandHistory history)
			: base(printer)
		{
			_registry = registry ?? throw new ArgumentNullException(nameof(registry));
			_history = history ?? throw new ArgumentNullException(nameof(history));
		}

		protected override void Configure(ICommandBuilder builder)
		{
			builder.WithName("help")
				.WithDescription("Lists all registered commands. Pass a command name to view detailed documentation.")
				.WithAlias("?")
				.AddOptionalParameter<string>("command", string.Empty)
				.Executes<string>(ShowHelp);

			builder.WithName("clear")
				.WithDescription("Clears all log entries from the console output window.")
				.WithAlias("cls")
				.Executes(() => OnClearRequested?.Invoke());

			builder.WithName("close")
				.WithDescription("Hides (closes) the console window.")
				.WithAlias("hide")
				.Executes(() => OnCloseRequested?.Invoke());

			builder.WithName("echo")
				.WithDescription("Prints the given text to the console.")
				.AddParameter<string>("message")
				.Executes<string>(Print);

			builder.WithName("history")
				.WithDescription("Shows the command input history for this session (newest last).")
				.WithAlias("hist")
				.AddOptionalParameter<int>("count", 20)
				.Executes<int>(ShowHistory);

			builder.WithName("history.clear")
				.WithDescription("Wipes the entire command input history for this session.")
				.WithAlias("hc")
				.Executes(() =>
				{
					_history.Clear();
					PrintSuccess("Command history cleared.");
				});

			builder.WithName("alias")
				.WithDescription("Lists all registered command aliases and their canonical command names.")
				.Executes(ShowAliases);
		}

		private void ShowHelp(string commandFilter)
		{
			if (!string.IsNullOrWhiteSpace(commandFilter))
			{
				ShowCommandDetail(commandFilter.Trim());
				return;
			}
			ShowAllCommands();
		}

		private void ShowCommandDetail(string name)
		{
			if (!_registry.TryGetCommand(name, out CommandSignature sig))
			{
				PrintWarning($"No command '{name}' found. Type 'help' to see all commands.");
				return;
			}

			var sb = new StringBuilder();
			sb.AppendLine(ProfileFormatter.FormatSectionHeader(sig.Name));
			sb.AppendLine();
			sb.Append(ProfileFormatter.Indent(1));
			sb.AppendLine(RichText.Color(sig.Description, ShellPalette.TextSecondary));

			if (sig.Aliases.Count > 0)
			{
				sb.AppendLine();
				sb.Append(ProfileFormatter.Indent(1));
				sb.Append(RichText.Color("Aliases  ", ShellPalette.TextMuted));
				sb.Append(RichText.Color("  ", ShellPalette.TextDim));
				string aliasLine = string.Join(RichText.Color(", ", ShellPalette.TextDim),
					sig.Aliases.Select(a => RichText.Color(a, ShellPalette.SyntaxAlias)));
				sb.AppendLine(aliasLine);
			}

			sb.Append(ProfileFormatter.Indent(1));
			sb.Append(RichText.Color("Env tags ", ShellPalette.TextMuted));
			sb.Append("  ");
			sb.AppendLine(ProfileFormatter.FormatEnvironmentTags(sig.Tags));

			sb.Append(ProfileFormatter.Indent(1));
			sb.Append(RichText.Color("Usage    ", ShellPalette.TextMuted));
			sb.Append("  ");
			sb.AppendLine(ProfileFormatter.FormatUsageSignature(sig, int.MaxValue));

			if (sig.Parameters.Count > 0)
			{
				sb.AppendLine();
				sb.Append(ProfileFormatter.Indent(1));
				sb.AppendLine(RichText.Bold(RichText.Color("Parameters:", ShellPalette.TableHeader)));

				foreach (CommandParameter p in sig.Parameters)
				{
					string type = ProfileFormatter.FriendlyTypeName(p.ParameterType);
					string req = p.IsOptional
						? RichText.Color($"optional, default = {p.DefaultValue ?? "null"}", ShellPalette.Optional)
						: RichText.Color("required", ShellPalette.Required);

					sb.Append(ProfileFormatter.Indent(2));
					sb.Append(RichText.Color($"-{p.Name}", ShellPalette.SyntaxParam));
					sb.Append("  ");
					sb.Append(RichText.Color($"<{type}>", ShellPalette.SyntaxType));
					sb.Append("  ");
					sb.AppendLine(req);
				}
			}

			sb.AppendLine();
			sb.Append(ProfileFormatter.Indent(1));
			sb.Append(RichText.Italic(RichText.Color($"Tip: type '{sig.Name} [Tab]' for autocomplete.", ShellPalette.TextDim)));

			Print(sb.ToString().TrimEnd());
		}

		private void ShowAllCommands()
		{
			var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var rows = new List<IReadOnlyList<string>>();

			foreach (CommandSignature sig in _registry.GetAllCommands())
			{
				if (!seen.Add(sig.Name)) continue;

				string aliases = sig.Aliases.Count > 0
					? string.Join(", ", sig.Aliases)
					: RichText.Color("-", ShellPalette.TextMuted);

				string wrappedDesc = ProfileFormatter.WrapText(sig.Description, 60);
				string colorizedDesc = string.Join("\n", wrappedDesc.Split('\n').Select(l => RichText.Color(l, ShellPalette.TextSecondary)));

				rows.Add(new[]
				{
					RichText.Color(sig.Name, ShellPalette.SyntaxCommand),
					RichText.Color(aliases,  ShellPalette.SyntaxAlias),
					ProfileFormatter.FormatUsageSignature(sig, 70),
					colorizedDesc
				});
			}

			rows.Sort(static (a, b) => string.Compare(RichTextStripper.Strip(a[0]), RichTextStripper.Strip(b[0]), StringComparison.OrdinalIgnoreCase));

			Print(RichText.Bold(RichText.Color($"  ── Available Commands ({rows.Count}) ─────────────────────────", ShellPalette.HeaderRule)));

			var headers = new List<string>
			{
				RichText.Bold(RichText.Color("Command", ShellPalette.TableHeader)),
				RichText.Bold(RichText.Color("Aliases", ShellPalette.TableHeader)),
				RichText.Bold(RichText.Color("Signature", ShellPalette.TableHeader)),
				RichText.Bold(RichText.Color("Description", ShellPalette.TableHeader))
			};

			PrintTable(headers, rows, TableStyle.Grid);

			Print(RichText.Italic(RichText.Color("  Tip: type 'help <command>' for full parameter documentation.", ShellPalette.TextHint)));
		}

		private void ShowHistory(int count)
		{
			IReadOnlyList<string> entries = _history.Entries;

			if (entries.Count == 0)
			{
				PrintWarning("Command history is empty.");
				return;
			}

			int total = entries.Count;
			int start = Math.Max(0, total - Math.Abs(count));
			var sb = new StringBuilder();

			sb.AppendLine(ProfileFormatter.FormatSectionHeader("command history"));

			for (int i = start; i < total; i++)
			{
				string idx = RichText.Color($"  {i + 1,3}  ", ShellPalette.TextDim);
				string cmd = RichText.Color(entries[i], ShellPalette.TextPrimary);
				sb.AppendLine(idx + cmd);
			}

			if (total > count)
			{
				sb.Append(RichText.Color($"  … showing last {count} of {total} entries. Use 'history -count {total}' for all.", ShellPalette.TextHint));
			}

			Print(sb.ToString().TrimEnd());
		}

		private void ShowAliases()
		{
			var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var rows = new List<IReadOnlyList<string>>();

			foreach (CommandSignature sig in _registry.GetAllCommands())
			{
				if (!seen.Add(sig.Name)) continue;
				if (sig.Aliases.Count == 0) continue;

				foreach (string alias in sig.Aliases)
				{
					rows.Add(new[]
					{
						RichText.Color(alias, ShellPalette.SyntaxAlias),
						RichText.Color("→", ShellPalette.TextDim),
						RichText.Color(sig.Name, ShellPalette.SyntaxCommand)
					});
				}
			}

			if (rows.Count == 0)
			{
				PrintWarning("No aliases are currently registered.");
				return;
			}

			rows.Sort(static (a, b) => string.Compare(RichTextStripper.Strip(a[0]), RichTextStripper.Strip(b[0]), StringComparison.OrdinalIgnoreCase));

			Print(ProfileFormatter.FormatSectionHeader($"aliases ({rows.Count})"));

			var headers = new[]
			{
				RichText.Bold(RichText.Color("Alias", ShellPalette.TableHeader)),
				"",
				RichText.Bold(RichText.Color("Command", ShellPalette.TableHeader))
			};

			PrintTable(headers, rows, TableStyle.Standard);
		}
	}
}