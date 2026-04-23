using System;
using System.Globalization;
using UnityEngine;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Parsing.Types;

namespace UShell.Runtime.Unity.Parsing.Types
{
	/// <summary>
	/// Accepts either Euler angles "x,y,z" or quaternion components "x,y,z,w".
	/// </summary>
	public sealed class QuaternionParser : TypeParser<Quaternion>
	{
		public override ExecutionResult<Quaternion> ParseTyped(string input)
		{
			string[] parts = input.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length == 3 &&
				TryParseFloat(parts[0], out float ex) &&
				TryParseFloat(parts[1], out float ey) &&
				TryParseFloat(parts[2], out float ez))
				return ExecutionResult<Quaternion>.Success(Quaternion.Euler(ex, ey, ez));

			if (parts.Length == 4 &&
				TryParseFloat(parts[0], out float qx) &&
				TryParseFloat(parts[1], out float qy) &&
				TryParseFloat(parts[2], out float qz) &&
				TryParseFloat(parts[3], out float qw))
				return ExecutionResult<Quaternion>.Success(new Quaternion(qx, qy, qz, qw));

			return ExecutionResult<Quaternion>.Failure(
				$"Cannot parse '{input}' as Quaternion. " +
				"Expected Euler 'x,y,z' or quaternion components 'x,y,z,w'.");
		}

		private static bool TryParseFloat(string s, out float v) =>
			float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v);
	}
}