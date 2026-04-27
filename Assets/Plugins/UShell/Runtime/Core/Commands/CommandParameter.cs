#nullable enable
using System;
using UShell.Runtime.Core.Suggestions;

namespace UShell.Runtime.Core.Commands
{
	public sealed class CommandParameter
	{
		public string Name { get; }
		public Type ParameterType { get; }
		public bool IsOptional { get; }
		public object? DefaultValue { get; }
		public ISuggestionProvider? SuggestionProvider { get; }

		public CommandParameter(string name, Type parameterType, bool isOptional, object? defaultValue, ISuggestionProvider? suggestionProvider = null)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Parameter name cannot be empty.", nameof(name));
			}

			Name = name;
			ParameterType = parameterType ?? throw new ArgumentNullException(nameof(parameterType));
			IsOptional = isOptional;
			DefaultValue = defaultValue;
			SuggestionProvider = suggestionProvider;
		}
	}
}