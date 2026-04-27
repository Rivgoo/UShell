#nullable enable
using System;
using System.Globalization;
using System.Text;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Execution;

namespace UShell.Runtime.Unity.BuiltIn
{
	/// <summary>
	/// An internal utility for evaluating string-based mathematical expressions.
	/// </summary>
	/// <remarks>
	/// Used primarily by the <see cref="MathUtilityProfile"/> to process the <c>eval</c> command.
	/// Not intended for external use.
	/// </remarks>
	internal static class SimpleExpressionEvaluator
	{
		public static ExecutionResult<double> Evaluate(string expression)
		{
			if (string.IsNullOrWhiteSpace(expression))
			{
				return Fail("Expression is empty.");
			}

			try
			{
				var parser = new ExprParser(expression.Trim().AsSpan());
				double result = parser.ParseExpr();
				return ExecutionResult<double>.Success(result);
			}
			catch (EvalException ex)
			{
				return Fail(ex.Message);
			}
			catch (DivideByZeroException)
			{
				return Fail("Division by zero.");
			}
			catch (Exception ex)
			{
				return Fail(ex.Message);
			}
		}

		public static string FormatResult(double value)
		{
			if (double.IsNaN(value)) return "NaN";
			if (double.IsPositiveInfinity(value)) return "∞";
			if (double.IsNegativeInfinity(value)) return "-∞";

			if (value == Math.Floor(value) && Math.Abs(value) < 1e15)
			{
				return ((long)value).ToString(CultureInfo.InvariantCulture);
			}

			return value.ToString("G10", CultureInfo.InvariantCulture);
		}

		private static ExecutionResult<double> Fail(string message) =>
			ExecutionResult<double>.Failure(
				ShellError.Create(ShellErrorCode.Execute_ExpressionError, -1, message));

		private sealed class EvalException : Exception
		{
			public EvalException(string message) : base(message) { }
		}

