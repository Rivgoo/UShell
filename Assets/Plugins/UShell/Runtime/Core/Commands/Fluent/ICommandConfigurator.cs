using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UShell.Runtime.Core.Execution.Context;
using UShell.Runtime.Core.Suggestions;

namespace UShell.Runtime.Core.Commands.Fluent
{
	/// <summary>
	/// Provides a fluent API for defining a command's metadata, parameters, suggestions, and execution delegate.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Order matters when adding parameters: positional arguments passed by the user will be bound 
	/// in the exact order they are added here via <see cref="AddParameter{T}"/> and <see cref="AddOptionalParameter{T}"/>.
	/// </para>
	/// <para>
	/// Every configurator chain must end with a call to one of the <c>Executes...</c> methods to bind the runtime logic.
	/// </para>
	/// </remarks>
	public interface ICommandConfigurator
	{
		/// <summary>
		/// Sets the human-readable description displayed in the <c>help</c> command.
		/// </summary>
		/// <param name="description">A short summary of what the command does.</param>
		/// <returns>The current <see cref="ICommandConfigurator"/> instance for method chaining.</returns>
		ICommandConfigurator WithDescription(string description);

		/// <summary>
		/// Adds an alternative name that can trigger this command.
		/// </summary>
		/// <param name="alias">The alternative name (e.g., <c>rm</c> for <c>remove</c>).</param>
		/// <returns>The current <see cref="ICommandConfigurator"/> instance for method chaining.</returns>
		ICommandConfigurator WithAlias(string alias);

		/// <summary>
		/// Restricts the availability of this command to specific runtime environments.
		/// </summary>
		/// <remarks>
		/// If not specified, commands default to <see cref="EnvironmentTag.Any"/>. 
		/// Useful for hiding cheat commands in release builds.
		/// </remarks>
		/// <param name="tags">A bitmask of environments where this command is valid.</param>
		/// <returns>The current <see cref="ICommandConfigurator"/> instance for method chaining.</returns>
		ICommandConfigurator RestrictedTo(EnvironmentTag tags);

		/// <summary>
		/// Registers a required parameter for the command.
		/// </summary>
		/// <typeparam name="T">The expected data type. Must have a registered <see cref="UShell.Runtime.Core.Parsing.Types.ITypeParser{T}"/>.</typeparam>
		/// <param name="name">The name of the parameter (e.g., "id" for <c>-id</c>).</param>
		/// <returns>The current <see cref="ICommandConfigurator"/> instance for method chaining.</returns>
		ICommandConfigurator AddParameter<T>(string name);

		/// <summary>
		/// Registers an optional parameter with a fallback value.
		/// </summary>
		/// <remarks>
		/// Optional parameters must be defined after all required parameters to avoid binding ambiguity.
		/// </remarks>
		/// <typeparam name="T">The expected data type.</typeparam>
		/// <param name="name">The name of the parameter.</param>
		/// <param name="defaultValue">The value injected if the user omits this parameter.</param>
		/// <returns>The current <see cref="ICommandConfigurator"/> instance for method chaining.</returns>
		ICommandConfigurator AddOptionalParameter<T>(string name, T defaultValue);

		/// <summary>
		/// Attaches a custom suggestion provider to the last added parameter.
		/// </summary>
		/// <param name="provider">The provider instance resolving suggestions based on user input.</param>
		/// <returns>The current <see cref="ICommandConfigurator"/> instance for method chaining.</returns>
		/// <exception cref="UShell.Runtime.Core.Exceptions.ShellConfigurationException">Thrown if no parameters have been added yet.</exception>
		ICommandConfigurator WithSuggestions(ISuggestionProvider provider);

		/// <summary>
		/// Attaches a static list of suggestions to the last added parameter.
		/// </summary>
		/// <param name="suggestions">A fixed collection of strings offered during autocomplete.</param>
		/// <returns>The current <see cref="ICommandConfigurator"/> instance for method chaining.</returns>
		ICommandConfigurator WithSuggestions(IEnumerable<string> suggestions);

		/// <summary>
		/// Attaches a dynamic, delegate-based suggestion provider to the last added parameter.
		/// </summary>
		/// <param name="provider">A function resolving suggestions based on the current input context.</param>
		/// <returns>The current <see cref="ICommandConfigurator"/> instance for method chaining.</returns>
		ICommandConfigurator WithSuggestions(Func<SuggestionContext, IEnumerable<string>> provider);

