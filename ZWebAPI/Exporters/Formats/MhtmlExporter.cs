using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ZWebAPI.Exporters.Formats
{
    /// <summary>
    /// Web-archive (.mhtml) exporter. Renders an HTML table and wraps it in a
    /// <c>multipart/related</c> MIME envelope so browsers open the file as a saved web page.
    /// </summary>
    internal sealed class MhtmlExporter
    {
        #region Variables
        private const string Boundary = "----=_NextPart_ZWebAPI_Export";
        #endregion

        #region Public methods
        public ExportResult Export(IEnumerable<object> rows, IReadOnlyList<ExportColumn> columns, string fileBaseName)
        {
            string html = BuildHtml(rows, columns);
            string encodedHtml = QuotedPrintableEncode(html);

            StringBuilder mime = new();
            mime.Append("MIME-Version: 1.0\r\n");
            mime.Append($"Content-Type: multipart/related; boundary=\"{Boundary}\"\r\n");
            mime.Append("\r\n");
            mime.Append($"--{Boundary}\r\n");
            mime.Append("Content-Type: text/html; charset=\"utf-8\"\r\n");
            mime.Append("Content-Transfer-Encoding: quoted-printable\r\n");
            mime.Append("Content-Location: export.html\r\n");
            mime.Append("\r\n");
            mime.Append(encodedHtml);
            mime.Append("\r\n");
            mime.Append($"--{Boundary}--\r\n");

            byte[] content = Encoding.UTF8.GetBytes(mime.ToString());
            return new ExportResult(content, "message/rfc822", $"{fileBaseName}.mhtml");
        }
        #endregion

        #region Private methods
        private static string BuildHtml(IEnumerable<object> rows, IReadOnlyList<ExportColumn> columns)
        {
            StringBuilder html = new();
            html.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><style>");
            html.Append("body{font-family:Arial,sans-serif;font-size:12px;}");
            html.Append("table{border-collapse:collapse;width:100%;}");
            html.Append("th,td{border:1px solid #888;padding:4px 8px;text-align:left;}");
            html.Append("th{background:#e0e0e0;}");
            html.Append("tr:nth-child(even) td{background:#f7f7f7;}");
            html.Append("</style></head><body><table><thead><tr>");

            foreach (ExportColumn column in columns)
            {
                html.Append("<th>").Append(WebUtility.HtmlEncode(column.Header)).Append("</th>");
            }
            html.Append("</tr></thead><tbody>");

            foreach (object row in rows)
            {
                html.Append("<tr>");
                for (int i = 0; i < columns.Count; i++)
                {
                    object? raw = columns[i].ValueSelector(row);
                    string formatted = ExportValueFormatter.Format(raw, columns[i].Type);
                    html.Append("<td>").Append(WebUtility.HtmlEncode(formatted)).Append("</td>");
                }
                html.Append("</tr>");
            }

            html.Append("</tbody></table></body></html>");
            return html.ToString();
        }

        private static string QuotedPrintableEncode(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            StringBuilder result = new();
            int lineLength = 0;

            foreach (byte b in bytes)
            {
                bool needsEncoding = b == (byte)'=' || b < 32 || b > 126;
                if (b == (byte)'\r' || b == (byte)'\n')
                {
                    result.Append((char)b);
                    lineLength = 0;
                    continue;
                }

                string chunk = needsEncoding ? $"={b:X2}" : ((char)b).ToString();

                if (lineLength + chunk.Length > 75)
                {
                    result.Append("=\r\n");
                    lineLength = 0;
                }

                result.Append(chunk);
                lineLength += chunk.Length;
            }

            return result.ToString();
        }
        #endregion
    }
}
