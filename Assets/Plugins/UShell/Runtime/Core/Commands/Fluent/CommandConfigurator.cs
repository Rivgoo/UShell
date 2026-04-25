#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UShell.Runtime.Core.Exceptions;
using UShell.Runtime.Core.Execution.Invocation;
using UShell.Runtime.Core.Registry;

namespace UShell.Runtime.Core.Commands.Fluent
{
	internal sealed class CommandConfigurator : ICommandConfigurator
	{
		private readonly string _name;
		private readonly List<string> _aliases = new();
		private readonly List<CommandParameter> _parameters = new();

		private string _description = string.Empty;
		private EnvironmentTag _tags = EnvironmentTag.Any;
		private ICommandInvoker? _invoker;

		public CommandConfigurator(string name)
		{
			CommandValidator.ValidateCommandName(name);
			_name = name;
		}

		public ICommandConfigurator WithDescription(string description)
		{
			_description = description ?? string.Empty;
			return this;
		}

		public ICommandConfigurator WithAlias(string alias)
		{
			CommandValidator.ValidateCommandName(alias);
			_aliases.Add(alias);
			return this;
		}

		public ICommandConfigurator RestrictedTo(EnvironmentTag tags)
		{
			_tags = tags;
			return this;
		}

		public ICommandConfigurator AddParameter<T>(string name)
		{
			CommandValidator.ValidateParameterName(_name, name);
			_parameters.Add(new CommandParameter(name, typeof(T), false, null));
			return this;
		}

		public ICommandConfigurator AddOptionalParameter<T>(string name, T defaultValue)
		{
			CommandValidator.ValidateParameterName(_name, name);
			_parameters.Add(new CommandParameter(name, typeof(T), true, defaultValue));
			return this;
		}

		public void Executes(Action action) { ValidateTypes(); AssignInvoker(new ActionInvoker(action)); }
		public void Executes<T1>(Action<T1> action) { ValidateTypes(typeof(T1)); AssignInvoker(new ActionInvoker<T1>(action)); }
		public void Executes<T1, T2>(Action<T1, T2> action) { ValidateTypes(typeof(T1), typeof(T2)); AssignInvoker(new ActionInvoker<T1, T2>(action)); }
		public void Executes<T1, T2, T3>(Action<T1, T2, T3> action) { ValidateTypes(typeof(T1), typeof(T2), typeof(T3)); AssignInvoker(new ActionInvoker<T1, T2, T3>(action)); }
		public void Executes<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action) { ValidateTypes(typeof(T1), typeof(T2), typeof(T3), typeof(T4)); AssignInvoker(new ActionInvoker<T1, T2, T3, T4>(action)); }
		public void Executes<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action) { ValidateTypes(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5)); AssignInvoker(new ActionInvoker<T1, T2, T3, T4, T5>(action)); }

		public void ExecutesReturning<TResult>(Func<TResult> func) { ValidateTypes(); AssignInvoker(new FuncInvoker<TResult>(func)); }
		public void ExecutesReturning<T1, TResult>(Func<T1, TResult> func) { ValidateTypes(typeof(T1)); AssignInvoker(new FuncInvoker<T1, TResult>(func)); }
		public void ExecutesReturning<T1, T2, TResult>(Func<T1, T2, TResult> func) { ValidateTypes(typeof(T1), typeof(T2)); AssignInvoker(new FuncInvoker<T1, T2, TResult>(func)); }
		public void ExecutesReturning<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func) { ValidateTypes(typeof(T1), typeof(T2), typeof(T3)); AssignInvoker(new FuncInvoker<T1, T2, T3, TResult>(func)); }
		public void ExecutesReturning<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func) { ValidateTypes(typeof(T1), typeof(T2), typeof(T3), typeof(T4)); AssignInvoker(new FuncInvoker<T1, T2, T3, T4, TResult>(func)); }
		public void ExecutesReturning<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func) { ValidateTypes(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5)); AssignInvoker(new FuncInvoker<T1, T2, T3, T4, T5, TResult>(func)); }

		public void ExecutesAsync(Func<Task> action) { ValidateTypes(); AssignInvoker(new FuncInvoker<Task>(action)); }
		public void ExecutesAsync<T1>(Func<T1, Task> action) { ValidateTypes(typeof(T1)); AssignInvoker(new FuncInvoker<T1, Task>(action)); }
		public void ExecutesAsync<T1, T2>(Func<T1, T2, Task> action) { ValidateTypes(typeof(T1), typeof(T2)); AssignInvoker(new FuncInvoker<T1, T2, Task>(action)); }
		public void ExecutesAsync<T1, T2, T3>(Func<T1, T2, T3, Task> action) { ValidateTypes(typeof(T1), typeof(T2), typeof(T3)); AssignInvoker(new FuncInvoker<T1, T2, T3, Task>(action)); }
		public void ExecutesAsync<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Task> action) { ValidateTypes(typeof(T1), typeof(T2), typeof(T3), typeof(T4)); AssignInvoker(new FuncInvoker<T1, T2, T3, T4, Task>(action)); }
		public void ExecutesAsync<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, Task> action) { ValidateTypes(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5)); AssignInvoker(new FuncInvoker<T1, T2, T3, T4, T5, Task>(action)); }

		private void AssignInvoker(ICommandInvoker invoker)
		{
			_invoker = invoker ?? throw new ArgumentNullException(nameof(invoker));
		}

		internal CommandSignature Build()
		{
			if (_invoker == null)
			{
				throw new ShellConfigurationException($"Command '{_name}' has no execution delegate assigned. Did you forget to call .Executes()?");
			}

			return new CommandSignature(
				_name,
				_description,
				new List<string>(_aliases).AsReadOnly(),
				_tags,
				new List<CommandParameter>(_parameters).AsReadOnly(),
				_invoker);
		}

		private void ValidateTypes(params Type[] expectedTypes)
		{
			if (_parameters.Count != expectedTypes.Length)
			{
				throw new ShellConfigurationException(
					$"Command '{_name}' signature mismatch. Registered {_parameters.Count} parameters, but Executes expects {expectedTypes.Length}.");
			}

			for (int index = 0; index < expectedTypes.Length; index++)
			{
				Type expectedType = _parameters[index].ParameterType;
				Type actualType = expectedTypes[index];

				if (expectedType != actualType)
				{
					throw new ShellConfigurationException(
						$"Command '{_name}' type mismatch at parameter index {index} ('{_parameters[index].Name}'). " +
						$"Registered as '{expectedType.Name}', but Executes expects '{actualType.Name}'.");
				}
			}
		}
	}
}