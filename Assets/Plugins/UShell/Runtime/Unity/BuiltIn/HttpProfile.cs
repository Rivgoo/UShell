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
		/// The maximum number of characters to print to the console from an HTTP response.
		/// Prevents TextMeshPro from crashing due to max-vertex mesh limits on huge HTML/JSON bodies.
		/// </summary>
		private const int MaxResponsePrintLength = 3500;

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
					ctx.Token.ThrowIfCancellationRequested();
					
					float currentProgress = request.uploadProgress > 0 ? request.uploadProgress : request.downloadProgress;
					progress.Report(currentProgress, url);
					
					await Task.Yield();
				}

				if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
				{
					progress.Fail($"Error {request.responseCode}");
					ctx.PrintError($"HTTP Response: {request.responseCode} - {request.error}");
					
					PrintSafeResponseBody(ctx, request.downloadHandler?.text);
					return;
				}
			} 

			string statusColor = request.responseCode >= 200 && request.responseCode < 300 ? ShellPalette.Success : ShellPalette.Warning;
			string statusBadge = RichText.Bold(RichText.Color($"[{request.responseCode}]", statusColor));
			
			ctx.PrintSuccess($"{statusBadge} HTTP {method} request to {url} completed.");
			
			PrintSafeResponseBody(ctx, request.downloadHandler?.text);
		}

		/// <summary>
		/// Sanitizes, truncates, and wraps the raw network response to prevent TextMeshPro from 
		/// crashing when parsing raw HTML/XML tags.
		/// </summary>
		private void PrintSafeResponseBody(ICommandContext ctx, string? rawResponse)
		{
			if (string.IsNullOrWhiteSpace(rawResponse)) return;

			string safeText = rawResponse!;

			if (safeText.Length > MaxResponsePrintLength)
			{
				safeText = safeText.Substring(0, MaxResponsePrintLength) + "\n\n... [Response body truncated for console]";
			}

			string escapedText = $"<noparse>{safeText}</noparse>";
			
			ctx.Print(RichText.Color(escapedText, ShellPalette.TextSecondary));
		}
	}
}