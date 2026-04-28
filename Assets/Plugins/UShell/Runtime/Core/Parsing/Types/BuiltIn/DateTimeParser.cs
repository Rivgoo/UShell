using System;
using System.Globalization;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core.Parsing.Types.BuiltIn
{
	/// <summary>Parses a date and time string (e.g., "2024-05-15 14:30:00").</summary>
	public sealed class DateTimeParser : TypeParser<DateTime>
	{
		/// <inheritdoc/>
		public override ExecutionResult<DateTime> ParseTyped(string input)
		{
			if (DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
				return ExecutionResult<DateTime>.Success(result);

			return ExecutionResult<DateTime>.Failure(ShellError.Create(ShellErrorCode.Bind_TypeMismatch, -1, input, "DateTime"));
		}
	}
}