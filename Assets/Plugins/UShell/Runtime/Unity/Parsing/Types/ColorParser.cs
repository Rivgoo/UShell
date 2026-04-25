using UnityEngine;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Parsing.Types;

namespace UShell.Runtime.Unity.Parsing.Types
{
	public sealed class ColorParser : TypeParser<Color>
	{
		public override ExecutionResult<Color> ParseTyped(string input)
		{
			string colorString = input.StartsWith("#") ? input : $"#{input}";

			if (ColorUtility.TryParseHtmlString(colorString, out Color color))
			{
				return ExecutionResult<Color>.Success(color);
			}

			if (ColorUtility.TryParseHtmlString(input, out Color namedColor))
			{
				return ExecutionResult<Color>.Success(namedColor);
			}

			return ExecutionResult<Color>.Failure(
				ShellError.Create(ShellErrorCode.Bind_CustomError, -1,
					$"Cannot parse '{input}' as Color. Expected hex format (e.g., #FF0000) or valid name."));
		}
	}
}