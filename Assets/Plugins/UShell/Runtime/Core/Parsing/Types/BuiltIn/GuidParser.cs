using System;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core.Parsing.Types.BuiltIn
{
	/// <summary>Parses a globally unique identifier (GUID).</summary>
	public sealed class GuidParser : TypeParser<Guid>
	{
		/// <inheritdoc/>
		public override ExecutionResult<Guid> ParseTyped(string input)
		{
			if (Guid.TryParse(input, out Guid result))
				return ExecutionResult<Guid>.Success(result);

			return ExecutionResult<Guid>.Failure(ShellError.Create(ShellErrorCode.Bind_TypeMismatch, -1, input, "Guid"));
		}
	}
}