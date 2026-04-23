#nullable enable
using System;
using System.Collections.Generic;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Parsing.Syntax;
using UShell.Runtime.Core.Parsing.Types;

namespace UShell.Runtime.Core.Execution.Binding
{
	internal sealed class ArgumentBinder : IArgumentBinder
	{
		private readonly ITypeParserRegistry _parserRegistry;

		public ArgumentBinder(ITypeParserRegistry parserRegistry)
		{
			_parserRegistry = parserRegistry ?? throw new ArgumentNullException(nameof(parserRegistry));
		}

		public ExecutionResult<object?[]> BindArguments(IReadOnlyList<CommandParameter> parameters, IReadOnlyList<SyntaxNode> arguments)
		{
			object?[] boundArguments = new object?[parameters.Count];
			bool[] isBound = new bool[parameters.Count];

			int positionalIndex = 0;
			bool isNamedSectionStarted = false;

			for (int index = 0; index < arguments.Count; index++)
			{
				SyntaxNode argumentNode = arguments[index];

				if (argumentNode is NamedArgumentNode namedArgument)
				{
					isNamedSectionStarted = true;
					int parameterIndex = FindParameterIndex(parameters, namedArgument.Name);

					if (parameterIndex == -1)
					{
						return ExecutionResult<object?[]>.Failure($"Unknown parameter '{namedArgument.Name}'.", namedArgument.StartIndex);
					}

					if (isBound[parameterIndex])
					{
						return ExecutionResult<object?[]>.Failure($"Parameter '{namedArgument.Name}' is already bound.", namedArgument.StartIndex);
					}

					var parseResult = ParseNode(namedArgument.Value, parameters[parameterIndex].ParameterType);
					if (!parseResult.IsSuccess)
					{
						return ExecutionResult<object?[]>.Failure($"Failed to parse parameter '{namedArgument.Name}': {parseResult.ErrorMessage}", namedArgument.Value.StartIndex);
					}

					boundArguments[parameterIndex] = parseResult.Value;
					isBound[parameterIndex] = true;
				}
				else
				{
					if (isNamedSectionStarted)
					{
						return ExecutionResult<object?[]>.Failure("Positional arguments cannot appear after named arguments.", argumentNode.StartIndex);
					}

					if (positionalIndex >= parameters.Count)
					{
						return ExecutionResult<object?[]>.Failure("Too many arguments provided.", argumentNode.StartIndex);
					}

					var parseResult = ParseNode(argumentNode, parameters[positionalIndex].ParameterType);
					if (!parseResult.IsSuccess)
					{
						return ExecutionResult<object?[]>.Failure($"Failed to parse parameter '{parameters[positionalIndex].Name}': {parseResult.ErrorMessage}", argumentNode.StartIndex);
					}

					boundArguments[positionalIndex] = parseResult.Value;
					isBound[positionalIndex] = true;
					positionalIndex++;
				}
			}

			return ApplyDefaultValues(parameters, boundArguments, isBound);
		}

		private int FindParameterIndex(IReadOnlyList<CommandParameter> parameters, string name)
		{
			for (int index = 0; index < parameters.Count; index++)
			{
				if (string.Equals(parameters[index].Name, name, StringComparison.OrdinalIgnoreCase))
				{
					return index;
				}
			}
			return -1;
		}

		private ExecutionResult<object?[]> ApplyDefaultValues(IReadOnlyList<CommandParameter> parameters, object?[] boundArguments, bool[] isBound)
		{
			for (int index = 0; index < parameters.Count; index++)
			{
				if (!isBound[index])
				{
					if (parameters[index].IsOptional)
					{
						boundArguments[index] = parameters[index].DefaultValue;
					}
					else
					{
						return ExecutionResult<object?[]>.Failure($"Missing required parameter '{parameters[index].Name}'.", -1);
					}
				}
			}

			return ExecutionResult<object?[]>.Success(boundArguments);
		}

		private ExecutionResult<object?> ParseNode(SyntaxNode node, Type targetType)
		{
			if (targetType.IsArray)
			{
				return ParseArrayNode(node, targetType);
			}

			string textValue = ExtractScalarValue(node, out bool isScalar);
			if (!isScalar)
			{
				return ExecutionResult<object?>.Failure("Expected a single value, but got an array.", node.StartIndex);
			}

			if (!_parserRegistry.TryGetParser(targetType, out ITypeParser parser))
			{
				return ExecutionResult<object?>.Failure($"No type parser registered for type '{targetType.Name}'.", node.StartIndex);
			}

			var result = parser.Parse(textValue);
			if (!result.IsSuccess)
			{
				return ExecutionResult<object?>.Failure(result.ErrorMessage, node.StartIndex);
			}

			return result;
		}

		private ExecutionResult<object?> ParseArrayNode(SyntaxNode node, Type targetType)
		{
			if (node is not ArrayNode arrayNode)
			{
				return ExecutionResult<object?>.Failure("Expected an array value (e.g. [1, 2, 3]).", node.StartIndex);
			}

			Type elementType = targetType.GetElementType()!;
			Array arrayInstance = Array.CreateInstance(elementType, arrayNode.Elements.Count);

			for (int index = 0; index < arrayNode.Elements.Count; index++)
			{
				var elementResult = ParseNode(arrayNode.Elements[index], elementType);
				if (!elementResult.IsSuccess)
				{
					return ExecutionResult<object?>.Failure($"Array element at index {index}: {elementResult.ErrorMessage}", arrayNode.Elements[index].StartIndex);
				}

				arrayInstance.SetValue(elementResult.Value, index);
			}

			return ExecutionResult<object?>.Success(arrayInstance);
		}

		private string ExtractScalarValue(SyntaxNode node, out bool isScalar)
		{
			isScalar = true;

			if (node is LiteralNode literal)
			{
				return literal.Value;
			}

			if (node is StringNode stringNode)
			{
				return stringNode.Value;
			}

			isScalar = false;
			return string.Empty;
		}
	}
}