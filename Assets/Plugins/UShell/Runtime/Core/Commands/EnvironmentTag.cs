using System;

namespace UShell.Runtime.Core.Commands
{
	/// <summary>
	/// Represents bitmask flags indicating the target environments where a command is permitted to execute.
	/// </summary>
	/// <remarks>
	/// Used during bootstrapping to filter out commands that shouldn't exist in specific builds 
	/// (e.g., removing cheating/debugging commands in the final <see cref="Release"/> build).
	/// </remarks>
	[Flags]
	public enum EnvironmentTag : byte
	{
		/// <summary>The command is disabled entirely.</summary>
		None = 0,

		/// <summary>The command is only available when running inside the Unity Editor.</summary>
		Editor = 1 << 0,

		/// <summary>The command is available in Development builds.</summary>
		Development = 1 << 1,

		/// <summary>The command is available in final Release builds.</summary>
		Release = 1 << 2,

		/// <summary>The command is universally available in all environments.</summary>
		Any = Editor | Development | Release
	}
}