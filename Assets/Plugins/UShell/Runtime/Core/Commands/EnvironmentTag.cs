using System;

namespace UShell.Runtime.Core.Commands
{
	[Flags]
	public enum EnvironmentTag : byte
	{
		None = 0,
		Editor = 1 << 0,
		Development = 1 << 1,
		Release = 1 << 2,
		Any = Editor | Development | Release
	}
}