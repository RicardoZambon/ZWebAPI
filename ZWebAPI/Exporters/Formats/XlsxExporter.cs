using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;

namespace ZWebAPI.Exporters.Formats
{
    /// <summary>
    /// Excel (.xlsx) exporter using ClosedXML. Writes type-aware cells so numbers, dates,
    /// and currencies remain sortable and filterable in Excel; freezes the header row.
    /// </summary>
    internal sealed class XlsxExporter
    {
        #region Public methods
        public ExportResult Export(IEnumerable<object> rows, IReadOnlyList<ExportColumn> columns, string fileBaseName)
        {
            using XLWorkbook workbook = new();
            IXLWorksheet sheet = workbook.Worksheets.Add("Export");

            for (int c = 0; c < columns.Count; c++)
            {
                IXLCell headerCell = sheet.Cell(1, c + 1);
                headerCell.Value = columns[c].Header;
                headerCell.Style.Font.Bold = true;
            }

            int rowIndex = 2;
            foreach (object row in rows)
            {
                for (int c = 0; c < columns.Count; c++)
                {
                    IXLCell cell = sheet.Cell(rowIndex, c + 1);
                    WriteCell(cell, columns[c].ValueSelector(row), columns[c].Type);
                }
                rowIndex++;
            }

            sheet.SheetView.FreezeRows(1);
            sheet.Columns().AdjustToContents();

            using MemoryStream stream = new();
            workbook.SaveAs(stream);

            return new ExportResult(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{fileBaseName}.xlsx");
        }
        #endregion

        #region Private methods
        private static void WriteCell(IXLCell cell, object? value, ExportColumnType type)
        {
            if (value is null)
            {
                cell.Value = Blank.Value;
                return;
            }

            switch (type)
            {
                case ExportColumnType.Date:
                    if (value is DateTime dt)
                    {
                        cell.Value = dt;
                        cell.Style.DateFormat.Format = "dd/MM/yyyy";
                    }
                    else
                    {
                        cell.Value = value.ToString();
                    }
                    break;

                case ExportColumnType.DateTime:
                    if (value is DateTime dtm)
                    {
                        cell.Value = dtm;
                        cell.Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                    }
                    else
                    {
                        cell.Value = value.ToString();
                    }
                    break;

                case ExportColumnType.Currency:
                    if (value is IConvertible conv1)
                    {
                        cell.Value = Convert.ToDecimal(conv1, ExportValueFormatter.Culture);
                        cell.Style.NumberFormat.Format = "\"R$\" #,##0.00";
                    }
                    else
                    {
                        cell.Value = value.ToString();
                    }
                    break;

                case ExportColumnType.Number:
                    if (value is IConvertible conv2)
                    {
                        cell.Value = Convert.ToDouble(conv2, ExportValueFormatter.Culture);
                    }
                    else
                    {
                        cell.Value = value.ToString();
                    }
                    break;

                case ExportColumnType.Boolean:
                    if (value is bool b)
                    {
                        cell.Value = b ? "Sim" : "Não";
                    }
                    else
                    {
                        cell.Value = value.ToString();
                    }
                    break;

                default:
                    cell.Value = value.ToString();
                    break;
            }
        }
        #endregion
    }
}
