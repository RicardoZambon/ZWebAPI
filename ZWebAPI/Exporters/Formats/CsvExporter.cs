using System.Collections.Generic;
using System.Text;

namespace ZWebAPI.Exporters.Formats
{
    /// <summary>
    /// RFC 4180 CSV exporter. Writes a UTF-8 BOM so Excel opens the file with correct encoding.
    /// </summary>
    internal sealed class CsvExporter
    {
        #region Public methods
        public ExportResult Export(IEnumerable<object> rows, IReadOnlyList<ExportColumn> columns, string fileBaseName)
        {
            StringBuilder builder = new();

            for (int i = 0; i < columns.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(',');
                }
                builder.Append(Escape(columns[i].Header));
            }
            builder.Append("\r\n");

            foreach (object row in rows)
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(',');
                    }
                    object? raw = columns[i].ValueSelector(row);
                    string formatted = ExportValueFormatter.Format(raw, columns[i].Type);
                    builder.Append(Escape(formatted));
                }
                builder.Append("\r\n");
            }

            byte[] bom = Encoding.UTF8.GetPreamble();
            byte[] body = Encoding.UTF8.GetBytes(builder.ToString());
            byte[] content = new byte[bom.Length + body.Length];
            Buffer.BlockCopy(bom, 0, content, 0, bom.Length);
            Buffer.BlockCopy(body, 0, content, bom.Length, body.Length);

            return new ExportResult(content, "text/csv; charset=utf-8", $"{fileBaseName}.csv");
        }
        #endregion

        #region Private methods
        private static string Escape(string value)
        {
            if (value.IndexOfAny([',', '"', '\r', '\n']) < 0)
            {
                return value;
            }
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
        #endregion
    }
}
