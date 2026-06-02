using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ZWebAPI.Exporters.Formats
{
    /// <summary>
    /// XML exporter. Emits a simple <c>&lt;rows&gt;&lt;row&gt;&lt;column name="..."&gt;value&lt;/column&gt;...&lt;/row&gt;&lt;/rows&gt;</c>
    /// document using the column headers as attribute values. Columns are emitted under a
    /// <c>&lt;column&gt;</c> element (rather than as dynamic element names) so any header string
    /// works without XML naming restrictions.
    /// </summary>
    internal sealed class XmlExporter
    {
        #region Public methods
        public ExportResult Export(IEnumerable<object> rows, IReadOnlyList<ExportColumn> columns, string fileBaseName)
        {
            using MemoryStream stream = new();
            XmlWriterSettings settings = new()
            {
                Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                Indent = true,
            };

            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("rows");

                foreach (object row in rows)
                {
                    writer.WriteStartElement("row");
                    for (int i = 0; i < columns.Count; i++)
                    {
                        object? raw = columns[i].ValueSelector(row);
                        string formatted = ExportValueFormatter.Format(raw, columns[i].Type);

                        writer.WriteStartElement("column");
                        writer.WriteAttributeString("name", columns[i].Header);
                        writer.WriteString(formatted);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            return new ExportResult(stream.ToArray(), "application/xml; charset=utf-8", $"{fileBaseName}.xml");
        }
        #endregion
    }
}
