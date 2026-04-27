using System;

namespace UShell.Runtime.Core.Output
{
	public readonly struct LogEntry
	{
		public string Message { get; }
		public LogType Type { get; }
		public DateTime Timestamp { get; }
		public Guid? Id { get; }

		public LogEntry(string message, LogType type, Guid? id = null)
		{
			Message = message ?? string.Empty;
			Type = type;
			Timestamp = DateTime.UtcNow;
			Id = id;
		}
	}
}