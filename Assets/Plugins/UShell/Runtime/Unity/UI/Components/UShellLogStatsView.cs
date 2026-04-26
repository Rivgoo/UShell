using UnityEngine;
using TMPro;

namespace UShell.Runtime.Unity.UI.Components
{
	public sealed class UShellLogStatsView : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _infoText = null!;
		[SerializeField] private TextMeshProUGUI _warningText = null!;
		[SerializeField] private TextMeshProUGUI _errorText = null!;

		public void UpdateStats(int infoCount, int warningCount, int errorCount)
		{
			_infoText.text = FormatCount(infoCount);
			_warningText.text = FormatCount(warningCount);
			_errorText.text = FormatCount(errorCount);
		}

		private static string FormatCount(int count)
		{
			return count > 999 ? "999+" : count.ToString();
		}
	}
}