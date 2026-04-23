using System;
using System.Globalization;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Core.BuiltIn
{
	/// <summary>
	/// Evaluates arithmetic expressions: +, -, *, /, parentheses.
	/// Recursive descent — zero reflection, zero allocations for typical inputs.
	/// </summary>
	internal static class SimpleExpressionEvaluator
	{
		public static ExecutionResult<double> Evaluate(string expression)
		{
			if (string.IsNullOrWhiteSpace(expression))
				return ExecutionResult<double>.Failure("Expression is empty.");
			try
			{
				var p = new ExprParser(expression.Trim().AsSpan());
				double result = p.ParseExpr();
				return ExecutionResult<double>.Success(result);
			}
			catch (Exception ex) { return ExecutionResult<double>.Failure(ex.Message); }
		}

		private ref struct ExprParser
		{
			private readonly ReadOnlySpan<char> _s;
			private int _i;

			public ExprParser(ReadOnlySpan<char> s) { _s = s; _i = 0; }

			public double ParseExpr() => ParseAddSub();

			private double ParseAddSub()
			{
				double v = ParseMulDiv();
				while (_i < _s.Length && (_s[_i] == '+' || _s[_i] == '-'))
				{
					char op = _s[_i++];
					v = op == '+' ? v + ParseMulDiv() : v - ParseMulDiv();
				}
				return v;
			}

			private double ParseMulDiv()
			{
				double v = ParseUnary();
				while (_i < _s.Length && (_s[_i] == '*' || _s[_i] == '/'))
				{
					char op = _s[_i++];
					double r = ParseUnary();
					if (op == '/' && r == 0) throw new DivideByZeroException("Division by zero.");
					v = op == '*' ? v * r : v / r;
				}
				return v;
			}

			private double ParseUnary()
			{
				Skip();
				if (_i < _s.Length && _s[_i] == '-') { _i++; return -ParsePrimary(); }
				return ParsePrimary();
			}

			private double ParsePrimary()
			{
				Skip();
				if (_i < _s.Length && _s[_i] == '(')
				{
					_i++;
					double v = ParseExpr();
					Skip();
					if (_i >= _s.Length || _s[_i] != ')') throw new FormatException("Missing ')'.");
					_i++;
					return v;
				}
				return ParseNumber();
			}

			private double ParseNumber()
			{
				Skip();
				int start = _i;
				while (_i < _s.Length && (char.IsDigit(_s[_i]) || _s[_i] == '.')) _i++;
				if (_i == start) throw new FormatException($"Expected number at pos {start}.");
				if (!double.TryParse(_s.Slice(start, _i - start),
					NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
					throw new FormatException($"Invalid number at pos {start}.");
				return v;
			}

			private void Skip()
			{
				while (_i < _s.Length && char.IsWhiteSpace(_s[_i])) _i++;
			}
		}
	}
}