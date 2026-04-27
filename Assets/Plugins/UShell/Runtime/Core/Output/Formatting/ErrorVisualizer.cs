using System;

namespace UShell.Runtime.Core.Output.Formatting
{
	/// <summary>
	/// Utility for rendering intuitive diagnostic visuals for syntax and execution errors.
	/// </summary>
	public static class ErrorVisualizer
	{
		private const int MaxQueryLengthWithoutTruncation = 100;
		private const int WindowSize = 25;

		/// <summary>
		/// Formats a multi-line string containing the user's input, an ASCII pointer (<c>----^</c>) 
		/// indicating the exact column where the error occurred, and the explanation message.
		/// </summary>
		/// <param name="input">The raw text entered by the user.</param>
		/// <param name="position">The zero-based index of the problematic character.</param>
		/// <param name="errorMessage">The human-readable error description.</param>
		/// <returns>A formatted diagnostic string.</returns>
		public static string GenerateErrorPointer(string input, int position, string errorMessage)
		{
			if (string.IsNullOrEmpty(input))
			{
				return errorMessage;
			}

			int clampedPos = Math.Max(0, Math.Min(position, input.Length));

			if (input.Length <= MaxQueryLengthWithoutTruncation)
			{
				string pointer = new string('-', clampedPos) + "^";
				return $"{pointer}\n{errorMessage}";
			}

			int start = Math.Max(0, clampedPos - WindowSize);
			int end = Math.Min(input.Length, clampedPos + WindowSize);

			string prefix = start > 0 ? "..." : "";
			string suffix = end < input.Length ? "..." : "";

			string fragment = prefix + input.Substring(start, end - start) + suffix;

			int relativePos = clampedPos - start + prefix.Length;
			string localPointer = new string('-', relativePos) + "^";

			return $"{fragment}\n{localPointer}\n{errorMessage}";
		}
	}
}