#nullable enable
using System;

namespace UShell.Runtime.Core.Parsing.Lexing
{
	public ref struct Lexer
	{
		private readonly ReadOnlySpan<char> _input;
		private int _position;

		public Lexer(ReadOnlySpan<char> input)
		{
			_input = input;
			_position = 0;
		}

		public Token GetNextToken()
		{
			SkipWhitespace();

			if (_position >= _input.Length)
			{
				return new Token(TokenType.EndOfFile, _position, 0);
			}

			char current = _input[_position];

			if (current == '-' && IsNamedArgumentMinus())
			{
				return ConsumeSingleCharToken(TokenType.Minus);
			}

			return current switch
			{
				'[' => ConsumeSingleCharToken(TokenType.LBracket),
				']' => ConsumeSingleCharToken(TokenType.RBracket),
				',' => ConsumeSingleCharToken(TokenType.Comma),
				'"' => LexString(),
				_ => LexWordOrNumber()
			};
		}

		private bool IsNamedArgumentMinus()
		{
			int next = _position + 1;
			return next < _input.Length && char.IsLetter(_input[next]);
		}

		private void SkipWhitespace()
		{
			while (_position < _input.Length && char.IsWhiteSpace(_input[_position]))
			{
				_position++;
			}
		}

		private Token ConsumeSingleCharToken(TokenType type)
		{
			int start = _position++;
			return new Token(type, start, 1);
		}

		private Token LexString()
		{
			int start = _position++;

			while (_position < _input.Length)
			{
				char c = _input[_position];

				if (c == '\\' && _position + 1 < _input.Length)
				{
					_position += 2;
					continue;
				}

				if (c == '"')
				{
					_position++;
					return new Token(TokenType.String, start, _position - start);
				}

				_position++;
			}

			return new Token(TokenType.Unknown, start, _position - start);
		}

		private Token LexWordOrNumber()
		{
			int start = _position;
			bool hasLetters = false;

			if (_position < _input.Length && _input[_position] == '-')
			{
				_position++;
			}

			while (_position < _input.Length
				   && !char.IsWhiteSpace(_input[_position])
				   && !IsDelimiter(_input[_position]))
			{
				char c = _input[_position];
				if (char.IsLetter(c) || c == '_' || c == '.')
				{
					hasLetters = true;
				}
				_position++;
			}

			if (_position == start)
			{
				_position++;
				return new Token(TokenType.Unknown, start, 1);
			}

			TokenType type = hasLetters ? TokenType.Identifier : TokenType.Number;
			return new Token(type, start, _position - start);
		}

		private static bool IsDelimiter(char c) => c == '[' || c == ']' || c == ',' || c == '"';
	}
}