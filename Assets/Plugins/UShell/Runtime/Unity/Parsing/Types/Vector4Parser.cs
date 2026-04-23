using System;
using System.Globalization;
using UnityEngine;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Parsing.Types;

namespace UShell.Runtime.Unity.Parsing.Types
{
	public sealed class Vector4Parser : TypeParser<Vector4>
	{
		public override ExecutionResult<Vector4> ParseTyped(string input)
		{
			string[] parts = input.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length != 4)
				return ExecutionResult<Vector4>.Failure(
					$"Cannot parse '{input}' as Vector4. Expected format: 'x,y,z,w'.");

			if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
				float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
				float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z) &&
				float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float w))
				return ExecutionResult<Vector4>.Success(new Vector4(x, y, z, w));

			return ExecutionResult<Vector4>.Failure($"Cannot parse '{input}' as Vector4. Invalid number format.");
		}
	}
}