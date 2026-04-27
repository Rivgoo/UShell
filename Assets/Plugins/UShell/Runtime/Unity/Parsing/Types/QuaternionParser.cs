using System;
using System.Globalization;
using UnityEngine;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Parsing.Types;

namespace UShell.Runtime.Unity.Parsing.Types
{
	/// <summary>
	/// Parses a <see cref="UnityEngine.Quaternion"/> from comma-separated Euler angles or raw 4-component coordinates.
	/// </summary>
	/// <remarks>
	/// Supports <c>x,y,z</c> (interpreted as Euler angles) and <c>x,y,z,w</c> (interpreted as raw quaternion components).
	/// </remarks>
	public sealed class QuaternionParser : TypeParser<Quaternion>
	{
		/// <inheritdoc/>
		public override ExecutionResult<Quaternion> ParseTyped(string input)
		{
			string[] parts = input.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length == 3 &&
				TryParseFloat(parts[0], out float ex) &&
				TryParseFloat(parts[1], out float ey) &&
				TryParseFloat(parts[2], out float ez))
			{
				return ExecutionResult<Quaternion>.Success(Quaternion.Euler(ex, ey, ez));
			}

			if (parts.Length == 4 &&
				TryParseFloat(parts[0], out float qx) &&
				TryParseFloat(parts[1], out float qy) &&
				TryParseFloat(parts[2], out float qz) &&
				TryParseFloat(parts[3], out float qw))
			{
				return ExecutionResult<Quaternion>.Success(new Quaternion(qx, qy, qz, qw));
			}

			return ExecutionResult<Quaternion>.Failure(
				ShellError.Create(ShellErrorCode.Bind_CustomError, -1,
					$"Cannot parse '{input}' as Quaternion. Expected Euler 'x,y,z' or components 'x,y,z,w'."));
		}

		private static bool TryParseFloat(string s, out float v)
		{
			return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v);
		}
	}
}