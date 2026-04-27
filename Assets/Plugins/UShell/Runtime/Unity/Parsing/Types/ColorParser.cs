using UnityEngine;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Parsing.Types;

namespace UShell.Runtime.Unity.Parsing.Types
{
	/// <summary>
	/// Parses standard Unity <see cref="UnityEngine.Color"/> objects from hex codes or named color strings.
	/// </summary>
	/// <remarks>
	/// Supports formatting like <c>#FF0000</c>, <c>FF0000</c>, or valid Unity color names like <c>red</c>, <c>white</c>.
	/// </remarks>
	public sealed class ColorParser : TypeParser<Color>
	{
		/// <inheritdoc/>
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