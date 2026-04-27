using System;
using System.Collections.Generic;
using UnityEngine;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Core.Output.Formatting;

using LogType = UShell.Runtime.Core.Output.LogType;

namespace UShell.Runtime.Unity.Output
{
	/// <summary>
	/// Connects the shell's logging mechanisms to Unity's native <see cref="UnityEngine.Debug"/> pipeline.
	/// </summary>
	public sealed class UnityConsolePrinter : IConsolePrinter
	{
		/// <inheritdoc/>
		public event Action<LogEntry> OnLogAdded = delegate { };

		/// <inheritdoc/>
		public event Action<Guid, LogEntry> OnLogUpdated = delegate { };

		/// <summary>
		/// When <c>true</c>, all logs intercepted by UShell will also be printed directly 
		/// into the standard Unity Editor Console window.
		/// </summary>
		public bool MirrorToUnityConsole { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="UnityConsolePrinter"/> class.
		/// </summary>
		public UnityConsolePrinter(bool mirrorToUnityConsole = true)
		{
			MirrorToUnityConsole = mirrorToUnityConsole;
		}

		/// <inheritdoc/>
		public void Print(LogEntry entry)
		{
			OnLogAdded.Invoke(entry);

			if (MirrorToUnityConsole)
			{
				MirrorToEngine(entry);
			}
		}

		/// <inheritdoc/>
		public void UpdatePrint(Guid id, LogEntry entry)
		{
			OnLogUpdated.Invoke(id, entry);
		}

		/// <inheritdoc/>
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