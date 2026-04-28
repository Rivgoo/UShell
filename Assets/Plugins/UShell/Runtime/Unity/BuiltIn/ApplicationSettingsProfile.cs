#nullable enable
using UnityEngine;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands.Fluent;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Core.Output.Formatting;

namespace UShell.Runtime.Unity.BuiltIn
{
	/// <summary>
	/// Provides commands for modifying Application, Screen, and Time settings at runtime.
	/// </summary>
	public sealed class ApplicationSettingsProfile : ShellProfile
	{
		public ApplicationSettingsProfile(IConsolePrinter printer) : base(printer) { }

		protected override void Configure(ICommandBuilder builder)
		{
			builder.WithName("app.quit")
				.WithDescription("Quits the application.")
				.WithAlias("quit")
				.WithAlias("exit")
				.Executes(ExecuteQuit);

			builder.WithName("screen.fullscreen")
				.WithDescription("Gets or sets the fullscreen state of the application.")
				.AddOptionalParameter<bool>("enabled", true)
				.Executes<bool>(SetFullscreen);

			builder.WithName("screen.resolution")
				.WithDescription("Sets the display resolution.")
				.AddParameter<int>("width")
				.AddParameter<int>("height")
				.AddOptionalParameter<bool>("fullscreen", true)
				.Executes<int, int, bool>(SetResolution);

			builder.WithName("gfx.fps")
				.WithDescription("Gets or sets the target maximum framerate. Use -1 for unlimited.")
				.AddOptionalParameter<int>("target", -2) // -2 acts as a 'get' signal
				.Executes<int>(SetMaxFps);

			builder.WithName("gfx.vsync")
				.WithDescription("Gets or sets the VSync count (0 = disabled, 1 = every VBlank, 2 = every second VBlank).")
				.AddOptionalParameter<int>("count", -1)
				.Executes<int>(SetVSync);

			builder.WithName("time.scale")
				.WithDescription("Gets or sets the global flow of time (Time.timeScale).")
				.AddOptionalParameter<float>("scale", -1f)
				.Executes<float>(SetTimeScale);
		}

		private void ExecuteQuit()
		{
			PrintWarning("Termination requested...");

#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif
		}

		private void SetFullscreen(bool enabled)
		{
			Screen.fullScreen = enabled;
			PrintSuccess($"Fullscreen mode: {ProfileFormatter.FormatBool(enabled)}");
		}

		private void SetResolution(int width, int height, bool fullscreen)
		{
			if (width <= 0 || height <= 0)
			{
				PrintError("Width and height must be strictly positive.");
				return;
			}
			Screen.SetResolution(width, height, fullscreen);
			PrintSuccess($"Resolution set to {width}x{height} (Fullscreen: {fullscreen}).");
		}

		private void SetMaxFps(int target)
		{
			if (target == -2)
			{
				Print($"Target FPS: {RichText.Color(Application.targetFrameRate.ToString(), ShellPalette.SyntaxNumber)}");
				return;
			}

			Application.targetFrameRate = target;
			PrintSuccess($"Target FPS set to {(target == -1 ? "Unlimited" : target.ToString())}.");
		}

		private void SetVSync(int count)
		{
			if (count < 0)
			{
				Print($"VSync Count: {RichText.Color(QualitySettings.vSyncCount.ToString(), ShellPalette.SyntaxNumber)}");
				return;
			}

			QualitySettings.vSyncCount = Mathf.Clamp(count, 0, 4);
			PrintSuccess($"VSync set to {QualitySettings.vSyncCount}.");
		}

		private void SetTimeScale(float scale)
		{
			if (scale < 0f)
			{
				Print($"Time Scale: {RichText.Color(Time.timeScale.ToString("F2"), ShellPalette.SyntaxNumber)}");
				return;
			}

			Time.timeScale = Mathf.Max(0f, scale);
			PrintSuccess($"Time Scale set to {Time.timeScale:F2}.");
		}
	}
}