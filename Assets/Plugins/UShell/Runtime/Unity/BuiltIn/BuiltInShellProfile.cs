#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Commands.Fluent;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Core.Output.Formatting;

namespace UShell.Runtime.Unity.BuiltIn
{
	public sealed class BuiltInShellProfile : IShellProfile
	{
		public static event Action? OnClearRequested;
		public static event Action? OnCloseRequested;
		public static event Action? OnHistoryClearRequested;

		private const string UShellVersion = "1.0.0";

		private readonly IConsolePrinter _printer;
		private readonly ICommandRegistry _registry;
		private readonly string _gameVersion;
		private readonly EnvironmentTag _environment;

		private readonly Func<(float fps, float deltaMs, float unscaledDeltaMs)>? _getFrameStats;
		private readonly Func<(float reserved, float allocated, float monoHeap, float monoUsed, int gcCount)>? _getMemStats;
		private readonly Action? _forceGC;
		private readonly Func<(float scale, float time, float realtime, int frame)>? _getTimeInfo;
		private readonly Func<bool>? _isPaused;
		private readonly Func<int>? _getObjectCount;
		private readonly Func<string, IReadOnlyList<string>>? _findByTag;
		private readonly Func<string, IReadOnlyList<string>>? _findByLayer;
		private readonly Func<bool>? _getMirrorToUnity;
		private readonly Action<bool>? _setMirrorToUnity;
		private readonly Action? _clearPrefs;
		private readonly Func<string, string?>? _getPrefs;
		private readonly Action<string, string>? _setPrefs;
		private readonly Func<(bool locked, bool visible)>? _getCursorState;
		private readonly Action<bool, bool>? _setCursorState;
		private readonly Func<string>? _getPlatform;
		private readonly Func<string>? _getBuildGuid;
		private readonly Func<IReadOnlyList<string>>? _getCommandHistory;

		public BuiltInShellProfile(
			IConsolePrinter printer,
			ICommandRegistry registry,
			string gameVersion,
			EnvironmentTag environment,
			Func<(float fps, float deltaMs, float unscaledDeltaMs)>? getFrameStats,
			Func<(float reserved, float allocated, float monoHeap, float monoUsed, int gcCount)>? getMemStats,
			Action? forceGC,
			Func<(float scale, float time, float realtime, int frame)>? getTimeInfo,
			Func<bool>? isPaused,
			Func<int>? getObjectCount,
			Func<string, IReadOnlyList<string>>? findByTag,
			Func<string, IReadOnlyList<string>>? findByLayer,
			Func<bool>? getMirrorToUnity,
			Action<bool>? setMirrorToUnity,
			Action? clearPrefs,
			Func<string, string?>? getPrefs,
			Action<string, string>? setPrefs,
			Func<(bool locked, bool visible)>? getCursorState,
			Action<bool, bool>? setCursorState,
			Func<string>? getPlatform,
			Func<string>? getBuildGuid,
			Func<IReadOnlyList<string>>? getCommandHistory)
		{
			_printer = printer ?? throw new ArgumentNullException(nameof(printer));
			_registry = registry ?? throw new ArgumentNullException(nameof(registry));
			_gameVersion = gameVersion ?? "Unknown";
			_environment = environment;

			_getFrameStats = getFrameStats;
			_getMemStats = getMemStats;
			_forceGC = forceGC;
			_getTimeInfo = getTimeInfo;
			_isPaused = isPaused;
			_getObjectCount = getObjectCount;
			_findByTag = findByTag;
			_findByLayer = findByLayer;
			_getMirrorToUnity = getMirrorToUnity;
			_setMirrorToUnity = setMirrorToUnity;
			_clearPrefs = clearPrefs;
			_getPrefs = getPrefs;
			_setPrefs = setPrefs;
			_getCursorState = getCursorState;
			_setCursorState = setCursorState;
			_getPlatform = getPlatform;
			_getBuildGuid = getBuildGuid;
			_getCommandHistory = getCommandHistory;
		}

		public void RegisterCommands(ICommandBuilder builder)
		{
			RegisterConsoleManagement(builder);
			RegisterEnvironmentInfo(builder);
			RegisterMathUtility(builder);
			RegisterRuntimeDiagnostics(builder);
		}

