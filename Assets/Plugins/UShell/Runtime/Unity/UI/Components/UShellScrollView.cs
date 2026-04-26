using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Unity.UI.Configuration;

using LogType = UShell.Runtime.Core.Output.LogType;

namespace UShell.Runtime.Unity.UI.Components
{
	[RequireComponent(typeof(ScrollRect))]
	public sealed class UShellScrollView : MonoBehaviour
	{
		public event Action<int, int, int> OnStatsChanged = delegate { };

		[SerializeField] private RectTransform _content = null!;
		[SerializeField] private UShellLogItem _logItemPrefab = null!;

		private UShellUIConfiguration _configuration = null!;
		private ScrollRect _scrollRect = null!;

		private readonly Queue<UShellLogItem> _activeLogs = new();
		private bool _isScrolledToBottom = true;

		private int _infoCount;
		private int _warningCount;
		private int _errorCount;

		private void Awake()
		{
			_scrollRect = GetComponent<ScrollRect>();
			_scrollRect.onValueChanged.AddListener(HandleScrollChanged);
		}

		public void Initialize(UShellUIConfiguration configuration)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			Clear();
		}

		public void AddLog(LogEntry log)
		{
			UShellLogItem item;

			if (_activeLogs.Count < _configuration.MaxLogs)
			{
				item = Instantiate(_logItemPrefab, _content);
				item.Initialize(_configuration);
			}
			else
			{
				item = _activeLogs.Dequeue();
				AdjustStatCounter(item.CurrentLogType, -1);
				item.transform.SetAsLastSibling();
			}

			item.ApplyData(log, _configuration);
			_activeLogs.Enqueue(item);

			AdjustStatCounter(log.Type, 1);
			NotifyStatsChanged();

			if (_isScrolledToBottom)
			{
				ForceScrollToBottom();
			}
		}

		public void Clear()
		{
			foreach (var item in _activeLogs)
			{
				if (item != null)
				{
					Destroy(item.gameObject);
				}
			}

			_activeLogs.Clear();
			_isScrolledToBottom = true;
			ResetStatCounters();
		}

		private void HandleScrollChanged(Vector2 scrollPosition)
		{
			_isScrolledToBottom = _scrollRect.verticalNormalizedPosition <= 0.05f;
		}

		private void ForceScrollToBottom()
		{
			Canvas.ForceUpdateCanvases();

			if (_content.gameObject.activeInHierarchy)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
			}

			_scrollRect.verticalNormalizedPosition = 0f;
			_isScrolledToBottom = true;
		}

		private void AdjustStatCounter(LogType type, int delta)
		{
			switch (type)
			{
				case LogType.Warning:
					_warningCount += delta;
					break;
				case LogType.Error:
					_errorCount += delta;
					break;
				case LogType.Standard:
				case LogType.Success:
				default:
					_infoCount += delta;
					break;
			}
		}

		private void ResetStatCounters()
		{
			_infoCount = 0;
			_warningCount = 0;
			_errorCount = 0;
			NotifyStatsChanged();
		}

		private void NotifyStatsChanged()
		{
			OnStatsChanged.Invoke(_infoCount, _warningCount, _errorCount);
		}
	}
}