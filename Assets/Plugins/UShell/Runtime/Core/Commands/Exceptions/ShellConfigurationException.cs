using System;

namespace UShell.Runtime.Core.Commands.Exceptions
{
	public sealed class ShellConfigurationException : Exception
	{
		public ShellConfigurationException(string message) : base(message)
		{
		}
	}
}