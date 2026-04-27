#nullable enable
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Parsing.Lexing;
using UShell.Runtime.Core.Parsing.Syntax;

namespace UShell.Runtime.Core.Parsing
{
	/// <summary>
	/// The front-end compiler unit responsible for transforming raw shell text into an Abstract Syntax Tree (AST).
	/// </summary>
	public static class Parser
	{
		private const int MaxRecursionDepth = 20;

		/// <summary>
		/// Tokenizes and parses the provided string into an executable syntax tree.
		/// </summary>
		/// <param name="input">The raw text entered by the user.</param>
		/// <returns>A successful result holding the root <see cref="SyntaxNode"/>, or a structured syntax error.</returns>
		public static ExecutionResult<SyntaxNode> Parse(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				return ExecutionResult<SyntaxNode>.Failure(
					ShellError.Create(ShellErrorCode.Syntax_EmptyInput, 0));
			}

			int maxTokens = input.Length + 1;
			Token[] buffer = ArrayPool<Token>.Shared.Rent(maxTokens);

			try
			{
				int count = Tokenize(input, buffer);
				var state = new ParserState(input, buffer, count);
				return state.ParseStatement();
			}
			finally
			{
				ArrayPool<Token>.Shared.Return(buffer);
			}
		}

		private static int Tokenize(string input, Token[] buffer)
		{
			int count = 0;
			var lexer = new Lexer(input.AsSpan());
			Token t;
			do
			{
				t = lexer.GetNextToken();
				buffer[count++] = t;
			}
			while (t.Type != TokenType.EndOfFile);
			return count;
		}

