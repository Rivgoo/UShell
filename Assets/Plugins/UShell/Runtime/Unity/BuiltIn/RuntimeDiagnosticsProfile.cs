#nullable enable
using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Commands.Fluent;
using UShell.Runtime.Core.Output.Formatting;
using UShell.Runtime.Unity.Output;

namespace UShell.Runtime.Unity.BuiltIn
{
	/// <summary>
	/// A built-in profile that provides commands for evaluating Unity performance metrics and editor state.
	/// </summary>
	/// <remarks>
	/// Registers commands such as <c>stats</c>, <c>mem</c>, <c>gc</c>, and scene querying tools.
	/// </remarks>
	public sealed class RuntimeDiagnosticsProfile : ShellProfile
	{
		private readonly UnityConsolePrinter _unityPrinter;

		/// <summary>
		/// Initializes a new instance of the <see cref="RuntimeDiagnosticsProfile"/> class.
		/// </summary>
		public RuntimeDiagnosticsProfile(UnityConsolePrinter printer) : base(printer)
		{
			_unityPrinter = printer;
		}

		/// <inheritdoc/>
		protected override void Configure(ICommandBuilder builder)
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
				.ExecutesReturning<int>(ShowObjectCount);

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

		private void ShowStats()
		{
			var sb = new StringBuilder();
			sb.AppendLine(ProfileFormatter.FormatSectionHeader("runtime stats"));
			sb.AppendLine();

			float fps = Time.unscaledDeltaTime > 0f ? 1f / Time.unscaledDeltaTime : 0f;
			float deltaMs = Time.deltaTime * 1000f;
			float unscaledDeltaMs = Time.unscaledDeltaTime * 1000f;

			string fpsColor = ShellPalette.MetricColorInverted(fps, 55f, 25f);
			ProfileFormatter.AppendKeyValue(sb, "FPS", $"{RichText.Color($"{fps:F1}", fpsColor)}  {RichText.Color("fps", ShellPalette.StatUnit)}");
			ProfileFormatter.AppendKeyValue(sb, "Frame Time", $"{RichText.Color($"{deltaMs:F2}", ShellPalette.MetricColor(deltaMs, 16.7f, 33.3f))}  {RichText.Color("ms", ShellPalette.StatUnit)}");
			ProfileFormatter.AppendKeyValue(sb, "Unscaled Δt", $"{RichText.Color($"{unscaledDeltaMs:F2}", ShellPalette.StatLabel)}  {RichText.Color("ms", ShellPalette.StatUnit)}");

			sb.AppendLine();

			const float bytesToMb = 1048576f;
			float reserved = Profiler.GetTotalReservedMemoryLong() / bytesToMb;
			float allocated = Profiler.GetTotalAllocatedMemoryLong() / bytesToMb;
			float monoHeap = Profiler.GetMonoHeapSizeLong() / bytesToMb;
			float monoUsed = Profiler.GetMonoUsedSizeLong() / bytesToMb;
			int gcCount = GC.CollectionCount(0);

			ProfileFormatter.AppendKeyValue(sb, "Reserved", $"{RichText.Color($"{reserved:F1}", ShellPalette.StatLabel)}  {RichText.Color("MB", ShellPalette.StatUnit)}");
			ProfileFormatter.AppendKeyValue(sb, "Allocated", $"{RichText.Color($"{allocated:F1}", ShellPalette.MetricColor(allocated, 200f, 512f))}  {RichText.Color("MB", ShellPalette.StatUnit)}");
			ProfileFormatter.AppendKeyValue(sb, "Mono Heap", $"{RichText.Color($"{monoHeap:F1}", ShellPalette.StatLabel)}  {RichText.Color("MB", ShellPalette.StatUnit)}");
			ProfileFormatter.AppendKeyValue(sb, "Mono Used", $"{RichText.Color($"{monoUsed:F1}", ShellPalette.MetricColor(monoUsed, monoHeap * 0.6f, monoHeap * 0.9f))}  {RichText.Color("MB", ShellPalette.StatUnit)}");
			ProfileFormatter.AppendKeyValue(sb, "GC Count", gcCount.ToString());

			Print(sb.ToString().TrimEnd());
		}

