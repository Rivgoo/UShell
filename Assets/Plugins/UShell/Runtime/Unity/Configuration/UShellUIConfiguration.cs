using UnityEngine;
using TMPro;

namespace UShell.Runtime.Unity.UI.Configuration
{
	[CreateAssetMenu(fileName = "UShellUIConfiguration", menuName = "UShell/UI Configuration")]
	public sealed class UShellUIConfiguration : ScriptableObject
	{
		[Header("Typography")]
		[SerializeField] private TMP_FontAsset _mainFont = null!;
		[SerializeField] private float _inputFontSize = 14f;
		[SerializeField] private float _logFontSize = 14f;

		[Header("Colors & Themes")]
		[SerializeField] private Color _ghostTextColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
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
		[SerializeField] private int _maxLogs = 100;

		public TMP_FontAsset MainFont => _mainFont;
		public float InputFontSize => _inputFontSize;
		public float LogFontSize => _logFontSize;

		public Color GhostTextColor => _ghostTextColor;
		public Color PromptColor => _promptColor;

		public Color StandardLogColor => _standardLogColor;
		public Color SuccessLogColor => _successLogColor;
		public Color WarningLogColor => _warningLogColor;
		public Color ErrorLogColor => _errorLogColor;

		public Sprite StandardIcon => _standardIcon;
		public Sprite SuccessIcon => _successIcon;
		public Sprite WarningIcon => _warningIcon;
		public Sprite ErrorIcon => _errorIcon;

		public int MaxLogs => _maxLogs;
	}
}