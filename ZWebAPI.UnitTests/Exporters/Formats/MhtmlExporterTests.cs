using System.Text;
using ZWebAPI.Exporters;
using ZWebAPI.Exporters.Formats;
using ZWebAPI.UnitTests.Factories;
using ZWebAPI.UnitTests.Fakes.ExportersFake;

namespace ZWebAPI.UnitTests.Exporters.Formats
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.Exporters.Formats.MhtmlExporter"/>.
    /// </summary>
    public class MhtmlExporterTests
    {
        /// <summary>
        /// Test the Export should describe the payload as a web archive.
        /// </summary>
        [Fact]
        public void Export_Pass_DescribesThePayloadAsWebArchive()
        {
            // Arrange
            MhtmlExporter exporter = new();
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Text);

            // Act
            ExportResult result = exporter.Export([], columns, "report");

            // Assert
            result.ContentType.Should().Be("message/rfc822");
            result.FileName.Should().Be("report.mhtml");
        }

        /// <summary>
        /// Test the Export should wrap the document in a multipart related MIME envelope.
        /// </summary>
        [Fact]
        public void Export_Pass_WrapsTheDocumentInAMimeEnvelope()
        {
            // Arrange
            MhtmlExporter exporter = new();
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Text);

            // Act
            ExportResult result = exporter.Export([], columns, "report");

            // Assert
            string mime = Encoding.UTF8.GetString(result.Content);
            mime.Should().StartWith("MIME-Version: 1.0\r\n");
            mime.Should().Contain("Content-Type: multipart/related; boundary=\"----=_NextPart_ZWebAPI_Export\"");
            mime.Should().Contain("Content-Type: text/html; charset=\"utf-8\"");
            mime.Should().Contain("Content-Transfer-Encoding: quoted-printable");
            mime.Should().Contain("Content-Location: export.html");
            mime.Should().EndWith("------=_NextPart_ZWebAPI_Export--\r\n");
        }

        /// <summary>
        /// Test the Export should render the headers and the formatted cells of every row.
        /// </summary>
        [Fact]
        public void Export_Pass_RendersHeadersAndRows()
        {
            // Arrange
            MhtmlExporter exporter = new();
            List<object> rows =
            [
                new ExportRowFake { ID = 1, IsActive = true },
                new ExportRowFake { ID = 2 },
            ];
            IReadOnlyList<ExportColumn> columns =
            [
                new("Identifier", ExportColumnType.Text, row => ((ExportRowFake)row).ID),
                new("Active", ExportColumnType.Boolean, row => ((ExportRowFake)row).IsActive),
            ];

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            string html = DecodeQuotedPrintable(Encoding.UTF8.GetString(result.Content));
            html.Should().Contain("<th>Identifier</th><th>Active</th>");
            html.Should().Contain("<td>1</td><td>Sim</td>");
            html.Should().Contain("<td>2</td><td>N&#227;o</td>");
        }

        /// <summary>
        /// Test the Export should HTML-encode the headers and the values.
        /// </summary>
        [Fact]
        public void Export_Pass_HtmlEncodesHeadersAndValues()
        {
            // Arrange
            MhtmlExporter exporter = new();
            List<object> rows = ["<script>alert('x')</script>"];
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Text, "Header & <tag>");

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            string html = DecodeQuotedPrintable(Encoding.UTF8.GetString(result.Content));
            html.Should().Contain("<th>Header &amp; &lt;tag&gt;</th>");
            html.Should().Contain("&lt;script&gt;");
            html.Should().NotContain("<script>");
        }

        /// <summary>
        /// Test the Export should quoted-printable encode the equals sign and the non-ASCII characters.
        /// </summary>
        [Fact]
        public void Export_Pass_QuotedPrintableEncodesReservedCharacters()
        {
            // Arrange
            MhtmlExporter exporter = new();
            List<object> rows = ["a=b"];
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Text);

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            string mime = Encoding.UTF8.GetString(result.Content);
            mime.Should().Contain("a=3Db");
            DecodeQuotedPrintable(mime).Should().Contain("<td>a=b</td>");
        }

        /// <summary>
        /// Test the Export should soft-wrap the encoded payload so no line exceeds the quoted-printable limit.
        /// </summary>
        [Fact]
        public void Export_Pass_SoftWrapsLongEncodedLines()
        {
            // Arrange
            MhtmlExporter exporter = new();
            List<object> rows = [new string('x', 500)];
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Text);

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            string body = ReadEncodedBody(Encoding.UTF8.GetString(result.Content));

            body.Should().Contain("=\r\n");
            body.Split("\r\n").Should().OnlyContain(x => x.Length <= 76);
        }

        /// <summary>
        /// Test the Export should turn the accented characters into HTML numeric entities.
        /// </summary>
        [Fact]
        public void Export_Pass_EncodesNonAsciiCharactersAsHtmlEntities()
        {
            // Arrange
            MhtmlExporter exporter = new();
            List<object> rows = ["ação"];
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Text);

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            string mime = Encoding.UTF8.GetString(result.Content);
            mime.Should().NotContain("ação");
            DecodeQuotedPrintable(mime).Should().Contain("<td>a&#231;&#227;o</td>");
        }

        /// <summary>
        /// Test the Export should pass the line breaks through untouched and restart the line budget.
        /// </summary>
        [Fact]
        public void Export_Pass_PassesLineBreaksThrough()
        {
            // Arrange
            MhtmlExporter exporter = new();
            List<object> rows = ["first\r\nsecond"];
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Text);

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            DecodeQuotedPrintable(Encoding.UTF8.GetString(result.Content)).Should().Contain("<td>first\r\nsecond</td>");
        }

        /// <summary>
        /// Test the Export should render an empty table body when there is no row.
        /// </summary>
        [Fact]
        public void Export_Pass_RendersAnEmptyTableBodyForNoRows()
        {
            // Arrange
            MhtmlExporter exporter = new();
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Text);

            // Act
            ExportResult result = exporter.Export([], columns, "report");

            // Assert
            DecodeQuotedPrintable(Encoding.UTF8.GetString(result.Content)).Should().Contain("<tbody></tbody>");
        }

        /// <summary>
        /// Test the Export should render an empty cell when the value is null.
        /// </summary>
        [Fact]
        public void Export_Pass_RendersEmptyCellForNullValue()
        {
            // Arrange
            MhtmlExporter exporter = new();
            List<object> rows = [ExportColumnFakeFactory.NullValueRow()];
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Text);

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            DecodeQuotedPrintable(Encoding.UTF8.GetString(result.Content)).Should().Contain("<tr><td></td></tr>");
        }

        private static string ReadEncodedBody(string mime)
        {
            const string bodyStart = "Content-Location: export.html\r\n\r\n";
            const string bodyEnd = "\r\n------=_NextPart_ZWebAPI_Export--\r\n";

            int start = mime.IndexOf(bodyStart, StringComparison.Ordinal) + bodyStart.Length;
            int end = mime.LastIndexOf(bodyEnd, StringComparison.Ordinal);

            return mime[start..end];
        }

        private static string DecodeQuotedPrintable(string mime)
        {
            string unfolded = ReadEncodedBody(mime).Replace("=\r\n", string.Empty);
            List<byte> bytes = [];

            for (int i = 0; i < unfolded.Length; i++)
            {
                if (unfolded[i] == '=' && i + 2 < unfolded.Length)
                {
                    bytes.Add(Convert.ToByte(unfolded.Substring(i + 1, 2), 16));
                    i += 2;
                    continue;
                }

                bytes.Add((byte)unfolded[i]);
            }

            return Encoding.UTF8.GetString([.. bytes]);
        }
    }
}
