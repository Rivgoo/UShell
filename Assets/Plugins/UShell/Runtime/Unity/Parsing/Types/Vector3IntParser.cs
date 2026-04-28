using System;
using System.Globalization;
using UnityEngine;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Parsing.Types;

namespace UShell.Runtime.Unity.Parsing.Types
{
	/// <summary>
	/// Parses a <see cref="UnityEngine.Vector3Int"/> from a comma or space-separated string.
	/// </summary>
	/// <remarks>Valid inputs include <c>10, 20, 30</c> or <c>10 20 30</c>.</remarks>
	public sealed class Vector3IntParser : TypeParser<Vector3Int>
	{
		/// <inheritdoc/>
		public override ExecutionResult<Vector3Int> ParseTyped(string input)
		{
			string[] parts = input.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length != 3)
				return ExecutionResult<Vector3Int>.Failure(ShellError.Create(ShellErrorCode.Bind_CustomError, -1, $"Cannot parse '{input}' as Vector3Int. Expected format: 'x,y,z' or 'x y z'."));

			if (int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int x) &&
				int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int y) &&
				int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int z))
			{
				return ExecutionResult<Vector3Int>.Success(new Vector3Int(x, y, z));
			}

			return ExecutionResult<Vector3Int>.Failure(ShellError.Create(ShellErrorCode.Bind_TypeMismatch, -1, input, "Vector3Int"));
		}
	}
}