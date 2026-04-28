using UnityEngine;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Parsing.Types;

namespace UShell.Runtime.Unity.Parsing.Types
{
	/// <summary>
	/// Locates a GameObject by name in the active scene and resolves its <see cref="UnityEngine.Transform"/> component.
	/// </summary>
	public sealed class TransformParser : TypeParser<Transform>
	{
		/// <inheritdoc/>
		public override ExecutionResult<Transform> ParseTyped(string input)
		{
			GameObject target = GameObject.Find(input);

			if (target != null)
			{
				return ExecutionResult<Transform>.Success(target.transform);
			}

			return ExecutionResult<Transform>.Failure(
				ShellError.Create(ShellErrorCode.Bind_CustomError, -1,
					$"GameObject with name '{input}' not found in the active scene."));
		}
	}
}