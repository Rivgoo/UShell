#nullable enable
using System;
using System.Buffers;
using System.Collections.Generic;
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Parsing.Lexing;
using UShell.Runtime.Core.Parsing.Syntax;

namespace UShell.Runtime.Core.Parsing
{
	public static class Parser
	{
		public static ExecutionResult<CommandNode> Parse(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return ExecutionResult<CommandNode>.Failure("Input is empty.", 0);

			int maxTokens = input.Length + 1;
			Token[] buffer = ArrayPool<Token>.Shared.Rent(maxTokens);

			try
			{
				int count = Tokenize(input, buffer);
				var state = new ParserState(input, buffer, count);
				return state.ParseCommand();
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
			do { t = lexer.GetNextToken(); buffer[count++] = t; }
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

			private Token Peek() => _tokens[_position < _tokenCount ? _position : _tokenCount - 1];

			private Token Consume()
			{
				Token t = Peek();
				if (t.Type != TokenType.EndOfFile) _position++;
				return t;
			}

			private string Slice(Token t) => _source.Substring(t.Position, t.Length);

			public ExecutionResult<CommandNode> ParseCommand()
			{
				Token cmd = Consume();
				if (cmd.Type != TokenType.Identifier)
					return ExecutionResult<CommandNode>.Failure($"Expected command name, got '{Slice(cmd)}'.", cmd.Position);

				var args = new List<SyntaxNode>();

				while (Peek().Type != TokenType.EndOfFile)
				{
					var arg = ParseArgument();
					if (!arg.IsSuccess)
						return ExecutionResult<CommandNode>.Failure(arg.ErrorMessage, arg.ErrorPosition);
					args.Add(arg.Value);
				}

				int end = args.Count > 0 ? args[^1].StartIndex + args[^1].Length : cmd.Position + cmd.Length;
				return ExecutionResult<CommandNode>.Success(
					new CommandNode(cmd.Position, end - cmd.Position, Slice(cmd), args.AsReadOnly()));
			}

			private ExecutionResult<SyntaxNode> ParseArgument() =>
				Peek().Type == TokenType.Minus ? ParseNamedArgument() : ParseValue();

			private ExecutionResult<SyntaxNode> ParseNamedArgument()
			{
				Token minus = Consume();
				Token name = Consume();

				if (name.Type != TokenType.Identifier)
					return ExecutionResult<SyntaxNode>.Failure("Expected argument name after '-'.", name.Position);

				var val = ParseValue();
				if (!val.IsSuccess) return val;

				int len = val.Value.StartIndex + val.Value.Length - minus.Position;
				return ExecutionResult<SyntaxNode>.Success(
					new NamedArgumentNode(minus.Position, len, Slice(name), val.Value));
			}

			private ExecutionResult<SyntaxNode> ParseValue()
			{
				Token t = Peek();
				switch (t.Type)
				{
					case TokenType.LBracket:
						return ParseArray();

					case TokenType.String:
						Consume();
						string raw = _source.Substring(t.Position + 1, t.Length - 2);
						string unescaped = raw.Replace("\\\"", "\"").Replace("\\\\", "\\");
						return ExecutionResult<SyntaxNode>.Success(new StringNode(t.Position, t.Length, unescaped));

					case TokenType.Identifier:
					case TokenType.Number:
						Consume();
						return ExecutionResult<SyntaxNode>.Success(new LiteralNode(t.Position, t.Length, Slice(t)));

					case TokenType.Unknown:
						return ExecutionResult<SyntaxNode>.Failure("Unrecognized or unclosed symbol.", t.Position);

					default:
						return ExecutionResult<SyntaxNode>.Failure($"Unexpected token '{t.Type}'.", t.Position);
				}
			}

			private ExecutionResult<SyntaxNode> ParseArray()
			{
				Token open = Consume();
				var elements = new List<SyntaxNode>();

				while (Peek().Type != TokenType.RBracket && Peek().Type != TokenType.EndOfFile)
				{
					var el = ParseValue();
					if (!el.IsSuccess) return el;
					elements.Add(el.Value);

					if (Peek().Type == TokenType.Comma)
						Consume();
					else if (Peek().Type != TokenType.RBracket)
						return ExecutionResult<SyntaxNode>.Failure("Expected ',' or ']'.", Peek().Position);
				}

				Token close = Consume();
				if (close.Type != TokenType.RBracket)
					return ExecutionResult<SyntaxNode>.Failure("Unclosed array: expected ']'.", close.Position);

				int len = close.Position + close.Length - open.Position;
				return ExecutionResult<SyntaxNode>.Success(new ArrayNode(open.Position, len, elements.AsReadOnly()));
			}
		}
	}
}