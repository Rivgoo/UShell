using System;

namespace UShell.Runtime.Core.Execution.Context
{
	public interface IProgressReporter : IDisposable
	{
		void Report(float progress, string status = "");
		void Fail(string reason);
	}
}