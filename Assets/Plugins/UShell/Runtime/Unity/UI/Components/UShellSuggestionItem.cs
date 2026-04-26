using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UShell.Runtime.Unity.UI.Configuration;

namespace UShell.Runtime.Unity.UI.Components
{
	[RequireComponent(typeof(RectTransform))]
	public sealed class UShellSuggestionItem : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _textComponent = null!;
		[SerializeField] private Image _backgroundImage = null!;

		private RectTransform _rectTransform = null!;

		private void Awake()
		{
			_rectTransform = GetComponent<RectTransform>();
		}

		public void Initialize(UShellUIConfiguration config)
		{
			_textComponent.font = config.MainFont;
			_textComponent.fontSize = config.InputFontSize;
			_backgroundImage.color = config.SuggestionBackgroundColor;
		}

		public void SetText(string text, float paddingX)
		{
			_textComponent.text = text;
			_textComponent.ForceMeshUpdate();

			float width = _textComponent.renderedWidth;
			_rectTransform.sizeDelta = new Vector2(width + paddingX, _rectTransform.sizeDelta.y);
		}
	}
}