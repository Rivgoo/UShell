using System;
using System.Globalization;
using UnityEngine;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Parsing.Types;

namespace UShell.Runtime.Unity.Parsing.Types
{
	/// <summary>
	/// Parses a <see cref="UnityEngine.Vector2Int"/> from a comma or space-separated string.
	/// </summary>
	/// <remarks>Valid inputs include <c>10, 20</c> or <c>10 20</c>.</remarks>
	public sealed class Vector2IntParser : TypeParser<Vector2Int>
	{
		/// <inheritdoc/>
		public override ExecutionResult<Vector2Int> ParseTyped(string input)
		{
			string[] parts = input.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length != 2)
				return ExecutionResult<Vector2Int>.Failure(ShellError.Create(ShellErrorCode.Bind_CustomError, -1, $"Cannot parse '{input}' as Vector2Int. Expected format: 'x,y' or 'x y'."));

			if (int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int x) &&
				int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int y))
			{
				return ExecutionResult<Vector2Int>.Success(new Vector2Int(x, y));
			}

			return ExecutionResult<Vector2Int>.Failure(ShellError.Create(ShellErrorCode.Bind_TypeMismatch, -1, input, "Vector2Int"));
		}
	}
}