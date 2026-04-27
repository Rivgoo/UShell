using System;

namespace UShell.Runtime.Core.Output
{
	/// <summary>
	/// An immutable payload containing all data required to render a single message in the console UI.
	/// </summary>
	public readonly struct LogEntry
	{
		/// <summary>
		/// The raw or rich-text formatted string content to display.
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// The semantic category of the log.
		/// </summary>
		public LogType Type { get; }

		/// <summary>
		/// The UTC time when this entry was created.
		/// </summary>
		public DateTime Timestamp { get; }

		/// <summary>
		/// An optional unique identifier used if this log needs to be dynamically updated later 
		/// via <see cref="IConsolePrinter.UpdatePrint"/>.
		/// </summary>
		public Guid? Id { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="LogEntry"/> struct.
		/// </summary>
		public LogEntry(string message, LogType type, Guid? id = null)
		{
			Message = message ?? string.Empty;
			Type = type;
			Timestamp = DateTime.UtcNow;
			Id = id;
		}
	}
}