#nullable enable
using System;

namespace UShell.Runtime.Core.Commands
{
	public sealed class CommandParameter
	{
		public string Name { get; }
		public Type ParameterType { get; }
		public bool IsOptional { get; }
		public object? DefaultValue { get; }

		public CommandParameter(string name, Type parameterType, bool isOptional, object? defaultValue)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Parameter name cannot be empty.", nameof(name));
			}

			Name = name;
			ParameterType = parameterType ?? throw new ArgumentNullException(nameof(parameterType));
			IsOptional = isOptional;
			DefaultValue = defaultValue;
		}
	}
}