		private void ShowMemory()
		{
			const float bytesToMb = 1048576f;
			float reserved = Profiler.GetTotalReservedMemoryLong() / bytesToMb;
			float allocated = Profiler.GetTotalAllocatedMemoryLong() / bytesToMb;
			float monoHeap = Profiler.GetMonoHeapSizeLong() / bytesToMb;
			float monoUsed = Profiler.GetMonoUsedSizeLong() / bytesToMb;
			int gcCount = GC.CollectionCount(0);
			float monoFragmentation = monoHeap > 0 ? (1f - monoUsed / monoHeap) * 100f : 0f;

			var sb = new StringBuilder();
			sb.AppendLine(ProfileFormatter.FormatSectionHeader("memory report"));
			sb.AppendLine();

			ProfileFormatter.AppendKeyValueWide(sb, "Total Reserved", $"{reserved:F2} MB", ShellPalette.StatLabel);
			ProfileFormatter.AppendKeyValueWide(sb, "Total Allocated", $"{allocated:F2} MB", ShellPalette.MetricColor(allocated, 200f, 512f));
			ProfileFormatter.AppendKeyValueWide(sb, "Unmanaged", $"{Math.Max(0, reserved - allocated):F2} MB", ShellPalette.StatLabel);

			sb.AppendLine();
			ProfileFormatter.AppendKeyValueWide(sb, "Mono Heap Size", $"{monoHeap:F2} MB", ShellPalette.StatLabel);
			ProfileFormatter.AppendKeyValueWide(sb, "Mono Used", $"{monoUsed:F2} MB", ShellPalette.MetricColor(monoUsed, monoHeap * 0.6f, monoHeap * 0.9f));
			ProfileFormatter.AppendKeyValueWide(sb, "Mono Free", $"{monoHeap - monoUsed:F2} MB", ShellPalette.StatGood);
			ProfileFormatter.AppendKeyValueWide(sb, "Fragmentation", $"{monoFragmentation:F1} %", ShellPalette.MetricColor(monoFragmentation, 30f, 60f));

			sb.AppendLine();
			ProfileFormatter.AppendKeyValueWide(sb, "GC Collections", gcCount.ToString(), ShellPalette.StatLabel);

			Print(sb.ToString().TrimEnd());
		}

		private void ForceGC()
		{
			PrintWarning("Requesting GC collection…");
			GC.Collect();
			PrintSuccess("GC.Collect() executed. Check 'mem' for updated heap statistics.");
		}

		private void ShowTime()
		{
			var sb = new StringBuilder();
			sb.AppendLine(ProfileFormatter.FormatSectionHeader("time info"));
			sb.AppendLine();

			float scale = Time.timeScale;
			string scaleColor = scale < 0.01f ? ShellPalette.StatCritical : scale < 0.5f ? ShellPalette.StatWarn : ShellPalette.StatGood;

			ProfileFormatter.AppendKeyValue(sb, "Time Scale", ProfileFormatter.FormatStat($"{scale:F4}", scaleColor));
			ProfileFormatter.AppendKeyValue(sb, "Game Time", $"{Time.time:F3} s");
			ProfileFormatter.AppendKeyValue(sb, "Real Time", $"{Time.realtimeSinceStartup:F3} s");
			ProfileFormatter.AppendKeyValue(sb, "Frame", Time.frameCount.ToString());

			Print(sb.ToString().TrimEnd());
		}

		private int ShowObjectCount()
		{
			int count = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;
			Print($"Active GameObjects in scene:  {RichText.Bold(RichText.Color(count.ToString(), ShellPalette.SyntaxNumber))}");
			return count;
		}

		private void FindByTag(string tag, int limit)
		{
			var names = GameObject.FindGameObjectsWithTag(tag)
				.Select(g => g.name)
				.ToList();

			PrintList($"GameObjects with tag '{tag}'", names, limit);
		}

		private void FindByLayer(string layer, int limit)
		{
			int layerIndex = LayerMask.NameToLayer(layer);
			if (layerIndex == -1)
			{
				PrintList($"GameObjects on layer '{layer}'", Array.Empty<string>(), limit);
				return;
			}

			var names = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
				.Where(g => g.layer == layerIndex)
				.Select(g => g.name)
				.ToList();

			PrintList($"GameObjects on layer '{layer}'", names, limit);
		}

		private void SetUnityLogMirror(bool enabled)
		{
			_unityPrinter.MirrorToUnityConsole = enabled;
			if (enabled)
			{
				PrintSuccess("Unity console mirroring: ENABLED.");
			}
			else
			{
				Print(RichText.Color("Unity console mirroring: DISABLED.", ShellPalette.TextSecondary));
			}
		}

		private void ClearPrefs()
		{
			PlayerPrefs.DeleteAll();
			PlayerPrefs.Save();
			PrintSuccess("All PlayerPrefs cleared.");
		}

		private void GetPrefs(string key)
		{
			if (!PlayerPrefs.HasKey(key))
			{
				PrintWarning($"Key '{key}' not found in PlayerPrefs.");
				return;
			}

			string value = PlayerPrefs.GetString(key);
			Print($"{RichText.Color(key, ShellPalette.SyntaxParam)}  =  {RichText.Color(value, ShellPalette.SyntaxValue)}");
		}

		private void SetPrefs(string key, string value)
		{
			PlayerPrefs.SetString(key, value);
			PlayerPrefs.Save();
			PrintSuccess($"PlayerPrefs['{key}'] = '{value}'");
		}

		private void SetCursor(bool locked, bool visible)
		{
			Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
			Cursor.visible = visible;

			PrintSuccess($"Cursor — locked: {ProfileFormatter.FormatBool(locked)}  visible: {ProfileFormatter.FormatBool(visible)}");
		}
	}
}