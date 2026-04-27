namespace UShell.Runtime.Core.Output.Formatting
{
	/// <summary>
	/// Centralized design-token color palette for the UShell developer console.
	/// </summary>
	/// <remarks>
	/// <para>
	/// All built-in and custom profiles should reference these constants instead of hard-coding
	/// hex strings, guaranteeing visual consistency across every command's output.
	/// </para>
	/// <para>
	/// Usage — wrap any string with <see cref="RichText.Color"/>:
	/// <code>
	///   string coloredText = RichText.Color("my text", ShellPalette.Accent);
	/// </code>
	/// </para>
	/// </remarks>
	public static class ShellPalette
	{
		// ──────────────────────────────────────────────────────
		//  ACCENT  ·  Dark-orange brand colour family
		// ──────────────────────────────────────────────────────

		/// <summary>Primary brand/accent — prompt symbol, section titles, interactive elements.</summary>
		public const string Accent = "#C96A1E";

		/// <summary>Brighter accent — hover states, highlighted names, important labels.</summary>
		public const string AccentBright = "#E07A28";

		/// <summary>Muted accent — secondary labels, mild highlighting, decorative separators.</summary>
		public const string AccentMuted = "#8A4A15";

		/// <summary>Very dim accent — barely-visible tint used for ruler lines or background hints.</summary>
		public const string AccentDim = "#5A3010";

		// ──────────────────────────────────────────────────────
		//  SEMANTIC  ·  Success / Warning / Error
		// ──────────────────────────────────────────────────────

		// — Success (green family) —

		/// <summary>Primary success colour — operation completed, value within range.</summary>
		public const string Success = "#6AAB52";

		/// <summary>Bright success — notable positive result, enabled flag.</summary>
		public const string SuccessBright = "#7DCF5F";

		/// <summary>Muted success — secondary success text, inactive positive state.</summary>
		public const string SuccessMuted = "#4A7A38";

		// — Warning (yellow/amber family) —

		/// <summary>Primary warning colour — non-fatal issue, deprecation, recommendation.</summary>
		public const string Warning = "#E0B040";

		/// <summary>Bright warning — strong alert that needs user attention.</summary>
		public const string WarningBright = "#F5C842";

		/// <summary>Muted warning — secondary note, informational caution.</summary>
		public const string WarningMuted = "#A07820";

		// — Error (red family) —

		/// <summary>Primary error colour — command failed, exception, parse error.</summary>
		public const string Error = "#E84040";

		/// <summary>Bright error — critical failure, unrecoverable state.</summary>
		public const string ErrorBright = "#FF5555";

		/// <summary>Muted error — secondary error context, error trace line, position pointer.</summary>
		public const string ErrorMuted = "#A02828";

		// ──────────────────────────────────────────────────────
		//  NEUTRAL  ·  White → near-black grey ramp (10 steps)
		// ──────────────────────────────────────────────────────

		/// <summary>Pure bright white — maximum emphasis, selected item, critical value.</summary>
		public const string White = "#FFFFFF";

		/// <summary>Off-white — primary body text, default log output, normal print.</summary>
		public const string TextPrimary = "#E8E2D9";

		/// <summary>Warm light grey — secondary body text, description lines in help.</summary>
		public const string TextSecondary = "#C0B8AA";

		/// <summary>Mid grey — tertiary information, table cell content, parameter values.</summary>
		public const string TextTertiary = "#8A8278";

		/// <summary>Soft muted grey — subtle metadata, environment tags, read-only annotations.</summary>
		public const string TextMuted = "#6A6460";

		/// <summary>Dark grey — placeholder hints, autocomplete ghost text, disabled items.</summary>
		public const string TextHint = "#4A4440";

		/// <summary>Very dark grey — barely-visible separator text, ruler labels.</summary>
		public const string TextDim = "#363230";

		// ──────────────────────────────────────────────────────
		//  SYNTAX  ·  Code / type / keyword colour family
		// ──────────────────────────────────────────────────────

		/// <summary>Command name — the primary token users type. Slightly blue-white.</summary>
		public const string SyntaxCommand = "#7AA8E8";

		/// <summary>Parameter name (<c>-flag</c> style). Soft cyan-green.</summary>
		public const string SyntaxParam = "#56B8A0";

		/// <summary>Type annotation (<c>&lt;int&gt;</c>, <c>&lt;float&gt;</c>). Warm amber.</summary>
		public const string SyntaxType = "#D4A044";

		/// <summary>String / enum literal value — olive-green, distinct from plain text.</summary>
		public const string SyntaxValue = "#9DBD6A";

		/// <summary>Numeric literal value — light purple.</summary>
		public const string SyntaxNumber = "#B088D8";

		/// <summary>Keyword / operator (<c>=</c>, <c>true</c>, <c>false</c>, brackets). Soft sky-blue.</summary>
		public const string SyntaxKeyword = "#64B8D8";

		/// <summary>Alias token — italic-style teal, visually distinct from the canonical name.</summary>
		public const string SyntaxAlias = "#4EA898";

		/// <summary>Usage signature line — full command usage string.</summary>
		public const string SyntaxUsage = "#88B8D8";

		// ──────────────────────────────────────────────────────
		//  DECORATION  ·  Structural / chrome elements
		// ──────────────────────────────────────────────────────

		/// <summary>Section header line (e.g., <c>── help ─────</c>).</summary>
		public const string HeaderRule = "#C96A1E";

		/// <summary>Table header row text.</summary>
		public const string TableHeader = "#A09080";

		/// <summary>Table separator line (<c>───┼───</c> style).</summary>
		public const string TableSeparator = "#4A4040";

		/// <summary>Table cell content.</summary>
		public const string TableCell = "#B0A898";

		/// <summary>Ruler / horizontal divider line.</summary>
		public const string Ruler = "#3A3230";

		/// <summary>Inline badge background label text for Editor.</summary>
		public const string BadgeEditor = "#6A94C8";

		/// <summary>Inline badge for Development environment tag.</summary>
		public const string BadgeDev = "#A87840";

		/// <summary>Inline badge for Release environment tag.</summary>
		public const string BadgeRelease = "#607840";

		/// <summary>Optional / default parameter indicator.</summary>
		public const string Optional = "#6A7880";

		/// <summary>Required parameter indicator.</summary>
		public const string Required = "#C88058";

		// ──────────────────────────────────────────────────────
		//  STAT / METRICS  ·  For runtime diagnostics output
		// ──────────────────────────────────────────────────────

		/// <summary>Good / healthy metric value.</summary>
		public const string StatGood = "#6AAB52";

		/// <summary>Degraded / borderline metric value.</summary>
		public const string StatWarn = "#D4902A";

		/// <summary>Critical / failing metric value.</summary>
		public const string StatCritical = "#E84040";

		/// <summary>Neutral metric label.</summary>
		public const string StatLabel = "#8A8A9A";

		/// <summary>Metric unit suffix (e.g., <c>ms</c>, <c>MB</c>).</summary>
		public const string StatUnit = "#6A6870";

		// ──────────────────────────────────────────────────────
		//  HELPERS
		// ──────────────────────────────────────────────────────

		/// <summary>
		/// Selects a semantic color for a float metric (lower is better, e.g., memory).
		/// </summary>
		/// <param name="value">The metric value to evaluate.</param>
		/// <param name="warnThreshold">Upper bound of the "good" zone.</param>
		/// <param name="critThreshold">Upper bound of the "warn" zone.</param>
		/// <returns>A hex color string.</returns>
		public static string MetricColor(float value, float warnThreshold, float critThreshold)
		{
			if (value <= warnThreshold) return StatGood;
			if (value <= critThreshold) return StatWarn;
			return StatCritical;
		}

		/// <summary>
		/// Selects a semantic color for a float metric (higher is better, e.g., FPS).
		/// </summary>
		public static string MetricColorInverted(float value, float goodThreshold, float warnThreshold)
		{
			if (value >= goodThreshold) return StatGood;
			if (value >= warnThreshold) return StatWarn;
			return StatCritical;
		}

		/// <summary>
		/// Returns the badge color for a given <see cref="UShell.Runtime.Core.Commands.EnvironmentTag"/> value.
		/// </summary>
		public static string EnvironmentTagColor(string tagName)
		{
			return tagName switch
			{
				"Editor" => BadgeEditor,
				"Development" => BadgeDev,
				"Release" => BadgeRelease,
				_ => TextMuted
			};
		}
	}
}