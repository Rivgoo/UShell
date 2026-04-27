#nullable enable
using System.Text;
using UnityEngine;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Commands.Fluent;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Core.Output.Formatting;

namespace UShell.Runtime.Unity.BuiltIn
{
	/// <summary>
	/// A built-in profile that provides commands for checking the current application and system environment.
	/// </summary>
	/// <remarks>
	/// Registers commands such as <c>info</c>, <c>env</c>, and <c>platform</c>.
	/// </remarks>
	public sealed class EnvironmentInfoProfile : ShellProfile
	{
		private readonly string _gameVersion;
		private readonly EnvironmentTag _environment;

		/// <summary>
		/// Initializes a new instance of the <see cref="EnvironmentInfoProfile"/> class.
		/// </summary>
		public EnvironmentInfoProfile(
			IConsolePrinter printer,
			string gameVersion,
			EnvironmentTag environment)
			: base(printer)
		{
			_gameVersion = gameVersion ?? "Unknown";
			_environment = environment;
		}

		/// <inheritdoc/>
		protected override void Configure(ICommandBuilder builder)
		{
			builder.WithName("info")
				.WithDescription("Displays a comprehensive summary of the current build, environment, and platform.")
				.WithAlias("sysinfo")
				.Executes(ShowFullInfo);

			builder.WithName("env")
				.WithDescription("Shows the active UShell environment tag.")
				.Executes(ShowEnvironment);

			builder.WithName("game.version")
				.WithDescription("Displays the application/game version string.")
				.WithAlias("gver")
				.Executes(ShowGameVersion);

			builder.WithName("platform")
				.WithDescription("Shows the current runtime platform.")
				.Executes(ShowPlatform);

			builder.WithName("buildguid")
				.WithDescription("Shows the unique GUID assigned to this build by Unity.")
				.RestrictedTo(EnvironmentTag.Editor | EnvironmentTag.Development)
				.Executes(ShowBuildGuid);
		}

		private void ShowFullInfo()
		{
			var sb = new StringBuilder();
			sb.AppendLine(ProfileFormatter.FormatSectionHeader("system info"));
			sb.AppendLine();

			ProfileFormatter.AppendKeyValue(sb, "Game Version", _gameVersion);

			string envLabel = _environment.ToString();
			string envColor = ShellPalette.EnvironmentTagColor(envLabel);
			ProfileFormatter.AppendKeyValue(sb, "Environment", RichText.Color(envLabel, envColor));

			string platform = Application.platform.ToString();
			ProfileFormatter.AppendKeyValue(sb, "Platform", platform);

			string guid = Application.buildGUID;
			ProfileFormatter.AppendKeyValue(sb, "Build GUID", RichText.Color(guid, ShellPalette.TextTertiary));

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

		private void ShowPlatform()
		{
			Print($"Platform:  {RichText.Color(Application.platform.ToString(), ShellPalette.TextPrimary)}");
		}

		private void ShowBuildGuid()
		{
			Print($"Build GUID:  {RichText.Color(Application.buildGUID, ShellPalette.TextTertiary)}");
		}
	}
}