using System;
using System.Globalization;
using UnityEngine;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Parsing.Types;

namespace UShell.Runtime.Unity.Parsing.Types
{
	/// <summary>
	/// Parses <see cref="UnityEngine.Bounds"/> from a comma or space-separated string representing center and size (cx,cy,cz,sx,sy,sz).
	/// </summary>
	public sealed class BoundsParser : TypeParser<Bounds>
	{
		/// <inheritdoc/>
		public override ExecutionResult<Bounds> ParseTyped(string input)
		{
			string[] parts = input.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length != 6)
				return ExecutionResult<Bounds>.Failure(ShellError.Create(ShellErrorCode.Bind_CustomError, -1, $"Cannot parse '{input}' as Bounds. Expected format: 'centerX,centerY,centerZ,sizeX,sizeY,sizeZ'."));

			if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float cx) &&
				float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float cy) &&
				float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float cz) &&
				float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float sx) &&
				float.TryParse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float sy) &&
				float.TryParse(parts[5], NumberStyles.Float, CultureInfo.InvariantCulture, out float sz))
			{
				return ExecutionResult<Bounds>.Success(new Bounds(new Vector3(cx, cy, cz), new Vector3(sx, sy, sz)));
			}

			return ExecutionResult<Bounds>.Failure(ShellError.Create(ShellErrorCode.Bind_TypeMismatch, -1, input, "Bounds"));
		}
	}
}