		private ref struct ParserState
		{
			private readonly string _source;
			private readonly Token[] _tokens;
			private readonly int _tokenCount;
			private int _position;

			public ParserState(string source, Token[] tokens, int count)
			{
				_source = source;
				_tokens = tokens;
				_tokenCount = count;
				_position = 0;
			}

			private Token Peek(int offset = 0)
			{
				int index = _position + offset;
				return _tokens[index < _tokenCount ? index : _tokenCount - 1];
			}

			private Token Consume()
			{
				Token t = Peek();
				if (t.Type != TokenType.EndOfFile) _position++;
				return t;
			}

			private string Slice(Token t) => _source.Substring(t.Position, t.Length);

			public ExecutionResult<SyntaxNode> ParseStatement()
			{
				if (Peek().Type == TokenType.Variable && Peek(1).Type == TokenType.Equals)
				{
					Token varToken = Consume();
					Consume();

					ExecutionResult<SyntaxNode> rightSideResult;

					if (Peek().Type == TokenType.Identifier)
					{
						var cmdResult = ParseCommand();
						if (!cmdResult.IsSuccess) return ExecutionResult<SyntaxNode>.Failure(cmdResult.Error!.Value);
						rightSideResult = ExecutionResult<SyntaxNode>.Success(cmdResult.Value);
					}
					else
					{
						rightSideResult = ParseValue(1);
					}

					if (!rightSideResult.IsSuccess) return rightSideResult;

					string varName = Slice(varToken).Substring(1);
					int len = rightSideResult.Value.StartIndex + rightSideResult.Value.Length - varToken.Position;

					return ExecutionResult<SyntaxNode>.Success(
						new AssignmentNode(varToken.Position, len, varName, rightSideResult.Value));
				}

				if (Peek().Type == TokenType.Identifier)
				{
					var cmdResult = ParseCommand();
					if (!cmdResult.IsSuccess) return ExecutionResult<SyntaxNode>.Failure(cmdResult.Error!.Value);
					return ExecutionResult<SyntaxNode>.Success(cmdResult.Value);
				}

				var valResult = ParseValue(1);
				if (!valResult.IsSuccess) return valResult;

				if (Peek().Type != TokenType.EndOfFile)
				{
					return ExecutionResult<SyntaxNode>.Failure(
						ShellError.Create(ShellErrorCode.Syntax_UnexpectedToken, Peek().Position, Peek().Type));
				}

				return valResult;
			}

			private ExecutionResult<CommandNode> ParseCommand()
			{
				Token cmd = Consume();

				if (cmd.Type != TokenType.Identifier)
				{
					return ExecutionResult<CommandNode>.Failure(
						ShellError.Create(ShellErrorCode.Syntax_ExpectedCommandOrVariable, cmd.Position, Slice(cmd)));
				}

				var args = new List<SyntaxNode>();

				while (Peek().Type != TokenType.EndOfFile)
				{
					var arg = ParseArgument(1);
					if (!arg.IsSuccess) return ExecutionResult<CommandNode>.Failure(arg.Error!.Value);

					args.Add(arg.Value);
				}

				int end = args.Count > 0 ? args[^1].StartIndex + args[^1].Length : cmd.Position + cmd.Length;

				return ExecutionResult<CommandNode>.Success(
					new CommandNode(cmd.Position, end - cmd.Position, Slice(cmd), args.AsReadOnly()));
			}

			private ExecutionResult<SyntaxNode> ParseArgument(int depth)
			{
				if (depth > MaxRecursionDepth)
				{
					return ExecutionResult<SyntaxNode>.Failure(
						ShellError.Create(ShellErrorCode.Syntax_MaxDepthExceeded, Peek().Position));
				}

				return Peek().Type == TokenType.Minus ? ParseNamedArgument(depth) : ParseValue(depth);
			}

			private ExecutionResult<SyntaxNode> ParseNamedArgument(int depth)
			{
				Token minus = Consume();
				Token name = Consume();

				if (name.Type != TokenType.Identifier)
				{
					return ExecutionResult<SyntaxNode>.Failure(
						ShellError.Create(ShellErrorCode.Syntax_ExpectedArgumentName, name.Position));
				}

				var valResult = ParseValue(depth + 1);
				if (!valResult.IsSuccess) return valResult;

				int len = valResult.Value.StartIndex + valResult.Value.Length - minus.Position;

				return ExecutionResult<SyntaxNode>.Success(
					new NamedArgumentNode(minus.Position, len, Slice(name), valResult.Value));
			}

			private ExecutionResult<SyntaxNode> ParseValue(int depth)
			{
				if (depth > MaxRecursionDepth)
				{
					return ExecutionResult<SyntaxNode>.Failure(
						ShellError.Create(ShellErrorCode.Syntax_MaxDepthExceeded, Peek().Position));
				}

				Token t = Peek();
				switch (t.Type)
				{
					case TokenType.LBracket:
						return ParseArray(depth + 1);

					case TokenType.String:
						Consume();
						return ParseEscapedString(t);

					case TokenType.Variable:
						Consume();
						return ExecutionResult<SyntaxNode>.Success(
							new VariableNode(t.Position, t.Length, Slice(t).Substring(1)));

					case TokenType.Identifier:
					case TokenType.Number:
						Consume();
						return ExecutionResult<SyntaxNode>.Success(new LiteralNode(t.Position, t.Length, Slice(t)));

					case TokenType.Unknown:
						return ExecutionResult<SyntaxNode>.Failure(
							ShellError.Create(ShellErrorCode.Syntax_UnrecognizedSymbol, t.Position));

					default:
						return ExecutionResult<SyntaxNode>.Failure(
							ShellError.Create(ShellErrorCode.Syntax_UnexpectedToken, t.Position, t.Type));
				}
			}

			private ExecutionResult<SyntaxNode> ParseArray(int depth)
			{
				Token open = Consume();
				var elements = new List<SyntaxNode>();

				while (Peek().Type != TokenType.RBracket && Peek().Type != TokenType.EndOfFile)
				{
					var el = ParseValue(depth);
					if (!el.IsSuccess) return el;

					elements.Add(el.Value);

					if (Peek().Type == TokenType.Comma)
					{
						Consume();
					}
					else if (Peek().Type != TokenType.RBracket)
					{
						return ExecutionResult<SyntaxNode>.Failure(
							ShellError.Create(ShellErrorCode.Syntax_ExpectedCommaOrBracket, Peek().Position));
					}
				}

				Token close = Consume();
				if (close.Type != TokenType.RBracket)
				{
					return ExecutionResult<SyntaxNode>.Failure(
						ShellError.Create(ShellErrorCode.Syntax_UnclosedArray, close.Position));
				}

				int len = close.Position + close.Length - open.Position;

				return ExecutionResult<SyntaxNode>.Success(
					new ArrayNode(open.Position, len, elements.AsReadOnly()));
			}

			private ExecutionResult<SyntaxNode> ParseEscapedString(Token token)
			{
				if (token.Length < 2 || _source[token.Position + token.Length - 1] != '"')
				{
					return ExecutionResult<SyntaxNode>.Failure(
						ShellError.Create(ShellErrorCode.Syntax_UnclosedString, token.Position));
				}

				string raw = _source.Substring(token.Position + 1, token.Length - 2);
				var unescaped = new StringBuilder(raw.Length);

				for (int i = 0; i < raw.Length; i++)
				{
					if (raw[i] == '\\' && i + 1 < raw.Length)
					{
						char next = raw[i + 1];
						char resolved = next switch
						{
							'n' => '\n',
							't' => '\t',
							'r' => '\r',
							'\\' => '\\',
							'"' => '"',
							_ => next
						};
						unescaped.Append(resolved);
						i++;
					}
					else
					{
						unescaped.Append(raw[i]);
					}
				}

				return ExecutionResult<SyntaxNode>.Success(
					new StringNode(token.Position, token.Length, unescaped.ToString()));
			}
		}
	}
}