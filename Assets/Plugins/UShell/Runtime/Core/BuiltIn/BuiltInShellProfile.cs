#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Commands.Fluent;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Core.Output.Formatting;

namespace UShell.Runtime.Core.BuiltIn
{
	/// <summary>
	/// Core built-in commands: help, clear, echo, env, version, exit, eval.
	/// </summary>
	public sealed class BuiltInShellProfile : IShellProfile
	{
		public static event Action? OnClearRequested;
		public static event Action? OnExitRequested;

		private readonly IConsolePrinter _printer;
		private readonly ICommandRegistry _registry;
		private readonly string _version;
		private readonly EnvironmentTag _environment;

		public BuiltInShellProfile(
			IConsolePrinter printer,
			ICommandRegistry registry,
			string version,
			EnvironmentTag environment)
		{
			_printer = printer ?? throw new ArgumentNullException(nameof(printer));
			_registry = registry ?? throw new ArgumentNullException(nameof(registry));
			_version = version;
			_environment = environment;
		}

		public void RegisterCommands(ICommandBuilder builder)
		{
			builder.WithName("help")
				.WithDescription("Lists all commands. Pass a command name for detailed parameter docs.")
				.WithAlias("?")
				.AddOptionalParameter<string>("command", string.Empty)
				.Executes<string>(ShowHelp);

			builder.WithName("echo")
				.WithDescription("Prints a message to the console.")
				.AddParameter<string>("message")
				.Executes<string>(Echo);

			builder.WithName("clear")
				.WithDescription("Clears the console log.")
				.WithAlias("cls")
				.Executes(() => OnClearRequested?.Invoke());

			builder.WithName("exit")
				.WithDescription("Closes the console window.")
				.WithAlias("quit")
				.Executes(() => OnExitRequested?.Invoke());

			builder.WithName("env")
				.WithDescription("Shows the active environment tag.")
				.Executes(() => _printer.Print(new LogEntry(
					$"Environment: {RichText.Color(_environment.ToString(), "#e0af68")}", LogType.Standard)));

			builder.WithName("version")
				.WithDescription("Shows UShell version information.")
				.WithAlias("ver")
				.Executes(() => _printer.Print(new LogEntry(
					$"UShell {RichText.Color(_version, "#7aa2f7")} — in-game developer console", LogType.Standard)));

			builder.WithName("eval")
				.WithDescription("Evaluates a math expression. E.g. eval \"(3+5)*2\"")
				.AddParameter<string>("expression")
				.Executes<string>(Eval);
		}

		private void ShowHelp(string commandFilter)
		{
			if (!string.IsNullOrWhiteSpace(commandFilter))
			{
				ShowCommandDetail(commandFilter);
				return;
			}
			ShowAllCommands();
		}

		private void ShowCommandDetail(string name)
		{
			if (!_registry.TryGetCommand(name, out CommandSignature sig))
			{
				_printer.Print(new LogEntry($"No command '{name}'. Type 'help' for the full list.", LogType.Warning));
				return;
			}

			var sb = new StringBuilder();
			sb.AppendLine(RichText.Bold(RichText.Color($"── {sig.Name} ──────────────────────", "#7aa2f7")));
			sb.AppendLine($"  {sig.Description}");

			if (sig.Aliases.Count > 0)
			{
				sb.AppendLine(RichText.Color($"  Aliases  : {string.Join(", ", sig.Aliases)}", "#6a7185"));
			}

			sb.AppendLine(RichText.Color($"  Env tags : {sig.Tags}", "#6a7185"));
			sb.AppendLine(RichText.Color($"  Usage    : {BuildUsage(sig)}", "#7dcfff"));

			if (sig.Parameters.Count > 0)
			{
				sb.AppendLine(RichText.Color("  Parameters:", "#bb9af7"));
				foreach (CommandParameter p in sig.Parameters)
				{
					string req = p.IsOptional ? $"optional, default = {p.DefaultValue ?? "null"}" : "required";
					string type = FriendlyType(p.ParameterType);
					sb.AppendLine($"    {RichText.Color($"-{p.Name}", "#9ece6a")} " +
								  $"{RichText.Color($"<{type}>", "#e0af68")}  {req}");
				}
			}

			_printer.Print(new LogEntry(sb.ToString().TrimEnd(), LogType.Standard));
		}

		private void ShowAllCommands()
		{
			var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var headers = new List<string> { "Command", "Aliases", "Signature", "Description" };
			var rows = new List<IReadOnlyList<string>>();

			foreach (CommandSignature sig in _registry.GetAllCommands())
			{
				if (!seen.Add(sig.Name)) continue;

				rows.Add(new[]
				{
					sig.Name,
					sig.Aliases.Count > 0 ? string.Join(", ", sig.Aliases) : "-",
					BuildUsage(sig),
					sig.Description
				});
			}

			rows.Sort(static (a, b) => string.Compare(a[0], b[0], StringComparison.OrdinalIgnoreCase));

			_printer.Print(new LogEntry(
				RichText.Bold(RichText.Color("Available Commands", "#7aa2f7")), LogType.Standard));
			_printer.PrintTable(headers, rows);
			_printer.Print(new LogEntry(
				RichText.Color("  Tip: type 'help <command>' for full parameter documentation.", "#6a7185"),
				LogType.Standard));
		}

		private void Echo(string message)
		{
			_printer.Print(new LogEntry(message, LogType.Standard));
		}

		private void Eval(string expression)
		{
			var result = SimpleExpressionEvaluator.Evaluate(expression);

			_printer.Print(result.IsSuccess
				? new LogEntry($"= {result.Value:G}", LogType.Success)
				: new LogEntry(result.Error!.Value.Message, LogType.Error));
		}

		private static string BuildUsage(CommandSignature sig)
		{
			var sb = new StringBuilder(sig.Name);
			foreach (CommandParameter p in sig.Parameters)
			{
				string typePart = $":{FriendlyType(p.ParameterType)}";
				sb.Append(p.IsOptional ? $" [-{p.Name}{typePart}]" : $" <{p.Name}{typePart}>");
			}
			return sb.ToString();
		}

		private static string FriendlyType(System.Type t)
		{
			if (t == typeof(int)) return "int";
			if (t == typeof(float)) return "float";
			if (t == typeof(bool)) return "bool";
			if (t == typeof(string)) return "string";
			if (t == typeof(int[])) return "int[]";
			if (t == typeof(float[])) return "float[]";
			if (t == typeof(string[])) return "string[]";
			return t.Name;
		}
	}
}