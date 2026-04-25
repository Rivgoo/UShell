using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UShell.Runtime.Unity.UI.Configuration;

namespace UShell.Runtime.Unity.UI.Components
{
	public sealed class UShellInputField : MonoBehaviour
	{
		public event Action<string> OnInputChanged = delegate { };

		[SerializeField] private TMP_InputField _realInput = null!;
		[SerializeField] private TextMeshProUGUI _ghostText = null!;
		[SerializeField] private Image _promptImage = null!;

		private UShellUIConfiguration _configuration = null!;
		private string _currentSuggestion = string.Empty;

		public string CurrentText => _realInput.text;

		private void Awake()
		{
			_realInput.onValueChanged.AddListener(HandleValueChanged);
			_realInput.onValidateInput += ValidateInput;
		}

		public void Initialize(UShellUIConfiguration configuration)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

			_realInput.lineType = TMP_InputField.LineType.SingleLine;
			_realInput.navigation = new Navigation { mode = Navigation.Mode.Horizontal };

			_realInput.fontAsset = configuration.MainFont;
			_realInput.pointSize = configuration.InputFontSize;

			_ghostText.font = configuration.MainFont;
			_ghostText.fontSize = configuration.InputFontSize;
			_promptImage.color = configuration.PromptColor;
		}

		private void Update()
		{
			ProcessPromptBlinking();
		}

		public void RenderAutocomplete(string suggestion)
		{
			_currentSuggestion = suggestion;
			UpdateGhostTextDisplay();
		}

		public void ClearInput()
		{
			_realInput.SetTextWithoutNotify(string.Empty);
			_currentSuggestion = string.Empty;
			UpdateGhostTextDisplay();
		}

		public void SetInputText(string text)
		{
			text = SanitizeInput(text);

			_realInput.SetTextWithoutNotify(text);
			_realInput.caretPosition = text.Length;
			_currentSuggestion = string.Empty;
			UpdateGhostTextDisplay();
		}

		public void ActivateFocus()
		{
			gameObject.SetActive(true);
			StartCoroutine(ForceFocusRoutine());
		}

		public void DeactivateFocus()
		{
			_realInput.DeactivateInputField();
			gameObject.SetActive(false);
		}

		public void Refocus()
		{
			StartCoroutine(ForceFocusRoutine());
		}

		private char ValidateInput(string text, int charIndex, char addedChar)
		{
			if (addedChar == '`' || addedChar == '~')
			{
				return '\0';
			}

			if (addedChar == '\n' || addedChar == '\r' || addedChar == '\t')
			{
				return ' ';
			}

			return addedChar;
		}

		private void HandleValueChanged(string newValue)
		{
			if (RequiresSanitization(newValue))
			{
				int savedCaretPosition = _realInput.caretPosition;
				newValue = SanitizeInput(newValue);

				_realInput.SetTextWithoutNotify(newValue);
				_realInput.caretPosition = savedCaretPosition;
			}

			OnInputChanged.Invoke(newValue);
			UpdateGhostTextDisplay();
		}

		private static bool RequiresSanitization(string text)
		{
			return text.IndexOf('\n') >= 0 || text.IndexOf('\r') >= 0 || text.IndexOf('\t') >= 0;
		}

		private static string SanitizeInput(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}

			return text.Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ');
		}

		private void UpdateGhostTextDisplay()
		{
			string currentText = _realInput.text;

			if (string.IsNullOrEmpty(currentText) || string.IsNullOrEmpty(_currentSuggestion))
			{
				_ghostText.text = string.Empty;
				return;
			}

			if (_currentSuggestion.StartsWith(currentText, StringComparison.OrdinalIgnoreCase))
			{
				string completionPart = _currentSuggestion.Substring(currentText.Length);
				string invisibleTypedPart = $"<color=#00000000>{currentText}</color>";
				string visibleGhostPart = $"<color=#{ColorUtility.ToHtmlStringRGBA(_configuration.GhostTextColor)}>{completionPart}</color>";

				_ghostText.text = $"{invisibleTypedPart}{visibleGhostPart}";
			}
			else
			{
				_ghostText.text = string.Empty;
			}
		}

		private void ProcessPromptBlinking()
		{
			if (_realInput.text.Length > 0)
			{
				SetPromptAlpha(1f);
				return;
			}

			float alpha = Mathf.PingPong(Time.unscaledTime * 2f, 2f);
			SetPromptAlpha(Mathf.Lerp(0.2f, 1f, alpha));
		}

		private void SetPromptAlpha(float alpha)
		{
			Color currentImageColor = _promptImage.color;
			currentImageColor.a = alpha;
			_promptImage.color = currentImageColor;
		}

		private IEnumerator ForceFocusRoutine()
		{
			yield return null;
			_realInput.ActivateInputField();
			_realInput.Select();
			_realInput.caretPosition = _realInput.text.Length;
		}
	}
}