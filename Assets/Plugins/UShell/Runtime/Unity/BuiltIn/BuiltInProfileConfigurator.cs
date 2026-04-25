#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Unity.Output;

namespace UShell.Runtime.Unity.BuiltIn
{
	public static class BuiltInProfileConfigurator
	{
		public static BuiltInShellProfile Create(
			UnityConsolePrinter printer,
			ICommandRegistry registry,
			string gameVersion,
			EnvironmentTag environment,
			Func<IReadOnlyList<string>> historyProvider)
		{
			return new BuiltInShellProfile(
				printer: printer,
				registry: registry,
				gameVersion: gameVersion,
				environment: environment,
				getFrameStats: GetFrameStats,
				getMemStats: GetMemStats,
				forceGC: ForceGC,
				getTimeInfo: GetTimeInfo,
				isPaused: GetIsPaused,
				getObjectCount: GetObjectCount,
				findByTag: FindByTag,
				findByLayer: FindByLayer,
				getMirrorToUnity: () => printer.MirrorToUnityConsole,
				setMirrorToUnity: v => printer.MirrorToUnityConsole = v,
				clearPrefs: PlayerPrefs.DeleteAll,
				getPrefs: GetPrefs,
				setPrefs: PlayerPrefs.SetString,
				getCursorState: GetCursorState,
				setCursorState: SetCursorState,
				getPlatform: () => Application.platform.ToString(),
				getBuildGuid: () => Application.buildGUID,
				getCommandHistory: historyProvider
			);
		}

		private static (float, float, float) GetFrameStats()
		{
			float fps = Time.unscaledDeltaTime > 0f ? 1f / Time.unscaledDeltaTime : 0f;
			return (fps, Time.deltaTime * 1000f, Time.unscaledDeltaTime * 1000f);
		}

		private static (float, float, float, float, int) GetMemStats()
		{
			const float bytesToMb = 1048576f;
			return (
				Profiler.GetTotalReservedMemoryLong() / bytesToMb,
				Profiler.GetTotalAllocatedMemoryLong() / bytesToMb,
				Profiler.GetMonoHeapSizeLong() / bytesToMb,
				Profiler.GetMonoUsedSizeLong() / bytesToMb,
				GC.CollectionCount(0)
			);
		}

		private static void ForceGC()
		{
			GC.Collect();
		}

		private static (float, float, float, int) GetTimeInfo()
		{
			return (Time.timeScale, Time.time, Time.realtimeSinceStartup, Time.frameCount);
		}

		private static bool GetIsPaused()
		{
			return Time.timeScale == 0f;
		}

		private static int GetObjectCount()
		{
			return UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;
		}

		private static IReadOnlyList<string> FindByTag(string tag)
		{
			return GameObject.FindGameObjectsWithTag(tag)
				.Select(g => g.name)
				.ToList();
		}

		private static IReadOnlyList<string> FindByLayer(string layer)
		{
			int layerIndex = LayerMask.NameToLayer(layer);
			if (layerIndex == -1)
			{
				return Array.Empty<string>();
			}

			return UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
				.Where(g => g.layer == layerIndex)
				.Select(g => g.name)
				.ToList();
		}

		private static string? GetPrefs(string key)
		{
			return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : null;
		}

		private static (bool, bool) GetCursorState()
		{
			return (Cursor.lockState == CursorLockMode.Locked, Cursor.visible);
		}

		private static void SetCursorState(bool locked, bool visible)
		{
			Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
			Cursor.visible = visible;
		}
	}
}