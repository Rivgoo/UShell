using System;
using System.Threading.Tasks;

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

		void ExecutesReturning<TResult>(Func<TResult> func);
		void ExecutesReturning<T1, TResult>(Func<T1, TResult> func);
		void ExecutesReturning<T1, T2, TResult>(Func<T1, T2, TResult> func);
		void ExecutesReturning<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func);
		void ExecutesReturning<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func);
		void ExecutesReturning<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func);

		void ExecutesAsync(Func<Task> action);
		void ExecutesAsync<T1>(Func<T1, Task> action);
		void ExecutesAsync<T1, T2>(Func<T1, T2, Task> action);
		void ExecutesAsync<T1, T2, T3>(Func<T1, T2, T3, Task> action);
		void ExecutesAsync<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Task> action);
		void ExecutesAsync<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, Task> action);
	}
}