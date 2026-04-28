#nullable enable
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands.Fluent;
using UShell.Runtime.Core.Execution.Context;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Core.Output.Formatting;
using System;

namespace UShell.Runtime.Unity.BuiltIn
{
	/// <summary>
	/// Provides interactive and synchronous commands for Unity Scene management.
	/// </summary>
	public sealed class SceneManagementProfile : ShellProfile
	{
		public SceneManagementProfile(IConsolePrinter printer) : base(printer) { }

		protected override void Configure(ICommandBuilder builder)
		{
			builder.WithName("scene.load")
				.WithDescription("Asynchronously loads a scene by name with a progress bar.")
				.AddParameter<string>("sceneName")
				.WithTimeout(TimeSpan.FromSeconds(30))
				.ExecutesInteractiveAsync<string>(LoadSceneAsync);

			builder.WithName("scene.load.index")
				.WithDescription("Asynchronously loads a scene by its build index with a progress bar.")
				.AddParameter<int>("buildIndex")
				.WithTimeout(TimeSpan.FromSeconds(30))
				.ExecutesInteractiveAsync<int>(LoadSceneIndexAsync);

			builder.WithName("scene.unload")
				.WithDescription("Unloads an active scene by name.")
				.AddParameter<string>("sceneName")
				.Executes<string>(UnloadScene);

			builder.WithName("scene.active")
				.WithDescription("Gets or sets the active scene.")
				.AddOptionalParameter<string>("setSceneName", string.Empty)
				.Executes<string>(ManageActiveScene);

			builder.WithName("scene.list")
				.WithDescription("Lists all scenes currently loaded in the hierarchy.")
				.Executes(ListLoadedScenes);
		}

		private async Task LoadSceneAsync(ICommandContext ctx, string sceneName)
		{
			await ExecuteSceneLoad(ctx, SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive), sceneName);
		}

		private async Task LoadSceneIndexAsync(ICommandContext ctx, int buildIndex)
		{
			await ExecuteSceneLoad(ctx, SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive), $"Index {buildIndex}");
		}

		private async Task ExecuteSceneLoad(ICommandContext ctx, UnityEngine.AsyncOperation operation, string identifier)
		{
			if (operation == null)
			{
				ctx.PrintError($"Failed to start loading scene: '{identifier}'. Ensure it is added to Build Settings.");
				return;
			}

			using (IProgressReporter progress = ctx.CreateProgressBar($"Loading {identifier}"))
			{
				while (!operation.isDone)
				{
					ctx.Token.ThrowIfCancellationRequested();
					progress.Report(operation.progress, $"{Math.Round(operation.progress * 100)}%");
					await Task.Yield();
				}
			}

			ctx.PrintSuccess($"Scene '{identifier}' loaded successfully.");
		}

		private void UnloadScene(string sceneName)
		{
			Scene scene = SceneManager.GetSceneByName(sceneName);
			if (!scene.IsValid() || !scene.isLoaded)
			{
				PrintWarning($"Scene '{sceneName}' is not currently loaded.");
				return;
			}

			SceneManager.UnloadSceneAsync(scene);
			PrintSuccess($"Unload request sent for scene '{sceneName}'.");
		}

		private void ManageActiveScene(string setSceneName)
		{
			if (string.IsNullOrWhiteSpace(setSceneName))
			{
				Print($"Active Scene: {RichText.Color(SceneManager.GetActiveScene().name, ShellPalette.SyntaxValue)}");
				return;
			}

			Scene scene = SceneManager.GetSceneByName(setSceneName);
			if (!scene.IsValid() || !scene.isLoaded)
			{
				PrintError($"Cannot set active scene. '{setSceneName}' is not loaded.");
				return;
			}

			SceneManager.SetActiveScene(scene);
			PrintSuccess($"Active scene set to '{setSceneName}'.");
		}

		private void ListLoadedScenes()
		{
			var sb = new StringBuilder();
			sb.AppendLine(ProfileFormatter.FormatSectionHeader("loaded scenes"));

			int count = SceneManager.sceneCount;
			for (int i = 0; i < count; i++)
			{
				Scene scene = SceneManager.GetSceneAt(i);
				string activeFlag = scene == SceneManager.GetActiveScene()
					? RichText.Color(" [ACTIVE]", ShellPalette.Success)
					: string.Empty;

				sb.AppendLine($"  {RichText.Color(scene.buildIndex.ToString(), ShellPalette.SyntaxNumber)} : {RichText.Color(scene.name, ShellPalette.SyntaxValue)}{activeFlag}");
			}

			Print(sb.ToString().TrimEnd());
		}
	}
}