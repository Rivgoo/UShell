using System;
using System.Globalization;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core.Parsing.Types.BuiltIn
{
	/// <summary>Parses a time interval (e.g., "01:30:00" for 1.5 hours).</summary>
	public sealed class TimeSpanParser : TypeParser<TimeSpan>
	{
		/// <inheritdoc/>
		public override ExecutionResult<TimeSpan> ParseTyped(string input)
		{
			if (TimeSpan.TryParse(input, CultureInfo.InvariantCulture, out TimeSpan result))
				return ExecutionResult<TimeSpan>.Success(result);

			return ExecutionResult<TimeSpan>.Failure(ShellError.Create(ShellErrorCode.Bind_TypeMismatch, -1, input, "TimeSpan"));
		}
	}
}