		private ref struct ExprParser
		{
			private readonly ReadOnlySpan<char> _s;
			private int _i;

			public ExprParser(ReadOnlySpan<char> s) { _s = s; _i = 0; }

			public double ParseExpr()
			{
				double v = ParseAddSub();
				Skip();
				if (_i < _s.Length)
				{
					throw new EvalException($"Unexpected character '{_s[_i]}' at position {_i}.");
				}
				return v;
			}

			private double ParseAddSub()
			{
				double v = ParseMulDiv();
				while (_i < _s.Length)
				{
					Skip();
					if (_i >= _s.Length) break;
					char op = _s[_i];
					if (op != '+' && op != '-') break;
					_i++;
					double r = ParseMulDiv();
					v = op == '+' ? v + r : v - r;
				}
				return v;
			}

			private double ParseMulDiv()
			{
				double v = ParsePower();
				while (_i < _s.Length)
				{
					Skip();
					if (_i >= _s.Length) break;
					char op = _s[_i];
					if (op != '*' && op != '/' && op != '%') break;
					_i++;
					double r = ParsePower();

					if ((op == '/' || op == '%') && r == 0.0)
						throw new EvalException("Division / modulo by zero.");

					v = op == '*' ? v * r : op == '/' ? v / r : v % r;
				}
				return v;
			}

			private double ParsePower()
			{
				double baseVal = ParseUnary();
				Skip();
				if (_i < _s.Length && _s[_i] == '^')
				{
					_i++;
					double exp = ParsePower();
					return Math.Pow(baseVal, exp);
				}
				return baseVal;
			}

			private double ParseUnary()
			{
				Skip();
				if (_i < _s.Length)
				{
					if (_s[_i] == '-') { _i++; return -ParseUnary(); }
					if (_s[_i] == '+') { _i++; return +ParseUnary(); }
				}
				return ParsePrimary();
			}

			private double ParsePrimary()
			{
				Skip();
				if (_i >= _s.Length) throw new EvalException("Unexpected end of expression.");

				char c = _s[_i];

				if (c == '(')
				{
					_i++;
					double v = ParseAddSub();
					Skip();
					if (_i >= _s.Length || _s[_i] != ')') throw new EvalException("Missing closing ')'.");
					_i++;
					return v;
				}

				if (char.IsLetter(c) || c == '_') return ParseIdentifier();

				return ParseNumber();
			}

			private double ParseIdentifier()
			{
				int start = _i;
				while (_i < _s.Length && (char.IsLetterOrDigit(_s[_i]) || _s[_i] == '_')) _i++;

				ReadOnlySpan<char> nameSpan = _s.Slice(start, _i - start);
				string name = nameSpan.ToString().ToLowerInvariant();

				Skip();

				if (_i < _s.Length && _s[_i] == '(')
				{
					_i++;
					return DispatchFunction(name);
				}

				return ResolveConstant(name);
			}

			private static double ResolveConstant(string name) => name switch
			{
				"pi" => Math.PI,
				"e" => Math.E,
				"tau" => Math.PI * 2.0,
				"inf" => double.PositiveInfinity,
				"nan" => double.NaN,
				"sqrt2" => Math.Sqrt(2.0),
				"ln2" => Math.Log(2.0),
				"log2e" => 1.0 / Math.Log(2.0),
				"log10e" => Math.Log10(Math.E),
				"phi" => 1.6180339887498948482,
				_ => throw new EvalException($"Unknown identifier '{name}'.")
			};

			private double DispatchFunction(string name)
			{
				double result = name switch
				{
					"abs" => Math.Abs(ParseSingleArg(name)),
					"sqrt" => Math.Sqrt(ParseSingleArg(name)),
					"cbrt" => CubeRoot(ParseSingleArg(name)),
					"ceil" => Math.Ceiling(ParseSingleArg(name)),
					"floor" => Math.Floor(ParseSingleArg(name)),
					"round" => Math.Round(ParseSingleArg(name), MidpointRounding.AwayFromZero),
					"trunc" => Math.Truncate(ParseSingleArg(name)),
					"sign" => Math.Sign(ParseSingleArg(name)),
					"sin" => Math.Sin(ParseSingleArg(name)),
					"cos" => Math.Cos(ParseSingleArg(name)),
					"tan" => Math.Tan(ParseSingleArg(name)),
					"asin" => Math.Asin(ParseSingleArg(name)),
					"acos" => Math.Acos(ParseSingleArg(name)),
					"atan" => Math.Atan(ParseSingleArg(name)),
					"sinh" => Math.Sinh(ParseSingleArg(name)),
					"cosh" => Math.Cosh(ParseSingleArg(name)),
					"tanh" => Math.Tanh(ParseSingleArg(name)),
					"log" => ParseLog(),
					"log2" => Math.Log(ParseSingleArg(name)) / Math.Log(2.0),
					"log10" => Math.Log10(ParseSingleArg(name)),
					"exp" => Math.Exp(ParseSingleArg(name)),
					"deg2rad" => ParseSingleArg(name) * (Math.PI / 180.0),
					"rad2deg" => ParseSingleArg(name) * (180.0 / Math.PI),
					"fact" => Factorial(ParseSingleArg(name)),
					"atan2" => ParseAtan2(),
					"pow" => ParsePowFunc(),
					"min" => ParseMin(),
					"max" => ParseMaxFunc(),
					"clamp" => ParseClamp(),
					"lerp" => ParseLerp(),
					"hex" => ParseHex(),
					"bin" => ParseBin(),
					_ => throw new EvalException($"Unknown function '{name}'.")
				};

				return result;
			}

			private double ParseSingleArg(string funcName)
			{
				double v = ParseAddSub();
				Skip();
				if (_i >= _s.Length || _s[_i] != ')') throw new EvalException($"Missing ')' after {funcName}(...).");
				_i++;
				return v;
			}

			private double ParseLog()
			{
				double x = ParseAddSub();
				Skip();
				if (_i < _s.Length && _s[_i] == ',')
				{
					_i++;
					double b = ParseAddSub();
					Skip();
					ConsumeCloseParen("log");
					if (b <= 0 || b == 1) throw new EvalException("log base must be > 0 and ≠ 1.");
					return Math.Log(x) / Math.Log(b);
				}
				ConsumeCloseParen("log");
				return Math.Log(x);
			}

			private double ParseAtan2()
			{
				double y = ParseAddSub(); Skip(); ConsumeComma("atan2");
				double x = ParseAddSub(); Skip(); ConsumeCloseParen("atan2");
				return Math.Atan2(y, x);
			}

			private double ParsePowFunc()
			{
				double b = ParseAddSub(); Skip(); ConsumeComma("pow");
				double exp = ParseAddSub(); Skip(); ConsumeCloseParen("pow");
				return Math.Pow(b, exp);
			}

			private double ParseMin()
			{
				double a = ParseAddSub(); Skip(); ConsumeComma("min");
				double b = ParseAddSub(); Skip(); ConsumeCloseParen("min");
				return Math.Min(a, b);
			}

			private double ParseMaxFunc()
			{
				double a = ParseAddSub(); Skip(); ConsumeComma("max");
				double b = ParseAddSub(); Skip(); ConsumeCloseParen("max");
				return Math.Max(a, b);
			}

			private double ParseClamp()
			{
				double x = ParseAddSub(); Skip(); ConsumeComma("clamp");
				double min = ParseAddSub(); Skip(); ConsumeComma("clamp");
				double max = ParseAddSub(); Skip(); ConsumeCloseParen("clamp");
				if (min > max) throw new EvalException("clamp: min must be ≤ max.");
				return x < min ? min : x > max ? max : x;
			}

			private double ParseLerp()
			{
				double a = ParseAddSub(); Skip(); ConsumeComma("lerp");
				double b = ParseAddSub(); Skip(); ConsumeComma("lerp");
				double t = ParseAddSub(); Skip(); ConsumeCloseParen("lerp");
				return a + (b - a) * t;
			}

			private double ParseHex()
			{
				double v = ParseSingleArg("hex");
				throw new FormatResultException(v, FormatResultKind.Hex);
			}

			private double ParseBin()
			{
				double v = ParseSingleArg("bin");
				throw new FormatResultException(v, FormatResultKind.Bin);
			}

			private void ConsumeComma(string ctx)
			{
				if (_i >= _s.Length || _s[_i] != ',') throw new EvalException($"Expected ',' in {ctx}(...).");
				_i++;
			}

			private void ConsumeCloseParen(string ctx)
			{
				if (_i >= _s.Length || _s[_i] != ')') throw new EvalException($"Missing ')' after {ctx}(...).");
				_i++;
			}

			private double ParseNumber()
			{
				Skip();
				int start = _i;

				if (_i < _s.Length && _s[_i] == '.') _i++;

				while (_i < _s.Length && (char.IsDigit(_s[_i]) || _s[_i] == '.')) _i++;

				if (_i < _s.Length && (_s[_i] == 'e' || _s[_i] == 'E'))
				{
					_i++;
					if (_i < _s.Length && (_s[_i] == '+' || _s[_i] == '-')) _i++;
					while (_i < _s.Length && char.IsDigit(_s[_i])) _i++;
				}

				if (_i == start) throw new EvalException($"Expected number or identifier at position {start}.");

				ReadOnlySpan<char> slice = _s.Slice(start, _i - start);

				if (!double.TryParse(slice, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
					throw new EvalException($"Invalid number literal '{slice.ToString()}'.");

				return v;
			}

			private void Skip()
			{
				while (_i < _s.Length && char.IsWhiteSpace(_s[_i])) _i++;
			}
		}

		private static double CubeRoot(double x) => x >= 0 ? Math.Pow(x, 1.0 / 3.0) : -Math.Pow(-x, 1.0 / 3.0);

		private static double Factorial(double x)
		{
			if (x < 0 || x != Math.Floor(x)) throw new EvalException("fact() requires a non-negative integer argument.");
			if (x > 20) throw new EvalException("fact() maximum argument is 20 (overflow guard).");

			long n = (long)x;
			long acc = 1;
			for (long k = 2; k <= n; k++) acc *= k;
			return acc;
		}

		internal enum FormatResultKind { Normal, Hex, Bin }

		internal sealed class FormatResultException : Exception
		{
			public double Value { get; }
			public FormatResultKind Kind { get; }
			public FormatResultException(double value, FormatResultKind kind) : base(string.Empty)
			{
				Value = value;
				Kind = kind;
			}
		}

		public static ExecutionResult<double> EvaluateWithFormat(string expression, out FormatResultKind format)
		{
			format = FormatResultKind.Normal;

			if (string.IsNullOrWhiteSpace(expression)) return Fail("Expression is empty.");

			try
			{
				var parser = new ExprParser(expression.Trim().AsSpan());
				double result = parser.ParseExpr();
				return ExecutionResult<double>.Success(result);
			}
			catch (FormatResultException fre)
			{
				format = fre.Kind;
				return ExecutionResult<double>.Success(fre.Value);
			}
			catch (EvalException ex)
			{
				return Fail(ex.Message);
			}
			catch (DivideByZeroException)
			{
				return Fail("Division by zero.");
			}
			catch (Exception ex)
			{
				return Fail(ex.Message);
			}
		}

		public static string FormatResult(double value, FormatResultKind kind)
		{
			if (kind == FormatResultKind.Hex)
			{
				long lv = (long)Math.Round(value);
				return $"0x{lv:X}";
			}
			if (kind == FormatResultKind.Bin)
			{
				long lv = (long)Math.Round(value);
				if (lv == 0) return "0b0";
				var sb = new StringBuilder("0b");
				bool leading = true;
				for (int bit = 63; bit >= 0; bit--)
				{
					bool set = ((lv >> bit) & 1L) == 1L;
					if (set) leading = false;
					if (!leading) sb.Append(set ? '1' : '0');
				}
				return sb.ToString();
			}
			return FormatResult(value);
		}
	}
}