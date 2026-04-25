using System.Text.RegularExpressions;
using UShell.Runtime.Core.Exceptions;

namespace UShell.Runtime.Core.Registry
{
	internal static class CommandValidator
	{
		private static readonly Regex NameRegex = new Regex(@"^[^\s\[\]"",]+$", RegexOptions.Compiled);

		private static readonly Regex ParameterRegex = new Regex(@"^\p{L}[^\s\[\]"",]*$", RegexOptions.Compiled);

		public static void ValidateCommandName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new InvalidCommandSignatureException("Command name cannot be empty or whitespace.");
			}

			if (!NameRegex.IsMatch(name))
			{
				throw new InvalidCommandSignatureException(
					$"Invalid command name or alias '{name}'. Names cannot contain spaces, commas (,), quotes (\"), or square brackets ([ ]).");
			}
		}

		public static void ValidateParameterName(string commandName, string parameterName)
		{
			if (string.IsNullOrWhiteSpace(parameterName))
			{
				throw new InvalidCommandSignatureException($"Parameter name in command '{commandName}' cannot be empty.");
			}

			if (!ParameterRegex.IsMatch(parameterName))
			{
				throw new InvalidCommandSignatureException(
					$"Invalid parameter name '{parameterName}' in command '{commandName}'. " +
					"Parameter names must start with a letter and cannot contain spaces, commas (,), quotes (\"), or square brackets ([ ]).");
			}
		}
	}
}