namespace UShell.Runtime.Core.Output
{
	/// <summary>
	/// Represents the semantic severity or category of a console log entry.
	/// </summary>
	/// <remarks>
	/// UI implementations map these types to specific colors and icons.
	/// </remarks>
	public enum LogType : byte
	{
		/// <summary>A standard informational message or command result.</summary>
		Standard = 0,

		/// <summary>A positive confirmation of a successful operation.</summary>
		Success = 1,

		/// <summary>A non-fatal issue or advisory note.</summary>
		Warning = 2,

		/// <summary>A critical failure or execution exception.</summary>
		Error = 3
	}
}