using System;
using System.Globalization;
using UnityEngine;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Parsing.Types;

namespace UShell.Runtime.Unity.Parsing.Types
{
	public sealed class Vector3Parser : TypeParser<Vector3>
	{
		public override ExecutionResult<Vector3> ParseTyped(string input)
		{
			string[] parts = input.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length != 3)
			{
				return ExecutionResult<Vector3>.Failure($"Cannot parse '{input}' as Vector3. Expected format: 'x,y,z' or 'x y z'.");
			}

			if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
				float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
				float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
			{
				return ExecutionResult<Vector3>.Success(new Vector3(x, y, z));
			}

			return ExecutionResult<Vector3>.Failure($"Cannot parse '{input}' as Vector3. Invalid number format.");
		}
	}
}