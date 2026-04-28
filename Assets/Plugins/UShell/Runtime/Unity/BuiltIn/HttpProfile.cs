#nullable enable
using System;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands.Fluent;
using UShell.Runtime.Core.Execution.Context;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Core.Output.Formatting;

namespace UShell.Runtime.Unity.BuiltIn
{
	/// <summary>
	/// Provides interactive, asynchronous HTTP commands for testing APIs and fetching remote data.
	/// </summary>
	/// <remarks>
	/// All commands utilize <see cref="ExecutesInteractiveAsync"/>, generating a live progress bar 
	/// and allowing the user to gracefully abort the request by pressing Escape.
	/// </remarks>
	public sealed class HttpProfile : ShellProfile
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="HttpProfile"/> class.
		/// </summary>
		public HttpProfile(IConsolePrinter printer) : base(printer) { }

		/// <inheritdoc/>
		protected override void Configure(ICommandBuilder builder)
		{
			builder.WithName("http.get")
				.WithDescription("Sends an HTTP GET request to the specified URL and returns the response body.")
				.AddParameter<string>("url")
				.WithTimeout(TimeSpan.FromSeconds(30))
				.ExecutesInteractiveAsync<string>(ExecuteGetAsync);

			builder.WithName("http.post")
				.WithDescription("Sends an HTTP POST request with a plain text body to the specified URL.")
				.AddParameter<string>("url")
				.AddParameter<string>("body")
				.WithTimeout(TimeSpan.FromSeconds(30))
				.ExecutesInteractiveAsync<string, string>(ExecutePostAsync);

			builder.WithName("http.put")
				.WithDescription("Sends an HTTP PUT request with a plain text body to the specified URL.")
				.AddParameter<string>("url")
				.AddParameter<string>("body")
				.WithTimeout(TimeSpan.FromSeconds(30))
				.ExecutesInteractiveAsync<string, string>(ExecutePutAsync);

			builder.WithName("http.delete")
				.WithDescription("Sends an HTTP DELETE request to the specified URL.")
				.AddParameter<string>("url")
				.WithTimeout(TimeSpan.FromSeconds(30))
				.ExecutesInteractiveAsync<string>(ExecuteDeleteAsync);
		}

		private async Task ExecuteGetAsync(ICommandContext ctx, string url)
		{
			using UnityWebRequest request = UnityWebRequest.Get(url);
			await SendWebRequestAsync(ctx, request, "GET", url);
		}

		private async Task ExecutePostAsync(ICommandContext ctx, string url, string body)
		{
			using UnityWebRequest request = UnityWebRequest.PostWwwForm(url, body);
			// Override to send raw text instead of form data
			byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(body);
			request.uploadHandler = new UploadHandlerRaw(bodyRaw);
			request.SetRequestHeader("Content-Type", "text/plain");

			await SendWebRequestAsync(ctx, request, "POST", url);
		}

		private async Task ExecutePutAsync(ICommandContext ctx, string url, string body)
		{
			using UnityWebRequest request = UnityWebRequest.Put(url, body);
			request.SetRequestHeader("Content-Type", "text/plain");
			await SendWebRequestAsync(ctx, request, "PUT", url);
		}

		private async Task ExecuteDeleteAsync(ICommandContext ctx, string url)
		{
			using UnityWebRequest request = UnityWebRequest.Delete(url);
			// Ensure we still get the response body back on a delete request
			request.downloadHandler = new DownloadHandlerBuffer();
			await SendWebRequestAsync(ctx, request, "DELETE", url);
		}

		private async Task SendWebRequestAsync(ICommandContext ctx, UnityWebRequest request, string method, string url)
		{
			var operation = request.SendWebRequest();

			using (IProgressReporter progress = ctx.CreateProgressBar($"HTTP {method}"))
			{
				while (!operation.isDone)
				{
					ctx.Token.ThrowIfCancellationRequested(); // Cancel via Escape or timeout

					// Use upload progress if uploading, otherwise download progress
					float currentProgress = request.uploadProgress > 0 ? request.uploadProgress : request.downloadProgress;
					progress.Report(currentProgress, url);

					await Task.Yield();
				}

				if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
				{
					progress.Fail($"Error {request.responseCode}");
					ctx.PrintError($"HTTP Response: {request.responseCode} - {request.error}");
					if (!string.IsNullOrEmpty(request.downloadHandler?.text))
					{
						ctx.Print(request.downloadHandler.text);
					}
					return;
				}
			} // Disposing the progress bar automatically sets it to 100% / Success

			string statusColor = request.responseCode >= 200 && request.responseCode < 300 ? ShellPalette.Success : ShellPalette.Warning;
			string statusBadge = RichText.Bold(RichText.Color($"[{request.responseCode}]", statusColor));

			ctx.PrintSuccess($"{statusBadge} HTTP {method} request to {url} completed.");

			string responseText = request.downloadHandler?.text ?? string.Empty;
			if (!string.IsNullOrWhiteSpace(responseText))
			{
				ctx.Print(RichText.Color(responseText, ShellPalette.TextSecondary));
			}
		}
	}
}