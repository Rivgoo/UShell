using UnityEngine;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Parsing.Types;

namespace UShell.Runtime.Unity.Parsing.Types
{
	public sealed class GameObjectParser : TypeParser<GameObject>
	{
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