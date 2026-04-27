using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Unity.UI.Configuration;

using LogType = UShell.Runtime.Core.Output.LogType;

namespace UShell.Runtime.Unity.UI.Components
{
	[RequireComponent(typeof(RectTransform))]
	public sealed class UShellLogItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		public LogType CurrentLogType { get; private set; }
		public Guid? CurrentId { get; private set; }

		[SerializeField] private Image _iconImage = null!;
		[SerializeField] private TextMeshProUGUI _textComponent = null!;
		[SerializeField] private Button _copyButton = null!;
		[SerializeField] private Image _backgroundImage = null!;

		[SerializeField, Space] private float _minHeight = 25f;
		[SerializeField] private float _padding = 3f;

		private RectTransform _rectTransform = null!;
		private string _currentMessage = string.Empty;

		private void Awake()
		{
			_rectTransform = GetComponent<RectTransform>();

			if (_copyButton != null)
			{
				_copyButton.onClick.AddListener(HandleCopyClicked);
				_copyButton.gameObject.SetActive(false);
				_backgroundImage.gameObject.SetActive(false);
			}
		}

		private void OnDestroy()
		{
			if (_copyButton != null)
			{
				_copyButton.onClick.RemoveListener(HandleCopyClicked);
			}
		}

		public void Initialize(UShellUIConfiguration configuration)
		{
			_textComponent.font = configuration.MainFont;
			_textComponent.fontSize = configuration.LogFontSize;
		}

		public void ApplyData(LogEntry log, UShellUIConfiguration configuration)
		{
			CurrentLogType = log.Type;
			CurrentId = log.Id;
			_currentMessage = log.Message;

			string displayText = _currentMessage;

			if (configuration.ForceGlobalMonospace)
			{
				string width = configuration.GlobalMonospaceWidth.ToString(System.Globalization.CultureInfo.InvariantCulture);
				displayText = $"<mspace={width}em>{displayText}</mspace>";
			}

			_textComponent.text = displayText;

			ApplyIconConfiguration(log.Type, configuration);
			ApplyTextColor(log.Type, configuration);
			ApplyHeight();

			if (_copyButton != null)
			{
				_copyButton.gameObject.SetActive(false);
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (_copyButton != null)
			{
				_copyButton.gameObject.SetActive(true);
				_backgroundImage.gameObject.SetActive(true);
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (_copyButton != null)
			{
				_copyButton.gameObject.SetActive(false);
				_backgroundImage.gameObject.SetActive(false);
			}
		}

		private void HandleCopyClicked()
		{
			if (!string.IsNullOrEmpty(_currentMessage))
			{
				GUIUtility.systemCopyBuffer = _currentMessage;
			}
		}

		private float CalculateHeight()
		{
			_textComponent.ForceMeshUpdate();
			float textHeight = _textComponent.preferredHeight;
			return textHeight;
		}

		private void ApplyHeight()
		{
			var targetHeight = Mathf.Max(CalculateHeight(), _minHeight) + _padding;
			_rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, targetHeight);
		}

		private void ApplyIconConfiguration(LogType type, UShellUIConfiguration configuration)
		{
			_iconImage.sprite = type switch
			{
				LogType.Success => configuration.SuccessIcon,
				LogType.Warning => configuration.WarningIcon,
				LogType.Error => configuration.ErrorIcon,
				_ => configuration.StandardIcon
			};

			_iconImage.color = ExtractColorByType(type, configuration);
		}

		private void ApplyTextColor(LogType type, UShellUIConfiguration configuration)
		{
			_textComponent.color = ExtractColorByType(type, configuration);
		}

		private static Color ExtractColorByType(LogType type, UShellUIConfiguration configuration)
		{
			return type switch
			{
				LogType.Success => configuration.SuccessLogColor,
				LogType.Warning => configuration.WarningLogColor,
				LogType.Error => configuration.ErrorLogColor,
				_ => configuration.StandardLogColor
			};
		}
	}
}