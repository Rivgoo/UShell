using UnityEngine;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Parsing.Types;

namespace UShell.Runtime.Unity.Parsing.Types
{
	/// <summary>
	/// Parses a <see cref="UnityEngine.GameObject"/> by locating it directly within the active scene hierarchy by name.
	/// </summary>
	public sealed class GameObjectParser : TypeParser<GameObject>
	{
		/// <inheritdoc/>
		public override ExecutionResult<GameObject> ParseTyped(string input)
		{
			GameObject target = GameObject.Find(input);

			if (target != null)
			{
				return ExecutionResult<GameObject>.Success(target);
			}

			return ExecutionResult<GameObject>.Failure(
				ShellError.Create(ShellErrorCode.Bind_CustomError, -1,
					$"GameObject with name '{input}' not found in the active scene."));
		}
	}
}