		private void RegisterConsoleManagement(ICommandBuilder builder)
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
				.Executes<string>(msg => Print(msg));

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
					OnHistoryClearRequested?.Invoke();
					PrintSuccess("Command history cleared.");
				});

			builder.WithName("alias")
				.WithDescription("Lists all registered command aliases and their canonical command names.")
				.Executes(ShowAliases);
		}

		private void RegisterEnvironmentInfo(ICommandBuilder builder)
		{
			builder.WithName("info")
				.WithDescription("Displays a comprehensive summary of the current build, environment, platform, and runtime.")
				.WithAlias("sysinfo")
				.Executes(ShowFullInfo);

			builder.WithName("env")
				.WithDescription("Shows the active UShell environment tag.")
				.Executes(ShowEnvironment);

			builder.WithName("game.version")
				.WithDescription("Displays the application/game version string.")
				.WithAlias("gver")
				.Executes(ShowGameVersion);

			builder.WithName("shell.version")
				.WithDescription($"Displays the UShell package version (current: {UShellVersion}).")
				.WithAlias("sver")
				.Executes(ShowShellVersion);

			builder.WithName("platform")
				.WithDescription("Shows the current runtime platform.")
				.Executes(ShowPlatform);

			builder.WithName("buildguid")
				.WithDescription("Shows the unique GUID assigned to this build by Unity.")
				.RestrictedTo(EnvironmentTag.Editor | EnvironmentTag.Development)
				.Executes(ShowBuildGuid);
		}

		private void RegisterMathUtility(ICommandBuilder builder)
		{
			builder.WithName("eval")
				.WithDescription("Evaluates a mathematical expression and prints the result.")
				.WithAlias("calc")
				.AddParameter<string>("expression")
				.Executes<string>(Eval);

			builder.WithName("random")
				.WithDescription("Generates a random integer in the inclusive range [min, max].")
				.WithAlias("rand")
				.AddOptionalParameter<int>("min", 0)
				.AddOptionalParameter<int>("max", 100)
				.Executes<int, int>(Random);

			builder.WithName("convert")
				.WithDescription("Converts a value between common units or numeric bases.")
				.AddParameter<float>("value")
				.AddParameter<string>("from")
				.AddParameter<string>("to")
				.Executes<float, string, string>(Convert);
		}

		private void RegisterRuntimeDiagnostics(ICommandBuilder builder)
		{
			builder.WithName("stats")
				.WithDescription("Prints a snapshot of key runtime metrics: FPS, frame time, and memory usage.")
				.Executes(ShowStats);

			builder.WithName("mem")
				.WithDescription("Shows a detailed memory report: reserved, allocated, Mono heap/used, and GC collection count.")
				.Executes(ShowMemory);

			builder.WithName("gc")
				.WithDescription("Forces a garbage collection pass (GC.Collect).")
				.RestrictedTo(EnvironmentTag.Editor | EnvironmentTag.Development)
				.Executes(ForceGC);

			builder.WithName("time")
				.WithDescription("Shows the current time scale, elapsed game time, real time, and frame count.")
				.RestrictedTo(EnvironmentTag.Editor)
				.Executes(ShowTime);

			builder.WithName("objects")
				.WithDescription("Counts all active GameObjects currently present in the loaded scenes.")
				.RestrictedTo(EnvironmentTag.Editor)
				.Executes(ShowObjectCount);

			builder.WithName("tag.find")
				.WithDescription("Lists GameObjects in the current scene that have the specified tag.")
				.AddParameter<string>("tag")
				.AddOptionalParameter<int>("limit", 25)
				.RestrictedTo(EnvironmentTag.Editor)
				.Executes<string, int>(FindByTag);

			builder.WithName("layer.find")
				.WithDescription("Lists GameObjects in the current scene that belong to the specified layer name.")
				.AddParameter<string>("layer")
				.AddOptionalParameter<int>("limit", 25)
				.RestrictedTo(EnvironmentTag.Editor)
				.Executes<string, int>(FindByLayer);

			builder.WithName("log.unity")
				.WithDescription("Toggles (or sets) mirroring of UShell log output to the Unity Editor Console window.")
				.AddOptionalParameter<bool>("enabled", false)
				.RestrictedTo(EnvironmentTag.Editor)
				.Executes<bool>(SetUnityLogMirror);

			builder.WithName("prefs.clear")
				.WithDescription("Deletes ALL PlayerPrefs keys for this application.")
				.RestrictedTo(EnvironmentTag.Editor)
				.Executes(ClearPrefs);

			builder.WithName("prefs.get")
				.WithDescription("Reads and prints a string PlayerPrefs entry by key.")
				.AddParameter<string>("key")
				.RestrictedTo(EnvironmentTag.Editor)
				.Executes<string>(GetPrefs);

			builder.WithName("prefs.set")
				.WithDescription("Writes a string value to a PlayerPrefs key.")
				.AddParameter<string>("key")
				.AddParameter<string>("value")
				.RestrictedTo(EnvironmentTag.Editor)
				.Executes<string, string>(SetPrefs);

			builder.WithName("cursor")
				.WithDescription("Shows cursor state, or sets it (-locked true/false -visible true/false).")
				.AddOptionalParameter<bool>("locked", false)
				.AddOptionalParameter<bool>("visible", true)
				.RestrictedTo(EnvironmentTag.Editor)
				.Executes<bool, bool>(SetCursor);
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
			sb.AppendLine(SectionHeader(sig.Name));
			sb.AppendLine();
			sb.Append(Indent(1));
			sb.AppendLine(RichText.Color(sig.Description, ShellPalette.TextSecondary));

			if (sig.Aliases.Count > 0)
			{
				sb.AppendLine();
				sb.Append(Indent(1));
				sb.Append(RichText.Color("Aliases  ", ShellPalette.TextMuted));
				sb.Append(RichText.Color("  ", ShellPalette.TextDim));
				string aliasLine = string.Join(RichText.Color(", ", ShellPalette.TextDim),
					System.Linq.Enumerable.Select(sig.Aliases, a => RichText.Color(a, ShellPalette.SyntaxAlias)));
				sb.AppendLine(aliasLine);
			}

			sb.Append(Indent(1));
			sb.Append(RichText.Color("Env tags ", ShellPalette.TextMuted));
			sb.Append("  ");
			sb.AppendLine(FormatEnvTags(sig.Tags));

			sb.Append(Indent(1));
			sb.Append(RichText.Color("Usage    ", ShellPalette.TextMuted));
			sb.Append("  ");
			sb.AppendLine(FormatUsage(sig, int.MaxValue)); 

			if (sig.Parameters.Count > 0)
			{
				sb.AppendLine();
				sb.Append(Indent(1));
				sb.AppendLine(RichText.Bold(RichText.Color("Parameters:", ShellPalette.TableHeader)));

				foreach (CommandParameter p in sig.Parameters)
				{
					string type = FriendlyTypeName(p.ParameterType);
					string req = p.IsOptional
						? RichText.Color($"optional, default = {p.DefaultValue ?? "null"}", ShellPalette.Optional)
						: RichText.Color("required", ShellPalette.Required);

					sb.Append(Indent(2));
					sb.Append(RichText.Color($"-{p.Name}", ShellPalette.SyntaxParam));
					sb.Append("  ");
					sb.Append(RichText.Color($"<{type}>", ShellPalette.SyntaxType));
					sb.Append("  ");
					sb.AppendLine(req);
				}
			}

			sb.AppendLine();
			sb.Append(Indent(1));
			sb.Append(RichText.Italic(RichText.Color($"Tip: type '{sig.Name} [Tab]' for autocomplete.", ShellPalette.TextDim)));

			_printer.Print(new LogEntry(sb.ToString().TrimEnd(), LogType.Standard));
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
					: RichText.Color("-", ShellPalette.TextDim);

				string wrappedDesc = WrapText(sig.Description, 60);
				string colorizedDesc = string.Join("\n", wrappedDesc.Split('\n').Select(l => RichText.Color(l, ShellPalette.TextSecondary)));

				rows.Add(new[]
				{
					RichText.Color(sig.Name, ShellPalette.SyntaxCommand),
					RichText.Color(aliases,  ShellPalette.SyntaxAlias),
					FormatUsage(sig, 70),
					colorizedDesc
				});
			}

			rows.Sort(static (a, b) => string.Compare(RichTextStripper.Strip(a[0]), RichTextStripper.Strip(b[0]), StringComparison.OrdinalIgnoreCase));

			Print(RichText.Bold(RichText.Color($"  ── Available Commands ({rows.Count}) ─────────────────────────", ShellPalette.HeaderRule)));

			var headers = new List<string>
			{
				RichText.Bold(RichText.Color("Command", ShellPalette.TableHeader)),
				RichText.Bold(RichText.Color("Aliases  ", ShellPalette.TableHeader)),
				RichText.Bold(RichText.Color("Signature", ShellPalette.TableHeader)),
				RichText.Bold(RichText.Color("Description", ShellPalette.TableHeader))
			};

			_printer.PrintTable(headers, rows, TableStyle.Grid);

			Print(RichText.Italic(RichText.Color("  Tip: type 'help <command>' for full parameter documentation.", ShellPalette.TextHint)));
		}

		private void ShowHistory(int count)
		{
			IReadOnlyList<string>? history = _getCommandHistory?.Invoke();

			if (history == null || history.Count == 0)
			{
				PrintWarning("Command history is empty.");
				return;
			}

			int total = history.Count;
			int start = Math.Max(0, total - Math.Abs(count));
			var sb = new StringBuilder();

			sb.AppendLine(SectionHeader("command history"));

			for (int i = start; i < total; i++)
			{
				string idx = RichText.Color($"  {i + 1,3}  ", ShellPalette.TextDim);
				string cmd = RichText.Color(history[i], ShellPalette.TextPrimary);
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

			Print(SectionHeader($"aliases ({rows.Count})"));

			_printer.PrintTable(
				new[] { RichText.Bold(RichText.Color("Alias", ShellPalette.TableHeader)), "", RichText.Bold(RichText.Color("Command", ShellPalette.TableHeader)) },
				rows, TableStyle.Standard);
		}

		private void ShowFullInfo()
		{
			var sb = new StringBuilder();
			sb.AppendLine(SectionHeader("system info"));
			sb.AppendLine();

			AppendKV(sb, "Game Version", _gameVersion);
			AppendKV(sb, "UShell Version", UShellVersion);

			string envLabel = _environment.ToString();
			AppendKV(sb, "Environment", RichText.Color(envLabel, ShellPalette.EnvironmentTagColor(envLabel)));

			string platform = _getPlatform?.Invoke() ?? "n/a (Unity callbacks not wired)";
			AppendKV(sb, "Platform", platform);

			string guid = _getBuildGuid?.Invoke() ?? "n/a";
			AppendKV(sb, "Build GUID", RichText.Color(guid, ShellPalette.TextTertiary));

			if (_getTimeInfo != null)
			{
				var t = _getTimeInfo();
				sb.AppendLine();
				AppendKV(sb, "Time Scale", FormatStat(t.scale.ToString("F2"), t.scale < 0.1f ? ShellPalette.StatWarn : ShellPalette.StatGood));
				AppendKV(sb, "Game Time", $"{t.time:F1} s");
				AppendKV(sb, "Real Time", $"{t.realtime:F1} s");
				AppendKV(sb, "Frame Count", t.frame.ToString());
			}

			if (_getMemStats != null)
			{
				var m = _getMemStats();
				sb.AppendLine();
				AppendKV(sb, "Reserved Mem", $"{m.reserved:F1} MB");
				AppendKV(sb, "Allocated Mem", FormatStat($"{m.allocated:F1} MB", ShellPalette.MetricColor(m.allocated, 200f, 512f)));
				AppendKV(sb, "Mono Heap", $"{m.monoHeap:F1} MB");
				AppendKV(sb, "Mono Used", $"{m.monoUsed:F1} MB");
				AppendKV(sb, "GC Collections", m.gcCount.ToString());
			}

			if (_getFrameStats != null)
			{
				var f = _getFrameStats();
				sb.AppendLine();
				AppendKV(sb, "FPS", FormatStat($"{f.fps:F1}", ShellPalette.MetricColorInverted(f.fps, 55f, 25f)));
				AppendKV(sb, "Delta", $"{f.deltaMs:F2} ms");
				AppendKV(sb, "Unscaled Δ", $"{f.unscaledDeltaMs:F2} ms");
			}

			Print(sb.ToString().TrimEnd());
		}

		private void ShowEnvironment()
		{
			string label = _environment.ToString();
			string color = ShellPalette.EnvironmentTagColor(label);
			Print($"Active environment:  {RichText.Bold(RichText.Color(label, color))}");
		}

		private void ShowGameVersion()
		{
			Print($"Game version:  {RichText.Bold(RichText.Color(_gameVersion, ShellPalette.AccentBright))}");
		}

		private void ShowShellVersion()
		{
			Print($"UShell version:  {RichText.Bold(RichText.Color(UShellVersion, ShellPalette.SyntaxCommand))}  " +
				  RichText.Italic(RichText.Color("— in-game developer console", ShellPalette.TextMuted)));
		}

		private void ShowPlatform()
		{
			if (_getPlatform == null)
			{
				PrintCallbackUnavailable("platform");
				return;
			}
			Print($"Platform:  {RichText.Color(_getPlatform(), ShellPalette.TextPrimary)}");
		}

		private void ShowBuildGuid()
		{
			if (_getBuildGuid == null)
			{
				PrintCallbackUnavailable("buildguid");
				return;
			}
			string guid = _getBuildGuid();
			Print($"Build GUID:  {RichText.Color(guid, ShellPalette.TextTertiary)}");
		}

		private void Eval(string expression)
		{
			var result = SimpleExpressionEvaluator.EvaluateWithFormat(expression, out SimpleExpressionEvaluator.FormatResultKind format);

			if (!result.IsSuccess)
			{
				PrintError(result.Error!.Value.Message);
				return;
			}

			string formatted = SimpleExpressionEvaluator.FormatResult(result.Value, format);
			string display = RichText.Bold(RichText.Color(formatted, ShellPalette.SyntaxNumber));
			PrintSuccess($"= {display}");
		}

		private void Random(int min, int max)
		{
			if (min > max) (min, max) = (max, min);

			int val = new System.Random().Next(min, max + 1);
			string r = RichText.Bold(RichText.Color(val.ToString(), ShellPalette.SyntaxNumber));
			PrintSuccess($"Random [{min}, {max}]  →  {r}");
		}

		private void Convert(float value, string from, string to)
		{
			string fromL = from.ToLowerInvariant();
			string toL = to.ToLowerInvariant();

			if (TryConvertUnit(value, fromL, toL, out double result, out string unit))
			{
				string rv = RichText.Bold(RichText.Color(SimpleExpressionEvaluator.FormatResult(result), ShellPalette.SyntaxNumber));
				string uv = RichText.Color(unit, ShellPalette.StatUnit);
				PrintSuccess($"{value} {from}  →  {rv} {uv}");
			}
			else
			{
				PrintError($"Cannot convert from '{from}' to '{to}'. Supported conversions: " +
						   "deg↔rad, km↔m↔cm↔mm↔mi↔ft↔in, kg↔g↔lb, " +
						   "bytes↔kb↔mb↔gb, dec↔hex↔bin.");
			}
		}

		private static bool TryConvertUnit(float value, string from, string to, out double result, out string unit)
		{
			result = 0;
			unit = to;

			if (from == "deg" && to == "rad") { result = value * (Math.PI / 180.0); return true; }
			if (from == "rad" && to == "deg") { result = value * (180.0 / Math.PI); return true; }

			if (TryToMetres(from, value, out double metres) && TryFromMetres(to, metres, out double converted)) { result = converted; return true; }
			if (TryToKg(from, value, out double kg) && TryFromKg(to, kg, out double convertedKg)) { result = convertedKg; return true; }
			if (TryToBytes(from, value, out double bytes) && TryFromBytes(to, bytes, out double convertedBytes)) { result = convertedBytes; return true; }

			long intVal = (long)Math.Round(value);
			if (from == "dec" && to == "hex") { result = intVal; unit = $"0x{intVal:X}"; return true; }
			if (from == "dec" && to == "bin") { result = intVal; unit = $"0b{System.Convert.ToString(intVal, 2)}"; return true; }
			if (from == "hex" && to == "dec") { result = intVal; return true; }
			if (from == "bin" && to == "dec") { result = intVal; return true; }

			return false;
		}

		private static bool TryToMetres(string unit, double v, out double m)
		{
			m = unit switch
			{
				"km" => v * 1000.0, "m" => v, "cm" => v / 100.0, "mm" => v / 1000.0,
				"mi" => v * 1609.344, "ft" => v * 0.3048, "in" => v * 0.0254, _ => double.NaN
			};
			return !double.IsNaN(m);
		}

		private static bool TryFromMetres(string unit, double m, out double v)
		{
			v = unit switch
			{
				"km" => m / 1000.0, "m" => m, "cm" => m * 100.0, "mm" => m * 1000.0,
				"mi" => m / 1609.344, "ft" => m / 0.3048, "in" => m / 0.0254, _ => double.NaN
			};
			return !double.IsNaN(v);
		}

		private static bool TryToKg(string unit, double v, out double kg)
		{
			kg = unit switch { "kg" => v, "g" => v / 1000.0, "lb" => v * 0.45359237, _ => double.NaN };
			return !double.IsNaN(kg);
		}

		private static bool TryFromKg(string unit, double kg, out double v)
		{
			v = unit switch { "kg" => kg, "g" => kg * 1000.0, "lb" => kg / 0.45359237, _ => double.NaN };
			return !double.IsNaN(v);
		}

		private static bool TryToBytes(string unit, double v, out double b)
		{
			b = unit switch { "bytes" or "b" => v, "kb" => v * 1024.0, "mb" => v * 1024.0 * 1024.0, "gb" => v * 1024.0 * 1024.0 * 1024.0, _ => double.NaN };
			return !double.IsNaN(b);
		}

		private static bool TryFromBytes(string unit, double b, out double v)
		{
			v = unit switch { "bytes" or "b" => b, "kb" => b / 1024.0, "mb" => b / 1024.0 / 1024.0, "gb" => b / 1024.0 / 1024.0 / 1024.0, _ => double.NaN };
			return !double.IsNaN(v);
		}

		private void ShowStats()
		{
			bool hasFrame = _getFrameStats != null;
			bool hasMem = _getMemStats != null;

			if (!hasFrame && !hasMem)
			{
				PrintCallbackUnavailable("stats");
				return;
			}

			var sb = new StringBuilder();
			sb.AppendLine(SectionHeader("runtime stats"));
			sb.AppendLine();

			if (hasFrame)
			{
				var (fps, delta, unscaled) = _getFrameStats!();
				string fpsColor = ShellPalette.MetricColorInverted(fps, 55f, 25f);

				AppendKV(sb, "FPS", $"{RichText.Color($"{fps:F1}", fpsColor)}  {RichText.Color("fps", ShellPalette.StatUnit)}");
				AppendKV(sb, "Frame Time", $"{RichText.Color($"{delta:F2}", ShellPalette.MetricColor(delta, 16.7f, 33.3f))}  {RichText.Color("ms", ShellPalette.StatUnit)}");
				AppendKV(sb, "Unscaled Δt", $"{RichText.Color($"{unscaled:F2}", ShellPalette.StatLabel)}  {RichText.Color("ms", ShellPalette.StatUnit)}");
			}

			if (hasMem)
			{
				if (hasFrame) sb.AppendLine();

				var (reserved, allocated, monoHeap, monoUsed, gcCount) = _getMemStats!();
				AppendKV(sb, "Reserved", $"{RichText.Color($"{reserved:F1}", ShellPalette.StatLabel)}  {RichText.Color("MB", ShellPalette.StatUnit)}");
				AppendKV(sb, "Allocated", $"{RichText.Color($"{allocated:F1}", ShellPalette.MetricColor(allocated, 200f, 512f))}  {RichText.Color("MB", ShellPalette.StatUnit)}");
				AppendKV(sb, "Mono Heap", $"{RichText.Color($"{monoHeap:F1}", ShellPalette.StatLabel)}  {RichText.Color("MB", ShellPalette.StatUnit)}");
				AppendKV(sb, "Mono Used", $"{RichText.Color($"{monoUsed:F1}", ShellPalette.MetricColor(monoUsed, monoHeap * 0.6f, monoHeap * 0.9f))}  {RichText.Color("MB", ShellPalette.StatUnit)}");
				AppendKV(sb, "GC Count", gcCount.ToString());
			}

			Print(sb.ToString().TrimEnd());
		}

		private void ShowMemory()
		{
			if (_getMemStats == null)
			{
				PrintCallbackUnavailable("mem");
				return;
			}

			var (reserved, allocated, monoHeap, monoUsed, gcCount) = _getMemStats();
			float monoFragmentation = monoHeap > 0 ? (1f - monoUsed / monoHeap) * 100f : 0f;

			var sb = new StringBuilder();
			sb.AppendLine(SectionHeader("memory report"));
			sb.AppendLine();
			AppendKVWide(sb, "Total Reserved", $"{reserved:F2} MB", ShellPalette.StatLabel);
			AppendKVWide(sb, "Total Allocated", $"{allocated:F2} MB", ShellPalette.MetricColor(allocated, 200f, 512f));
			AppendKVWide(sb, "Unmanaged", $"{Math.Max(0, reserved - allocated):F2} MB", ShellPalette.StatLabel);
			sb.AppendLine();
			AppendKVWide(sb, "Mono Heap Size", $"{monoHeap:F2} MB", ShellPalette.StatLabel);
			AppendKVWide(sb, "Mono Used", $"{monoUsed:F2} MB", ShellPalette.MetricColor(monoUsed, monoHeap * 0.6f, monoHeap * 0.9f));
			AppendKVWide(sb, "Mono Free", $"{monoHeap - monoUsed:F2} MB", ShellPalette.StatGood);
			AppendKVWide(sb, "Fragmentation", $"{monoFragmentation:F1} %", ShellPalette.MetricColor(monoFragmentation, 30f, 60f));
			sb.AppendLine();
			AppendKVWide(sb, "GC Collections", gcCount.ToString(), ShellPalette.StatLabel);

			Print(sb.ToString().TrimEnd());
		}

		private void ForceGC()
		{
			if (_forceGC == null)
			{
				PrintCallbackUnavailable("gc");
				return;
			}
			Print(RichText.Color("Requesting GC collection…", ShellPalette.WarningMuted));
			_forceGC();
			PrintSuccess("GC.Collect() executed. Check 'mem' for updated heap statistics.");
		}

		private void ShowTime()
		{
			if (_getTimeInfo == null)
			{
				PrintCallbackUnavailable("time");
				return;
			}

			var (scale, time, realtime, frame) = _getTimeInfo();

			var sb = new StringBuilder();
			sb.AppendLine(SectionHeader("time info"));
			sb.AppendLine();
			AppendKV(sb, "Time Scale", FormatStat($"{scale:F4}", scale < 0.01f ? ShellPalette.StatCritical : scale < 0.5f ? ShellPalette.StatWarn : ShellPalette.StatGood));

			if (_isPaused != null)
				AppendKV(sb, "Paused", _isPaused() ? RichText.Color("yes", ShellPalette.Warning) : RichText.Color("no", ShellPalette.Success));

			AppendKV(sb, "Game Time", $"{time:F3} s");
			AppendKV(sb, "Real Time", $"{realtime:F3} s");
			AppendKV(sb, "Frame", frame.ToString());

			Print(sb.ToString().TrimEnd());
		}

		private void ShowObjectCount()
		{
			if (_getObjectCount == null)
			{
				PrintCallbackUnavailable("objects");
				return;
			}
			int count = _getObjectCount();
			Print($"Active GameObjects in scene:  {RichText.Bold(RichText.Color(count.ToString(), ShellPalette.SyntaxNumber))}");
		}

		private void FindByTag(string tag, int limit)
		{
			if (_findByTag == null)
			{
				PrintCallbackUnavailable("tag.find");
				return;
			}
			IReadOnlyList<string> names = _findByTag(tag);
			PrintObjectList($"GameObjects with tag '{tag}'", names, limit);
		}

		private void FindByLayer(string layer, int limit)
		{
			if (_findByLayer == null)
			{
				PrintCallbackUnavailable("layer.find");
				return;
			}
			IReadOnlyList<string> names = _findByLayer(layer);
			PrintObjectList($"GameObjects on layer '{layer}'", names, limit);
		}

		private void PrintObjectList(string title, IReadOnlyList<string> names, int limit)
		{
			if (names.Count == 0)
			{
				PrintWarning($"No results for: {title}.");
				return;
			}

			var sb = new StringBuilder();
			sb.AppendLine(SectionHeader(title));

			int shown = Math.Min(names.Count, limit);
			for (int i = 0; i < shown; i++)
			{
				sb.Append(RichText.Color($"  {i + 1,3}  ", ShellPalette.TextDim));
				sb.AppendLine(RichText.Color(names[i], ShellPalette.TextPrimary));
			}

			if (names.Count > limit)
			{
				sb.Append(RichText.Color($"  … and {names.Count - limit} more (use -limit {names.Count} to see all).", ShellPalette.TextHint));
			}

			Print(sb.ToString().TrimEnd());
		}

		private void SetUnityLogMirror(bool enabled)
		{
			if (_setMirrorToUnity == null || _getMirrorToUnity == null)
			{
				PrintCallbackUnavailable("log.unity");
				return;
			}

			_setMirrorToUnity(enabled);
			bool current = _getMirrorToUnity();
			if (current)
				PrintSuccess("Unity console mirroring: ENABLED.");
			else
				Print(RichText.Color("Unity console mirroring: DISABLED.", ShellPalette.TextSecondary));
		}

		private void ClearPrefs()
		{
			if (_clearPrefs == null)
			{
				PrintCallbackUnavailable("prefs.clear");
				return;
			}
			_clearPrefs();
			PrintSuccess("All PlayerPrefs cleared.");
		}

		private void GetPrefs(string key)
		{
			if (_getPrefs == null)
			{
				PrintCallbackUnavailable("prefs.get");
				return;
			}
			string? value = _getPrefs(key);
			if (value == null)
			{
				PrintWarning($"Key '{key}' not found in PlayerPrefs.");
				return;
			}
			Print($"{RichText.Color(key, ShellPalette.SyntaxParam)}  =  {RichText.Color(value, ShellPalette.SyntaxValue)}");
		}

		private void SetPrefs(string key, string value)
		{
			if (_setPrefs == null)
			{
				PrintCallbackUnavailable("prefs.set");
				return;
			}
			_setPrefs(key, value);
			PrintSuccess($"PlayerPrefs['{key}'] = '{value}'");
		}

		private void SetCursor(bool locked, bool visible)
		{
			if (_getCursorState == null && _setCursorState == null)
			{
				PrintCallbackUnavailable("cursor");
				return;
			}

			if (_setCursorState == null)
			{
				var (l, v) = _getCursorState!();
				Print($"Cursor — locked: {FormatBool(l)}  visible: {FormatBool(v)}");
				return;
			}

			_setCursorState(locked, visible);
			PrintSuccess($"Cursor — locked: {FormatBool(locked)}  visible: {FormatBool(visible)}");
		}

		private void Print(string message) => _printer.Print(new LogEntry(message, LogType.Standard));
		private void PrintSuccess(string message) => _printer.Print(new LogEntry(message, LogType.Success));
		private void PrintWarning(string message) => _printer.Print(new LogEntry(message, LogType.Warning));
		private void PrintError(string message) => _printer.Print(new LogEntry(message, LogType.Error));

		private void PrintCallbackUnavailable(string cmdName) => PrintWarning(
			$"Command '{cmdName}' requires Unity runtime callbacks that are not wired. Assign them in the constructor.");

		private static string SectionHeader(string title)
		{
			string left = RichText.Color("  ── ", ShellPalette.AccentMuted);
			string name = RichText.Bold(RichText.Color(title, ShellPalette.AccentBright));
			string right = RichText.Color(" " + new string('─', Math.Max(0, 44 - title.Length)), ShellPalette.AccentDim);
			return left + name + right;
		}

		private static string Indent(int level) => new string(' ', level * 2);

		private static void AppendKV(StringBuilder sb, string key, string value)
		{
			string k = RichText.Color(key.PadRight(14), ShellPalette.TextMuted);
			sb.Append(Indent(1)).Append(k).Append("  ").AppendLine(value);
		}

		private static void AppendKVWide(StringBuilder sb, string key, string value, string valueColor)
		{
			string k = RichText.Color(key.PadRight(20), ShellPalette.TextMuted);
			sb.Append(Indent(1)).Append(k).Append("  ").AppendLine(RichText.Color(value, valueColor));
		}

		private static string FormatStat(string text, string color) => RichText.Color(text, color);

		private static string FormatBool(bool v) => v ? RichText.Color("yes", ShellPalette.Success) : RichText.Color("no", ShellPalette.TextMuted);

		private static string WrapText(string text, int maxLineLength)
		{
			if (string.IsNullOrEmpty(text)) return string.Empty;

			var words = text.Split(' ');
			var sb = new StringBuilder();
			int currentLineLength = 0;

			for (int i = 0; i < words.Length; i++)
			{
				if (currentLineLength + words[i].Length > maxLineLength && currentLineLength > 0)
				{
					sb.AppendLine();
					currentLineLength = 0;
				}
				sb.Append(words[i]);
				currentLineLength += words[i].Length;

				if (i < words.Length - 1)
				{
					sb.Append(' ');
					currentLineLength++;
				}
			}
			return sb.ToString();
		}

		private static string FormatUsage(CommandSignature sig, int maxWidth)
		{
			var sb = new StringBuilder();
			string cmdName = RichText.Color(sig.Name, ShellPalette.SyntaxCommand);
			sb.Append(cmdName);

			int currentLineLen = sig.Name.Length;

			foreach (CommandParameter p in sig.Parameters)
			{
				string typeStr = FriendlyTypeName(p.ParameterType);
				string typePart = RichText.Color($":{typeStr}", ShellPalette.SyntaxType);
				string nameStr = RichText.Color($"-{p.Name}", ShellPalette.SyntaxParam);

				string formattedParam = p.IsOptional
					? RichText.Color(" [", ShellPalette.TextDim) + nameStr + typePart + RichText.Color("]", ShellPalette.TextDim)
					: RichText.Color(" <", ShellPalette.TextDim) + nameStr + typePart + RichText.Color(">", ShellPalette.TextDim);

				int paramVisLen = RichTextStripper.Strip(formattedParam).Length;

				if (currentLineLen + paramVisLen > maxWidth && currentLineLen > 0)
				{
					sb.Append('\n');
					sb.Append("  "); 
					currentLineLen = 2;
				}

				sb.Append(formattedParam);
				currentLineLen += paramVisLen;
			}

			return sb.ToString();
		}

		private static string FormatEnvTags(EnvironmentTag tags)
		{
			if (tags == EnvironmentTag.Any) return RichText.Color("Any", ShellPalette.TextMuted);

			var parts = new List<string>(3);
			if ((tags & EnvironmentTag.Editor) != 0) parts.Add(RichText.Color("[Editor]", ShellPalette.BadgeEditor));
			if ((tags & EnvironmentTag.Development) != 0) parts.Add(RichText.Color("[Development]", ShellPalette.BadgeDev));
			if ((tags & EnvironmentTag.Release) != 0) parts.Add(RichText.Color("[Release]", ShellPalette.BadgeRelease));

			return string.Join("  ", parts);
		}

		private static string FriendlyTypeName(Type t)
		{
			if (t == typeof(int)) return "int";
			if (t == typeof(float)) return "float";
			if (t == typeof(bool)) return "bool";
			if (t == typeof(string)) return "string";
			if (t == typeof(double)) return "double";
			if (t == typeof(long)) return "long";
			if (t == typeof(int[])) return "int[]";
			if (t == typeof(float[])) return "float[]";
			if (t == typeof(string[])) return "string[]";
			return t.Name;
		}
	}
}