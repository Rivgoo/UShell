using System;

namespace UShell.Runtime.Core.Commands.Fluent
{
	public interface ICommandConfigurator
	{
		ICommandConfigurator WithDescription(string description);
		ICommandConfigurator WithAlias(string alias);
		ICommandConfigurator RestrictedTo(EnvironmentTag tags);
		ICommandConfigurator AddParameter<T>(string name);
		ICommandConfigurator AddOptionalParameter<T>(string name, T defaultValue);
		void Executes(Action action);
		void Executes<T1>(Action<T1> action);
		void Executes<T1, T2>(Action<T1, T2> action);
		void Executes<T1, T2, T3>(Action<T1, T2, T3> action);
		void Executes<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action);
		void Executes<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action);
	}
}