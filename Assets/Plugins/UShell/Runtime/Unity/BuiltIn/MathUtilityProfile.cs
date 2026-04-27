#nullable enable
using System;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands.Fluent;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Core.Output.Formatting;

namespace UShell.Runtime.Unity.BuiltIn
{
	/// <summary>
	/// A built-in profile that provides commands for mathematical evaluation and unit conversion.
	/// </summary>
	/// <remarks>
	/// Registers commands such as <c>eval</c>, <c>random</c>, and <c>convert</c>.
	/// </remarks>
	public sealed class MathUtilityProfile : ShellProfile
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MathUtilityProfile"/> class.
		/// </summary>
		public MathUtilityProfile(IConsolePrinter printer) : base(printer)
		{
		}

		/// <inheritdoc/>
		protected override void Configure(ICommandBuilder builder)
		{
			builder.WithName("eval")
				.WithDescription("Evaluates a mathematical expression and prints the result.")
				.WithAlias("calc")
				.AddParameter<string>("expression")
				.ExecutesReturning<string, double>(Eval);

			builder.WithName("random")
				.WithDescription("Generates a random integer in the inclusive range [min, max].")
				.WithAlias("rand")
				.AddOptionalParameter<int>("min", 0)
				.AddOptionalParameter<int>("max", 100)
				.ExecutesReturning<int, int, int>(Random);

			builder.WithName("convert")
				.WithDescription("Converts a value between common units or numeric bases.")
				.AddParameter<float>("value")
				.AddParameter<string>("from")
				.AddParameter<string>("to")
				.ExecutesReturning<float, string, string, float>(Convert);
		}

		private double Eval(string expression)
		{
			var result = SimpleExpressionEvaluator.EvaluateWithFormat(expression, out SimpleExpressionEvaluator.FormatResultKind format);

			if (!result.IsSuccess)
			{
				throw new Exception(result.Error!.Value.Message);
			}

			string formatted = SimpleExpressionEvaluator.FormatResult(result.Value, format);
			string display = RichText.Bold(RichText.Color(formatted, ShellPalette.SyntaxNumber));
			PrintSuccess($"= {display}");

			return result.Value;
		}

		private int Random(int min, int max)
		{
			if (min > max) (min, max) = (max, min);

			int val = new System.Random().Next(min, max + 1);
			string r = RichText.Bold(RichText.Color(val.ToString(), ShellPalette.SyntaxNumber));
			PrintSuccess($"Random [{min}, {max}]  →  {r}");

			return val;
		}

		private float Convert(float value, string from, string to)
		{
			string fromL = from.ToLowerInvariant();
			string toL = to.ToLowerInvariant();

			if (TryConvertUnit(value, fromL, toL, out double result, out string unit))
			{
				string rv = RichText.Bold(RichText.Color(SimpleExpressionEvaluator.FormatResult(result), ShellPalette.SyntaxNumber));
				string uv = RichText.Color(unit, ShellPalette.StatUnit);
				PrintSuccess($"{value} {from}  →  {rv} {uv}");

				return (float)result;
			}

			throw new Exception($"Cannot convert from '{from}' to '{to}'. Supported conversions: deg↔rad, km↔m↔cm↔mm↔mi↔ft↔in, kg↔g↔lb, bytes↔kb↔mb↔gb, dec↔hex↔bin.");
		}

		private static bool TryConvertUnit(float value, string from, string to, out double result, out string unit)
		{
			result = 0;
			unit = to;

			if (from == "deg" && to == "rad") { result = value * (Math.PI / 180.0); return true; }
			if (from == "rad" && to == "deg") { result = value * (180.0 / Math.PI); return true; }

			if (TryToMetres(from, value, out double metres) && TryFromMetres(to, metres, out double converted)) { result = converted; return true; }
			if (TryToKg(from, value, out double kg) && TryFromKg(to, kg, out double convertedKg)) { result = convertedKg; return true; }
			if (TryToBytes(from, value, out double bytes) && TryFromBytes(to, bytes, out double convertedBytes)) { result = convertedBytes; return true; }

			long intVal = (long)Math.Round(value);
			if (from == "dec" && to == "hex") { result = intVal; unit = $"0x{intVal:X}"; return true; }
			if (from == "dec" && to == "bin") { result = intVal; unit = $"0b{System.Convert.ToString(intVal, 2)}"; return true; }
			if (from == "hex" && to == "dec") { result = intVal; return true; }
			if (from == "bin" && to == "dec") { result = intVal; return true; }

			return false;
		}

		private static bool TryToMetres(string unit, double v, out double m)
		{
			m = unit switch
			{
				"km" => v * 1000.0,
				"m" => v,
				"cm" => v / 100.0,
				"mm" => v / 1000.0,
				"mi" => v * 1609.344,
				"ft" => v * 0.3048,
				"in" => v * 0.0254,
				_ => double.NaN
			};
			return !double.IsNaN(m);
		}

		private static bool TryFromMetres(string unit, double m, out double v)
		{
			v = unit switch
			{
				"km" => m / 1000.0,
				"m" => m,
				"cm" => m * 100.0,
				"mm" => m * 1000.0,
				"mi" => m / 1609.344,
				"ft" => m / 0.3048,
				"in" => m / 0.0254,
				_ => double.NaN
			};
			return !double.IsNaN(v);
		}

		private static bool TryToKg(string unit, double v, out double kg)
		{
			kg = unit switch { "kg" => v, "g" => v / 1000.0, "lb" => v * 0.45359237, _ => double.NaN };
			return !double.IsNaN(kg);
		}

		private static bool TryFromKg(string unit, double kg, out double v)
		{
			v = unit switch { "kg" => kg, "g" => kg * 1000.0, "lb" => kg / 0.45359237, _ => double.NaN };
			return !double.IsNaN(v);
		}

		private static bool TryToBytes(string unit, double v, out double b)
		{
			b = unit switch { "bytes" or "b" => v, "kb" => v * 1024.0, "mb" => v * 1024.0 * 1024.0, "gb" => v * 1024.0 * 1024.0 * 1024.0, _ => double.NaN };
			return !double.IsNaN(b);
		}

		private static bool TryFromBytes(string unit, double b, out double v)
		{
			v = unit switch { "bytes" or "b" => b, "kb" => b / 1024.0, "mb" => b / 1024.0 / 1024.0, "gb" => b / 1024.0 / 1024.0 / 1024.0, _ => double.NaN };
			return !double.IsNaN(v);
		}
	}
}