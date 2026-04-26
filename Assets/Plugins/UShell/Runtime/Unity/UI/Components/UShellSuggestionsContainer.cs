using System;
using System.Collections.Generic;
using UnityEngine;
using UShell.Runtime.Unity.UI.Configuration;

namespace UShell.Runtime.Unity.UI.Components
{
	public sealed class UShellSuggestionsContainer : MonoBehaviour
	{
		[SerializeField] private UShellSuggestionItem _itemPrefab = null!;
		[SerializeField] private RectTransform _containerRect = null!;

		private UShellUIConfiguration _config = null!;
		private readonly List<UShellSuggestionItem> _pool = new(5);

		public void Initialize(UShellUIConfiguration config)
		{
			_config = config ?? throw new ArgumentNullException(nameof(config));

			for (int i = 0; i < 5; i++)
			{
				var item = Instantiate(_itemPrefab, _containerRect);
				item.Initialize(_config);
				item.gameObject.SetActive(false);
				_pool.Add(item);
			}
		}

		public void Render(IReadOnlyList<string> signatures)
		{
			int count = (signatures == null) ? 0 : Math.Min(signatures.Count, _pool.Count);

			for (int i = 0; i < _pool.Count; i++)
			{
				if (i < count)
				{
					_pool[i].gameObject.SetActive(true);
					_pool[i].SetText(signatures[i], _config.SuggestionPaddingX);
				}
				else
				{
					_pool[i].gameObject.SetActive(false);
				}
			}
		}
	}
}