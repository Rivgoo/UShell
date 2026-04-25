using System;

namespace UShell.Runtime.Core.Exceptions
{
	public class ShellConfigurationException : Exception
	{
		public ShellConfigurationException(string message) : base(message) { }
	}
}