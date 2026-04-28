#nullable enable
using System;
using System.Collections.Generic;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution.Context;
using UShell.Runtime.Core.Parsing.Syntax;
using UShell.Runtime.Core.Parsing.Types;

namespace UShell.Runtime.Core.Execution.Binding
{
	internal sealed class ArgumentBinder : IArgumentBinder
	{
		private readonly ITypeParserRegistry _parserRegistry;
		private readonly ISessionState _session;

		public ArgumentBinder(ITypeParserRegistry parserRegistry, ISessionState session)
		{
			_parserRegistry = parserRegistry ?? throw new ArgumentNullException(nameof(parserRegistry));
			_session = session ?? throw new ArgumentNullException(nameof(session));
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
			if (node is VariableNode varNode)
			{
				return ResolveVariableNode(varNode, targetType);
			}

			if (targetType.IsArray)
			{
				return ParseArrayNode(node, targetType, paramName);
			}

			if (targetType.IsGenericType && IsSupportedCollectionType(targetType.GetGenericTypeDefinition()))
			{
				return ParseCollectionNode(node, targetType, paramName);
			}

			if (targetType.IsEnum)
			{
				return ParseEnumNode(node, targetType, paramName);
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

		private ExecutionResult<object?> ResolveVariableNode(VariableNode varNode, Type targetType)
		{
			if (!_session.TryGetValue(varNode.Name, out object? val))
			{
				return ExecutionResult<object?>.Failure(
					ShellError.Create(ShellErrorCode.Execute_MacroOrSessionVariableNotFound, varNode.StartIndex, varNode.Name));
			}

			if (val != null && !targetType.IsAssignableFrom(val.GetType()))
			{
				try
				{
					val = Convert.ChangeType(val, targetType, System.Globalization.CultureInfo.InvariantCulture);
				}
				catch
				{
					return ExecutionResult<object?>.Failure(
						ShellError.Create(ShellErrorCode.Bind_TypeMismatch, varNode.StartIndex, $"${varNode.Name}", targetType.Name));
				}
			}
			return ExecutionResult<object?>.Success(val);
		}

		private ExecutionResult<object?> ParseArrayNode(SyntaxNode node, Type targetType, string paramName)
		{
			if (node is not ArrayNode arrayNode)
			{
				return ExecutionResult<object?>.Failure(
					ShellError.Create(ShellErrorCode.Bind_ExpectedArrayGotScalar, node.StartIndex, paramName));
			}

			Type elementType = targetType.GetElementType()!;

			if (elementType.IsArray || (elementType.IsGenericType && IsSupportedCollectionType(elementType.GetGenericTypeDefinition())))
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

		private ExecutionResult<object?> ParseCollectionNode(SyntaxNode node, Type targetCollectionType, string paramName)
		{
			Type elementType = targetCollectionType.GetGenericArguments()[0];
			Type arrayType = elementType.MakeArrayType();

			var arrayResult = ParseArrayNode(node, arrayType, paramName);
			if (!arrayResult.IsSuccess) return arrayResult;

			try
			{
				object collectionInstance = Activator.CreateInstance(targetCollectionType, arrayResult.Value)!;
				return ExecutionResult<object?>.Success(collectionInstance);
			}
			catch
			{
				return ExecutionResult<object?>.Failure(
					ShellError.Create(ShellErrorCode.Bind_TypeMismatch, node.StartIndex, "Array", targetCollectionType.Name));
			}
		}

		private ExecutionResult<object?> ParseEnumNode(SyntaxNode node, Type targetType, string paramName)
		{
			string textValue = ExtractScalarValue(node, out bool isScalar);

			if (!isScalar)
			{
				return ExecutionResult<object?>.Failure(
					ShellError.Create(ShellErrorCode.Bind_ExpectedScalarGotArray, node.StartIndex, paramName));
			}

			if (Enum.TryParse(targetType, textValue, ignoreCase: true, out object? enumValue) && enumValue != null)
			{
				return ExecutionResult<object?>.Success(enumValue);
			}

			return ExecutionResult<object?>.Failure(
				ShellError.Create(ShellErrorCode.Bind_TypeMismatch, node.StartIndex, textValue, targetType.Name));
		}

		private static bool IsSupportedCollectionType(Type genericTypeDef)
		{
			return genericTypeDef == typeof(List<>) ||
				   genericTypeDef == typeof(Stack<>) ||
				   genericTypeDef == typeof(Queue<>) ||
				   genericTypeDef == typeof(HashSet<>);
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