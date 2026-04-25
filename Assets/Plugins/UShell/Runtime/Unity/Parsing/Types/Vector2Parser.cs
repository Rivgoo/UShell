using System;
using System.Globalization;
using UnityEngine;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Parsing.Types;

namespace UShell.Runtime.Unity.Parsing.Types
{
	public sealed class Vector2Parser : TypeParser<Vector2>
	{
		public override ExecutionResult<Vector2> ParseTyped(string input)
		{
			string[] parts = input.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length != 2)
			{
				return ExecutionResult<Vector2>.Failure(
					ShellError.Create(ShellErrorCode.Bind_CustomError, -1,
						$"Cannot parse '{input}' as Vector2. Expected format: 'x,y' or 'x y'."));
			}

			if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
				float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
			{
				return ExecutionResult<Vector2>.Success(new Vector2(x, y));
			}

			return ExecutionResult<Vector2>.Failure(
				ShellError.Create(ShellErrorCode.Bind_TypeMismatch, -1, input, "Vector2"));
		}
	}
}