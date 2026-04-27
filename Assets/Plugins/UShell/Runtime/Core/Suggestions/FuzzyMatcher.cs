using System;

namespace UShell.Runtime.Core.Suggestions
{
	/// <summary>
	/// Exposes algorithmic logic to score string similarities for autocompletion mechanics.
	/// </summary>
	public static class FuzzyMatcher
	{
		/// <summary>
		/// Evaluates how closely a search query matches a target string based on sequence and casing.
		/// </summary>
		/// <param name="query">The user's typed input.</param>
		/// <param name="target">The candidate string to evaluate.</param>
		/// <returns>A positive integer score indicating match strength, or -1 if it does not match.</returns>
		public static int Score(string query, string target)
		{
			return Score(query.AsSpan(), target.AsSpan());
		}

		/// <summary>
		/// Evaluates how closely a search query matches a target string using memory-efficient Spans.
		/// </summary>
		public static int Score(ReadOnlySpan<char> query, ReadOnlySpan<char> target)
		{
			if (query.IsEmpty) return 0;
			if (target.IsEmpty) return -1;

			if (query.Equals(target, StringComparison.OrdinalIgnoreCase)) return 1000;

			if (target.StartsWith(query, StringComparison.OrdinalIgnoreCase))
				return 500 + (100 - target.Length);

			int qIdx = 0;
			int tIdx = 0;
			int score = 0;
			bool prevMatch = false;

			while (qIdx < query.Length && tIdx < target.Length)
			{
				char qc = char.ToLowerInvariant(query[qIdx]);
				char tc = char.ToLowerInvariant(target[tIdx]);

				if (qc == tc)
				{
					score += 10;

					if (prevMatch) score += 5;

					if (tIdx == 0 || target[tIdx - 1] == '.' || target[tIdx - 1] == '_' || target[tIdx - 1] == '-' || char.IsUpper(target[tIdx]))
					{
						score += 15;
					}

					qIdx++;
					prevMatch = true;
				}
				else
				{
					prevMatch = false;
				}

				tIdx++;
			}

			return qIdx == query.Length ? score : -1;
		}
	}
}