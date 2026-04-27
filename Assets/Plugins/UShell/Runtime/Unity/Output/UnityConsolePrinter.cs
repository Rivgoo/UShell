using System;
using System.Collections.Generic;
using UnityEngine;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Core.Output.Formatting;

using LogType = UShell.Runtime.Core.Output.LogType;

namespace UShell.Runtime.Unity.Output
{
	public sealed class UnityConsolePrinter : IConsolePrinter
	{
		public event Action<LogEntry> OnLogAdded = delegate { };
		public event Action<Guid, LogEntry> OnLogUpdated = delegate { };

		public bool MirrorToUnityConsole { get; set; }

		public UnityConsolePrinter(bool mirrorToUnityConsole = true)
		{
			MirrorToUnityConsole = mirrorToUnityConsole;
		}

		public void Print(LogEntry entry)
		{
			OnLogAdded.Invoke(entry);

			if (MirrorToUnityConsole)
			{
				MirrorToEngine(entry);
			}
		}

		public void UpdatePrint(Guid id, LogEntry entry)
		{
			OnLogUpdated.Invoke(id, entry);
		}

		public void PrintTable(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows, TableStyle style = TableStyle.Standard)
		{
			string table = TableBuilder.BuildAsciiTable(headers, rows, style);
			Print(new LogEntry(table, LogType.Standard));
		}

		private void MirrorToEngine(LogEntry entry)
		{
			string prefix = "[UShell] ";

			switch (entry.Type)
			{
				case LogType.Error:
					Debug.LogError($"{prefix}{entry.Message}");
					break;
				case LogType.Warning:
					Debug.LogWarning($"{prefix}{entry.Message}");
					break;
				case LogType.Success:
				case LogType.Standard:
				default:
					Debug.Log($"{prefix}{entry.Message}");
					break;
			}
		}
	}
}