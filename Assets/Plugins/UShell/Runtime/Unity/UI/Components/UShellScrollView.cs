using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Unity.UI.Configuration;

using LogType = UShell.Runtime.Core.Output.LogType;

namespace UShell.Runtime.Unity.UI.Components
{
	/// <summary>
	/// Manages the scrolling list of log entries, handling object pooling, instantiation, and auto-scrolling logic.
	/// </summary>
	[RequireComponent(typeof(ScrollRect))]
	public sealed class UShellScrollView : MonoBehaviour
	{
		/// <summary>
		/// Fired when logs are added or removed, reporting the new totals.
		/// </summary>
		public event Action<int, int, int> OnStatsChanged = delegate { };

		[SerializeField] private RectTransform _content = null!;
		[SerializeField] private UShellLogItem _logItemPrefab = null!;

		private UShellUIConfiguration _configuration = null!;
		private ScrollRect _scrollRect = null!;

		private readonly Queue<UShellLogItem> _activeLogs = new();
		private readonly Dictionary<Guid, UShellLogItem> _logMap = new();

		private bool _isScrolledToBottom = true;

		private int _infoCount;
		private int _warningCount;
		private int _errorCount;

		/// <summary>Gets the current amount of visual log items rendered.</summary>
		public int TotalLogsCount => _activeLogs.Count;

		private void Awake()
		{
			_scrollRect = GetComponent<ScrollRect>();
			_scrollRect.onValueChanged.AddListener(HandleScrollChanged);
		}

		/// <summary>
		/// Assigns the configuration and resets the view state.
		/// </summary>
		public void Initialize(UShellUIConfiguration configuration)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			Clear();
		}

		/// <summary>
		/// Appends a new log to the bottom of the list. Reuses the oldest entry if the capacity limit is reached.
		/// </summary>
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

				if (item.CurrentId.HasValue)
				{
					_logMap.Remove(item.CurrentId.Value);
				}

				AdjustStatCounter(item.CurrentLogType, -1);
				item.transform.SetAsLastSibling();
			}

			item.ApplyData(log, _configuration);

			if (log.Id.HasValue)
			{
				_logMap[log.Id.Value] = item;
			}

			_activeLogs.Enqueue(item);

			AdjustStatCounter(log.Type, 1);
			NotifyStatsChanged();

			if (_isScrolledToBottom)
			{
				ForceScrollToBottom();
			}
		}

		/// <summary>
		/// Locates an existing log by its ID and updates its content without moving it.
		/// </summary>
		public void UpdateLog(Guid id, LogEntry log)
		{
			if (_logMap.TryGetValue(id, out UShellLogItem item))
			{
				if (item.CurrentLogType != log.Type)
				{
					AdjustStatCounter(item.CurrentLogType, -1);
					AdjustStatCounter(log.Type, 1);
					NotifyStatsChanged();
				}

				item.ApplyData(log, _configuration);

				if (_isScrolledToBottom)
				{
					ForceScrollToBottom();
				}
			}
		}

		/// <summary>
		/// Destroys all visual log items and resets the UI state.
		/// </summary>
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
			_logMap.Clear();
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