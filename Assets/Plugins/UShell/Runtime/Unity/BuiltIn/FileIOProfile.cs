#nullable enable
using System;
using System.IO;
using UnityEngine;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Commands.Fluent;
using UShell.Runtime.Core.Output;

namespace UShell.Runtime.Unity.BuiltIn
{
	/// <summary>
	/// Provides restricted file system commands and screenshot capturing utilities.
	/// </summary>
	/// <remarks>
	/// To maintain security, these commands are restricted to the <see cref="EnvironmentTag.Editor"/> 
	/// and <see cref="EnvironmentTag.Development"/> environments by default.
	/// </remarks>
	public sealed class FileIOProfile : ShellProfile
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FileIOProfile"/> class.
		/// </summary>
		public FileIOProfile(IConsolePrinter printer) : base(printer) { }

		/// <inheritdoc/>
		protected override void Configure(ICommandBuilder builder)
		{
			builder.WithName("file.read")
				.WithDescription("Reads the contents of the file at the provided absolute or relative path.")
				.AddParameter<string>("path")
				.RestrictedTo(EnvironmentTag.Editor | EnvironmentTag.Development)
				.ExecutesReturning<string, string>(ReadFile);

			builder.WithName("file.write")
				.WithDescription("Writes the provided text data to a file at the provided path. Overwrites if exists.")
				.AddParameter<string>("path")
				.AddParameter<string>("contents")
				.RestrictedTo(EnvironmentTag.Editor | EnvironmentTag.Development)
				.Executes<string, string>(WriteFile);

			builder.WithName("screenshot")
				.WithDescription("Captures a screenshot and saves it as a PNG to the persistent data path.")
				.AddOptionalParameter<string>("filename", "screenshot.png")
				.AddOptionalParameter<int>("superSize", 1)
				.RestrictedTo(EnvironmentTag.Editor | EnvironmentTag.Development)
				.Executes<string, int>(CaptureScreenshot);
		}

		private string ReadFile(string path)
		{
			try
			{
				if (!File.Exists(path))
				{
					throw new FileNotFoundException($"The file at path '{path}' does not exist.");
				}

				string contents = File.ReadAllText(path);
				PrintSuccess($"Read {contents.Length} characters from '{path}'.");
				return contents;
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to read file: {ex.Message}");
			}
		}

		private void WriteFile(string path, string contents)
		{
			try
			{
				string? directory = Path.GetDirectoryName(path);
				if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}

				File.WriteAllText(path, contents);
				PrintSuccess($"Successfully wrote {contents.Length} characters to '{path}'.");
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to write file: {ex.Message}");
			}
		}

		private void CaptureScreenshot(string filename, int superSize)
		{
			if (superSize < 1) superSize = 1;

			if (!filename.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
			{
				filename += ".png";
			}

			// Ensure it saves to a safe, reliable directory across platforms
			string fullPath = Path.Combine(Application.persistentDataPath, filename);

			try
			{
				ScreenCapture.CaptureScreenshot(fullPath, superSize);
				PrintSuccess($"Screenshot requested. Saving to: {fullPath} (SuperSize: {superSize}x)");
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to capture screenshot: {ex.Message}");
			}
		}
	}
}