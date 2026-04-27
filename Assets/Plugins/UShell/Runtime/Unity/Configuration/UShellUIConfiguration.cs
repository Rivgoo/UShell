using UnityEngine;
using TMPro;

namespace UShell.Runtime.Unity.UI.Configuration
{
	/// <summary>
	/// A <see cref="ScriptableObject"/> storing the global visual configuration for the UShell console.
	/// </summary>
	/// <remarks>
	/// Modify this asset in the Editor to customize typography, spacing, UI color schemes, and icon references.
	/// </remarks>
	[CreateAssetMenu(fileName = "UShellUIConfiguration", menuName = "UShell/UI Configuration")]
	public sealed class UShellUIConfiguration : ScriptableObject
	{
		[Header("Typography")]
		[Tooltip("The core TextMeshPro font asset used across all console elements.")]
		[SerializeField] private TMP_FontAsset _mainFont = null!;

		[Tooltip("Font size for the command input field and ghost autocomplete text.")]
		[SerializeField] private float _inputFontSize = 14f;

		[Tooltip("Font size for all standard log entries.")]
		[SerializeField] private float _logFontSize = 14f;

		[Header("Global Monospace (ASCII Alignment)")]
		[Tooltip("Forces ALL logs to use monospaced character widths. Fixes alignment when using proportional fonts.")]
		[SerializeField] private bool _forceGlobalMonospace = true;

		[Tooltip("The width of each character in 'em' units when forcing monospace.")]
		[SerializeField] private float _globalMonospaceWidth = 0.6f;

		[Header("Suggestions (Info Blocks)")]
		[Tooltip("The background color behind the signature tooltips below the input field.")]
		[SerializeField] private Color _suggestionBackgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);

		[Tooltip("Padding applied to the width of the suggestion text block.")]
		[SerializeField] private float _suggestionPaddingX = 5f;

		[Header("Colors & Themes")]
		[Tooltip("Color of the faded autocomplete suggestion text overlapping the user's input.")]
		[SerializeField] private Color _ghostTextColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);

		[Tooltip("Color of the command line prefix icon (e.g., '>').")]
		[SerializeField] private Color _promptColor = new Color(1f, 0.55f, 0f, 1f);

		[Header("Log Text Colors")]
		[SerializeField] private Color _standardLogColor = new Color(0.83f, 0.83f, 0.83f, 1f);
		[SerializeField] private Color _successLogColor = new Color(0.42f, 0.6f, 0.33f, 1f);
		[SerializeField] private Color _warningLogColor = new Color(0.86f, 0.86f, 0.67f, 1f);
		[SerializeField] private Color _errorLogColor = new Color(0.96f, 0.28f, 0.28f, 1f);

		[Header("Icons")]
		[SerializeField] private Sprite _standardIcon = null!;
		[SerializeField] private Sprite _successIcon = null!;
		[SerializeField] private Sprite _warningIcon = null!;
		[SerializeField] private Sprite _errorIcon = null!;

		[Header("Layout")]
		[Tooltip("The maximum amount of log entries to retain visually before older ones are destroyed.")]
		[SerializeField] private int _maxLogs = 100;

		/// <summary>Gets the main font assigned to the console UI.</summary>
		public TMP_FontAsset MainFont => _mainFont;

		/// <summary>Gets the configured input font size.</summary>
		public float InputFontSize => _inputFontSize;

		/// <summary>Gets the configured log line font size.</summary>
		public float LogFontSize => _logFontSize;

		/// <summary>Returns true if the UI is forced into a monospaced layout.</summary>
		public bool ForceGlobalMonospace => _forceGlobalMonospace;

		/// <summary>Gets the specific spacing multiplier for monospaced rendering.</summary>
		public float GlobalMonospaceWidth => _globalMonospaceWidth;

		/// <summary>Gets the background color used for suggestion popups.</summary>
		public Color SuggestionBackgroundColor => _suggestionBackgroundColor;

		/// <summary>Gets the horizontal padding for suggestion items.</summary>
		public float SuggestionPaddingX => _suggestionPaddingX;

		/// <summary>Gets the color used for autocomplete ghost text.</summary>
		public Color GhostTextColor => _ghostTextColor;

		/// <summary>Gets the color of the input prompt prefix.</summary>
		public Color PromptColor => _promptColor;

		/// <summary>Gets the default text color for standard logging.</summary>
		public Color StandardLogColor => _standardLogColor;

		/// <summary>Gets the text color for success messages.</summary>
		public Color SuccessLogColor => _successLogColor;

		/// <summary>Gets the text color for warning messages.</summary>
		public Color WarningLogColor => _warningLogColor;

		/// <summary>Gets the text color for error messages.</summary>
		public Color ErrorLogColor => _errorLogColor;

		/// <summary>Gets the sprite assigned for standard log entries.</summary>
		public Sprite StandardIcon => _standardIcon;

		/// <summary>Gets the sprite assigned for success log entries.</summary>
		public Sprite SuccessIcon => _successIcon;

		/// <summary>Gets the sprite assigned for warning log entries.</summary>
		public Sprite WarningIcon => _warningIcon;

		/// <summary>Gets the sprite assigned for error log entries.</summary>
		public Sprite ErrorIcon => _errorIcon;

		/// <summary>Gets the maximum number of UI log items to keep active.</summary>
		public int MaxLogs => _maxLogs;
	}
}