		/// <summary>
		/// Specifies the maximum execution duration for an interactive command before it is automatically cancelled.
		/// </summary>
		/// <remarks>
		/// This must be called before <c>ExecutesInteractiveAsync</c>.
		/// </remarks>
		/// <param name="timeout">The allowed time limit.</param>
		/// <returns>The current <see cref="ICommandConfigurator"/> instance for method chaining.</returns>
		ICommandConfigurator WithTimeout(TimeSpan timeout);

		/// <summary>Binds a synchronous execution delegate with no parameters.</summary>
		void Executes(Action action);

		/// <summary>Binds a synchronous execution delegate with 1 parameter.</summary>
		void Executes<T1>(Action<T1> action);

		/// <summary>Binds a synchronous execution delegate with 2 parameters.</summary>
		void Executes<T1, T2>(Action<T1, T2> action);

		/// <summary>Binds a synchronous execution delegate with 3 parameters.</summary>
		void Executes<T1, T2, T3>(Action<T1, T2, T3> action);

		/// <summary>Binds a synchronous execution delegate with 4 parameters.</summary>
		void Executes<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action);

		/// <summary>Binds a synchronous execution delegate with 5 parameters.</summary>
		void Executes<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action);

		/// <summary>Binds a synchronous execution delegate that returns a result.</summary>
		void ExecutesReturning<TResult>(Func<TResult> func);

		/// <summary>Binds a synchronous execution delegate with 1 parameter that returns a result.</summary>
		void ExecutesReturning<T1, TResult>(Func<T1, TResult> func);

		/// <summary>Binds a synchronous execution delegate with 2 parameters that returns a result.</summary>
		void ExecutesReturning<T1, T2, TResult>(Func<T1, T2, TResult> func);

		/// <summary>Binds a synchronous execution delegate with 3 parameters that returns a result.</summary>
		void ExecutesReturning<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func);

		/// <summary>Binds a synchronous execution delegate with 4 parameters that returns a result.</summary>
		void ExecutesReturning<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func);

		/// <summary>Binds a synchronous execution delegate with 5 parameters that returns a result.</summary>
		void ExecutesReturning<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func);

		/// <summary>Binds an asynchronous execution delegate returning a Task.</summary>
		void ExecutesAsync(Func<Task> action);

		/// <summary>Binds an asynchronous execution delegate with 1 parameter.</summary>
		void ExecutesAsync<T1>(Func<T1, Task> action);

		/// <summary>Binds an asynchronous execution delegate with 2 parameters.</summary>
		void ExecutesAsync<T1, T2>(Func<T1, T2, Task> action);

		/// <summary>Binds an asynchronous execution delegate with 3 parameters.</summary>
		void ExecutesAsync<T1, T2, T3>(Func<T1, T2, T3, Task> action);

		/// <summary>Binds an asynchronous execution delegate with 4 parameters.</summary>
		void ExecutesAsync<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Task> action);

		/// <summary>Binds an asynchronous execution delegate with 5 parameters.</summary>
		void ExecutesAsync<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, Task> action);

		/// <summary>
		/// Binds an interactive asynchronous execution delegate that has access to the command context.
		/// </summary>
		/// <remarks>
		/// Interactive commands lock the shell input, allowing the command to request prompts, display 
		/// progress bars, or await user confirmations via the injected <see cref="ICommandContext"/>.
		/// Requires <see cref="WithTimeout"/> to be called beforehand.
		/// </remarks>
		void ExecutesInteractiveAsync(Func<ICommandContext, Task> action);

		/// <summary>Binds an interactive asynchronous delegate with 1 parameter.</summary>
		void ExecutesInteractiveAsync<T1>(Func<ICommandContext, T1, Task> action);

		/// <summary>Binds an interactive asynchronous delegate with 2 parameters.</summary>
		void ExecutesInteractiveAsync<T1, T2>(Func<ICommandContext, T1, T2, Task> action);

		/// <summary>Binds an interactive asynchronous delegate with 3 parameters.</summary>
		void ExecutesInteractiveAsync<T1, T2, T3>(Func<ICommandContext, T1, T2, T3, Task> action);

		/// <summary>Binds an interactive asynchronous delegate with 4 parameters.</summary>
		void ExecutesInteractiveAsync<T1, T2, T3, T4>(Func<ICommandContext, T1, T2, T3, T4, Task> action);

		/// <summary>Binds an interactive asynchronous delegate with 5 parameters.</summary>
		void ExecutesInteractiveAsync<T1, T2, T3, T4, T5>(Func<ICommandContext, T1, T2, T3, T4, T5, Task> action);
	}
}