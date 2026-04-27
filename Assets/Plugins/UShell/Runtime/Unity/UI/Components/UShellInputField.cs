using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UShell.Runtime.Unity.UI.Configuration;

namespace UShell.Runtime.Unity.UI.Components
{
	/// <summary>
	/// Manages the user text input area, applying configuration fonts and rendering autocomplete ghost text.
	/// </summary>
	public sealed class UShellInputField : MonoBehaviour
	{
		/// <summary>
		/// Fired when the text value changes due to user typing or internal updates.
		/// </summary>
		public event Action<string> OnInputChanged;

		[SerializeField] private TMP_InputField _realInput = null!;
		[SerializeField] private TextMeshProUGUI _ghostText = null!;
		[SerializeField] private Image _promptImage = null!;

		private UShellUIConfiguration _configuration = null!;
		private string _currentSuggestion = string.Empty;

		/// <summary>
		/// Gets the raw text currently inside the input field.
		/// </summary>
		public string CurrentText => _realInput.text;

		private void Awake()
		{
			_realInput.onValueChanged.AddListener(HandleValueChanged);
			_realInput.onValidateInput += ValidateInput;
		}

		/// <summary>
		/// Configures the input field's typography and colors based on the global settings.
		/// </summary>
		public void Initialize(UShellUIConfiguration configuration)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

			_realInput.lineType = TMP_InputField.LineType.SingleLine;
			_realInput.navigation = new Navigation { mode = Navigation.Mode.Horizontal };

			_realInput.fontAsset = configuration.MainFont;
			_realInput.pointSize = configuration.InputFontSize;

			_ghostText.font = configuration.MainFont;
			_ghostText.fontSize = configuration.InputFontSize;

			SetMode(ConsoleInputMode.Standard);
		}

		private void Update()
		{
			ProcessPromptBlinking();
		}

		/// <summary>
		/// Changes the interaction state and visual cue of the input field.
		/// </summary>
		public void SetMode(ConsoleInputMode mode)
		{
			if (mode == ConsoleInputMode.Locked)
			{
				_realInput.interactable = false;
				_ghostText.text = string.Empty;
				_promptImage.color = Color.gray;
			}
			else if (mode == ConsoleInputMode.Prompt)
			{
				_realInput.interactable = true;
				_promptImage.color = Color.yellow;
			}
			else
			{
				_realInput.interactable = true;
				_promptImage.color = _configuration.PromptColor;
			}
		}

		/// <summary>
		/// Overlays a faded suggestion text visually ahead of the user's current input.
		/// </summary>
		public void RenderAutocomplete(string suggestion)
		{
			_currentSuggestion = suggestion;
			UpdateGhostTextDisplay();
		}

		/// <summary>
		/// Silently clears the input field without triggering an update event.
		/// </summary>
		public void ClearInput()
		{
			_realInput.SetTextWithoutNotify(string.Empty);
			_currentSuggestion = string.Empty;
			UpdateGhostTextDisplay();
		}

		/// <summary>
		/// Overwrites the input field with the specified text and moves the caret to the end.
		/// </summary>
		public void SetInputText(string text)
		{
			text = SanitizeInput(text);

			_realInput.SetTextWithoutNotify(text);
			_realInput.caretPosition = text.Length;
			_currentSuggestion = string.Empty;
			UpdateGhostTextDisplay();
		}

		/// <summary>
		/// Enables the input field game object and forces UI focus.
		/// </summary>
		public void ActivateFocus()
		{
			gameObject.SetActive(true);
			StartCoroutine(ForceFocusRoutine());
		}

		/// <summary>
		/// Removes UI focus and disables the game object.
		/// </summary>
		public void DeactivateFocus()
		{
			_realInput.DeactivateInputField();
			gameObject.SetActive(false);
		}

		/// <summary>
		/// Re-applies UI focus to the input field if it is currently active.
		/// </summary>
		public void Refocus()
		{
			if (!gameObject.activeInHierarchy || !_realInput.interactable)
				return;

			StartCoroutine(ForceFocusRoutine());
		}

		private char ValidateInput(string text, int charIndex, char addedChar)
		{
			if (addedChar == '`' || addedChar == '~')
			{
				return '\0';
			}

			if (addedChar == '\t')
			{
				if (!string.IsNullOrEmpty(_currentSuggestion))
				{
					return '\0';
				}

				return ' ';
			}

			if (addedChar == '\n' || addedChar == '\r')
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
			if (string.IsNullOrEmpty(text)) return text;
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
			if (_realInput.text.Length > 0 || !_realInput.interactable)
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