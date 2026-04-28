using System;
using System.Globalization;
using UnityEngine;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Parsing.Types;

namespace UShell.Runtime.Unity.Parsing.Types
{
	/// <summary>
	/// Parses a <see cref="UnityEngine.Rect"/> from a comma or space-separated string (x, y, width, height).
	/// </summary>
	public sealed class RectParser : TypeParser<Rect>
	{
		/// <inheritdoc/>
		public override ExecutionResult<Rect> ParseTyped(string input)
		{
			string[] parts = input.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length != 4)
				return ExecutionResult<Rect>.Failure(ShellError.Create(ShellErrorCode.Bind_CustomError, -1, $"Cannot parse '{input}' as Rect. Expected format: 'x,y,width,height'."));

			if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
				float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
				float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float w) &&
				float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float h))
			{
				return ExecutionResult<Rect>.Success(new Rect(x, y, w, h));
			}

			return ExecutionResult<Rect>.Failure(ShellError.Create(ShellErrorCode.Bind_TypeMismatch, -1, input, "Rect"));
		}
	}
}