#nullable enable
using System;
using System.Collections.Generic;
using System.Text;

namespace UShell.Runtime.Core.Output.Formatting
{
	/// <summary>
	/// Provides utility methods for formatting 2D string arrays into aligned ASCII grid tables.
	/// </summary>
	public static class TableBuilder
	{
		/// <summary>
		/// Calculates the necessary padding for each column based on visual text length, 
		/// and generates a fully formatted ASCII table string.
		/// </summary>
		/// <param name="headers">A collection of column titles.</param>
		/// <param name="rows">A collection of rows, where each row is a collection of column cell strings.</param>
		/// <param name="style">Determines the visual density of the table's divider lines.</param>
		/// <returns>A multiline string representing the generated table.</returns>
		public static string BuildAsciiTable(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows, TableStyle style = TableStyle.Standard)
		{
			if (headers == null || headers.Count == 0)
			{
				return string.Empty;
			}

			int[] columnWidths = CalculateColumnWidths(headers, rows);
			var stringBuilder = new StringBuilder();

			AppendRow(stringBuilder, headers, columnWidths);
			AppendSeparator(stringBuilder, columnWidths);

			if (rows != null)
			{
				for (int i = 0; i < rows.Count; i++)
				{
					AppendRow(stringBuilder, rows[i], columnWidths, headers.Count);

					if (style == TableStyle.Grid && i < rows.Count - 1)
					{
						AppendSeparator(stringBuilder, columnWidths);
					}
				}
			}

			return stringBuilder.ToString();
		}

		private static int[] CalculateColumnWidths(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>>? rows)
		{
			int[] widths = new int[headers.Count];

			for (int index = 0; index < headers.Count; index++)
			{
				widths[index] = GetMaxVisualLength(headers[index]);
			}

			if (rows == null) return widths;

			foreach (var row in rows)
			{
				for (int index = 0; index < row.Count && index < headers.Count; index++)
				{
					int cellLength = GetMaxVisualLength(row[index]);
					if (cellLength > widths[index])
					{
						widths[index] = cellLength;
					}
				}
			}

			return widths;
		}

		private static int GetMaxVisualLength(string? cell)
		{
			if (string.IsNullOrEmpty(cell)) return 0;

			int maxLength = 0;
			string[] lines = cell.Split('\n');

			foreach (string line in lines)
			{
				int visualLength = RichTextStripper.Strip(line).Length;
				if (visualLength > maxLength)
				{
					maxLength = visualLength;
				}
			}

			return maxLength;
		}

		private static void AppendRow(StringBuilder stringBuilder, IReadOnlyList<string> row, int[] columnWidths, int maxColumns = -1)
		{
			int columnsToPrint = maxColumns > 0 ? maxColumns : columnWidths.Length;

			string[][] cellLines = new string[columnsToPrint][];
			int maxLinesInRow = 1;

			for (int i = 0; i < columnsToPrint; i++)
			{
				string cell = (i < row.Count && row[i] != null) ? row[i] : string.Empty;
				cellLines[i] = cell.Split('\n');

				if (cellLines[i].Length > maxLinesInRow)
				{
					maxLinesInRow = cellLines[i].Length;
				}
			}

			for (int lineIdx = 0; lineIdx < maxLinesInRow; lineIdx++)
			{
				for (int colIdx = 0; colIdx < columnsToPrint; colIdx++)
				{
					string line = lineIdx < cellLines[colIdx].Length ? cellLines[colIdx][lineIdx] : string.Empty;

					int visualLen = RichTextStripper.Strip(line).Length;
					int padding = Math.Max(0, columnWidths[colIdx] - visualLen);

					stringBuilder.Append(line);
					if (padding > 0)
					{
						stringBuilder.Append(' ', padding);
					}

					if (colIdx < columnsToPrint - 1)
					{
						stringBuilder.Append(" | ");
					}
				}
				stringBuilder.AppendLine();
			}
		}

		private static void AppendSeparator(StringBuilder stringBuilder, int[] columnWidths)
		{
			for (int index = 0; index < columnWidths.Length; index++)
			{
				stringBuilder.Append(new string('-', columnWidths[index]));

				if (index < columnWidths.Length - 1)
				{
					stringBuilder.Append("-+-");
				}
			}

			stringBuilder.AppendLine();
		}
	}
}