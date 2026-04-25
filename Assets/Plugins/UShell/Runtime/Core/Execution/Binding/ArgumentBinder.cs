#nullable enable
using System;
using System.Collections.Generic;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Diagnostics;
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
						return ExecutionResult<object?[]>.Failure(
							ShellError.Create(ShellErrorCode.Bind_UnknownParameter, namedArgument.StartIndex, namedArgument.Name));
					}

					if (isBound[parameterIndex])
					{
						return ExecutionResult<object?[]>.Failure(
							ShellError.Create(ShellErrorCode.Bind_ParameterAlreadyBound, namedArgument.StartIndex, namedArgument.Name));
					}

					var parseResult = ParseNode(namedArgument.Value, parameters[parameterIndex].ParameterType, parameters[parameterIndex].Name);

					if (!parseResult.IsSuccess) return ExecutionResult<object?[]>.Failure(parseResult.Error!.Value);

					boundArguments[parameterIndex] = parseResult.Value;
					isBound[parameterIndex] = true;
				}
				else
				{
					if (isNamedSectionStarted)
					{
						return ExecutionResult<object?[]>.Failure(
							ShellError.Create(ShellErrorCode.Bind_PositionalAfterNamed, argumentNode.StartIndex));
					}

					if (positionalIndex >= parameters.Count)
					{
						return ExecutionResult<object?[]>.Failure(
							ShellError.Create(ShellErrorCode.Bind_TooManyArguments, argumentNode.StartIndex));
					}

					var parseResult = ParseNode(argumentNode, parameters[positionalIndex].ParameterType, parameters[positionalIndex].Name);

					if (!parseResult.IsSuccess) return ExecutionResult<object?[]>.Failure(parseResult.Error!.Value);

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
						return ExecutionResult<object?[]>.Failure(
							ShellError.Create(ShellErrorCode.Bind_MissingRequiredParameter, -1, parameters[index].Name));
					}
				}
			}

			return ExecutionResult<object?[]>.Success(boundArguments);
		}

		private ExecutionResult<object?> ParseNode(SyntaxNode node, Type targetType, string paramName)
		{
			if (targetType.IsArray)
			{
				return ParseArrayNode(node, targetType, paramName);
			}

			string textValue = ExtractScalarValue(node, out bool isScalar);
			if (!isScalar)
			{
				return ExecutionResult<object?>.Failure(
					ShellError.Create(ShellErrorCode.Bind_ExpectedScalarGotArray, node.StartIndex, paramName));
			}

			if (!_parserRegistry.TryGetParser(targetType, out ITypeParser parser))
			{
				return ExecutionResult<object?>.Failure(
					ShellError.Create(ShellErrorCode.Bind_NoParserRegistered, node.StartIndex, targetType.Name));
			}

			var result = parser.Parse(textValue);

			if (!result.IsSuccess)
			{
				return ExecutionResult<object?>.Failure(result.Error!.Value.WithPosition(node.StartIndex));
			}

			return result;
		}

		private ExecutionResult<object?> ParseArrayNode(SyntaxNode node, Type targetType, string paramName)
		{
			if (node is not ArrayNode arrayNode)
			{
				return ExecutionResult<object?>.Failure(
					ShellError.Create(ShellErrorCode.Bind_ExpectedArrayGotScalar, node.StartIndex, paramName));
			}

			Type elementType = targetType.GetElementType()!;

			if (elementType.IsArray)
			{
				return ExecutionResult<object?>.Failure(
					ShellError.Create(ShellErrorCode.Bind_MultidimensionalArrayNotSupported, node.StartIndex));
			}

			Array arrayInstance = Array.CreateInstance(elementType, arrayNode.Elements.Count);

			for (int index = 0; index < arrayNode.Elements.Count; index++)
			{
				SyntaxNode elementNode = arrayNode.Elements[index];

				if (elementNode is ArrayNode)
				{
					return ExecutionResult<object?>.Failure(
						ShellError.Create(ShellErrorCode.Bind_MultidimensionalArrayNotSupported, elementNode.StartIndex));
				}

				var elementResult = ParseNode(elementNode, elementType, paramName);

				if (!elementResult.IsSuccess) return elementResult;

				arrayInstance.SetValue(elementResult.Value, index);
			}

			return ExecutionResult<object?>.Success(arrayInstance);
		}

		private string ExtractScalarValue(SyntaxNode node, out bool isScalar)
		{
			isScalar = true;

			if (node is LiteralNode literal) return literal.Value;
			if (node is StringNode stringNode) return stringNode.Value;

			isScalar = false;
			return string.Empty;
		}
	}
}