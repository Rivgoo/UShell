#nullable enable
using System.Collections.Generic;
using System.Text;

namespace UShell.Runtime.Core.Output.Formatting
{
	public static class TableBuilder
	{
		public static string BuildAsciiTable(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
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
				foreach (var row in rows)
				{
					AppendRow(stringBuilder, row, columnWidths, headers.Count);
				}
			}

			return stringBuilder.ToString();
		}

		private static int[] CalculateColumnWidths(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>>? rows)
		{
			int[] widths = new int[headers.Count];

			for (int index = 0; index < headers.Count; index++)
			{
				widths[index] = headers[index]?.Length ?? 0;
			}

			if (rows == null) return widths;

			foreach (var row in rows)
			{
				for (int index = 0; index < row.Count && index < headers.Count; index++)
				{
					int cellLength = row[index]?.Length ?? 0;
					if (cellLength > widths[index])
					{
						widths[index] = cellLength;
					}
				}
			}

			return widths;
		}

		private static void AppendRow(StringBuilder stringBuilder, IReadOnlyList<string> row, int[] columnWidths, int maxColumns = -1)
		{
			int columnsToPrint = maxColumns > 0 ? maxColumns : columnWidths.Length;

			for (int index = 0; index < columnsToPrint; index++)
			{
				string cell = (index < row.Count && row[index] != null) ? row[index] : string.Empty;
				stringBuilder.Append(cell.PadRight(columnWidths[index]));

				if (index < columnsToPrint - 1)
				{
					stringBuilder.Append(" | ");
				}
			}

			stringBuilder.AppendLine();
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