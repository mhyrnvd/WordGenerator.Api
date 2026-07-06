using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using WordGenerator.Api.Application.DTOs;

namespace WordGenerator.Api.Application.Services
{
    public class ExcelToTableService
    {
        public TableDataDto ConvertExcelToTable(ExcelImportDto importDto)
        {
            try
            {
                var bytes = Convert.FromBase64String(importDto.Base64Content);

                using var stream = new MemoryStream(bytes);
                using var spreadsheet = SpreadsheetDocument.Open(stream, false);

                var workbookPart = spreadsheet.WorkbookPart;
                if (workbookPart == null)
                    throw new Exception("فایل اکسل معتبر نیست");

                // دریافت شیت
                var sheet = GetSheet(workbookPart, importDto.SheetIndex);
                if (sheet == null)
                    throw new Exception($"شیت شماره {importDto.SheetIndex + 1} یافت نشد");

                var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                var worksheet = worksheetPart.Worksheet;
                var sheetData = worksheet.GetFirstChild<SheetData>();

                if (sheetData == null || !sheetData.Elements<Row>().Any())
                    throw new Exception("شیت انتخاب شده خالی است");

                var rows = sheetData.Elements<Row>().ToList();
                if (!rows.Any())
                    throw new Exception("هیچ ردیفی در شیت وجود ندارد");

                // ===== پیدا کردن حداکثر تعداد ستون‌ها =====
                int columnCount = 0;
                foreach (var row in rows)
                {
                    var cellCount = row.Elements<Cell>().Count();
                    if (cellCount > columnCount)
                        columnCount = cellCount;
                }

                if (columnCount == 0)
                    throw new Exception("هیچ ستونی در شیت وجود ندارد");

                // ===== استخراج هدرها =====
                var columns = new List<ColumnDataDto>();
                int startRowIndex = importDto.HasHeader ? 1 : 0;

                if (importDto.HasHeader && rows.Count > 0)
                {
                    var headerRow = rows[0];
                    var headerCells = headerRow.Elements<Cell>().ToList();

                    for (int i = 0; i < columnCount; i++)
                    {
                        string header = "";
                        if (i < headerCells.Count)
                        {
                            header = GetCellValue(headerCells[i], workbookPart);
                        }

                        columns.Add(new ColumnDataDto
                        {
                            Id = i + 1,
                            Header = string.IsNullOrEmpty(header) ? $"ستون {i + 1}" : header,
                            Width = 50,
                            Order = i + 1
                        });
                    }
                }
                else
                {
                    for (int i = 0; i < columnCount; i++)
                    {
                        columns.Add(new ColumnDataDto
                        {
                            Id = i + 1,
                            Header = $"ستون {i + 1}",
                            Width = 50,
                            Order = i + 1
                        });
                    }
                }

                // ===== استخراج داده‌ها =====
                var tableRows = new List<RowDataDto>();
                int rowNumber = 1;

                for (int i = startRowIndex; i < rows.Count; i++)
                {
                    var row = rows[i];
                    var cells = row.Elements<Cell>().ToList();

                    var rowData = new RowDataDto
                    {
                        Id = rowNumber,
                        RowNumber = rowNumber,
                        Values = new List<string>(),
                        Cells = new List<CellDataDto>()
                    };

                    bool hasData = false;

                    for (int col = 0; col < columnCount; col++)
                    {
                        string value = "";
                        if (col < cells.Count)
                        {
                            value = GetCellValue(cells[col], workbookPart);
                        }

                        if (!string.IsNullOrEmpty(value))
                            hasData = true;

                        rowData.Values.Add(value);
                        rowData.Cells.Add(new CellDataDto
                        {
                            Id = col + 1,
                            ColumnId = col + 1,
                            Value = value
                        });
                    }

                    // فقط ردیف‌هایی که داده دارند اضافه می‌شوند
                    if (hasData)
                    {
                        tableRows.Add(rowData);
                        rowNumber++;
                    }
                }

                if (!tableRows.Any())
                    throw new Exception("هیچ داده‌ای در شیت وجود ندارد");

                // ===== ساخت TableDataDto =====
                return new TableDataDto
                {
                    Id = DateTime.Now.Ticks,
                    Title = importDto.TableTitle ?? "جدول از اکسل",
                    Order = 1,
                    ShowRowNumbers = importDto.ShowRowNumbers,
                    RowNumberHeader = importDto.RowNumberHeader ?? "ردیف",
                    Columns = columns,
                    Rows = tableRows
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در تبدیل فایل اکسل: {ex.Message}");
            }
        }

        public ExcelPreviewDto GetExcelPreview(ExcelImportDto importDto)
        {
            try
            {
                var bytes = Convert.FromBase64String(importDto.Base64Content);

                using var stream = new MemoryStream(bytes);
                using var spreadsheet = SpreadsheetDocument.Open(stream, false);

                var workbookPart = spreadsheet.WorkbookPart;
                if (workbookPart == null)
                    throw new Exception("فایل اکسل معتبر نیست");

                var sheet = GetSheet(workbookPart, importDto.SheetIndex);
                if (sheet == null)
                    throw new Exception($"شیت شماره {importDto.SheetIndex + 1} یافت نشد");

                var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                var worksheet = worksheetPart.Worksheet;
                var sheetData = worksheet.GetFirstChild<SheetData>();

                var rows = sheetData?.Elements<Row>().ToList() ?? new List<Row>();

                var previewData = new ExcelPreviewDto
                {
                    SheetName = sheet.Name ?? $"شیت {importDto.SheetIndex + 1}",
                    RowCount = rows.Count,
                    ColumnCount = rows.Any() ? rows.Max(r => r.Elements<Cell>().Count()) : 0,
                    PreviewRows = new List<List<string>>()
                };

                // گرفتن حداکثر 6 ردیف برای پیش‌نمایش
                int maxPreviewRows = Math.Min(6, rows.Count);
                for (int i = 0; i < maxPreviewRows; i++)
                {
                    var row = rows[i];
                    var cells = row.Elements<Cell>().ToList();
                    var rowData = new List<string>();

                    int maxCols = Math.Min(previewData.ColumnCount, 10);
                    for (int col = 0; col < maxCols; col++)
                    {
                        string value = "";
                        if (col < cells.Count)
                        {
                            value = GetCellValue(cells[col], workbookPart);
                        }
                        rowData.Add(value);
                    }
                    previewData.PreviewRows.Add(rowData);
                }

                return previewData;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت پیش‌نمایش: {ex.Message}");
            }
        }

        public List<string> GetSheetNames(ExcelImportDto importDto)
        {
            try
            {
                var bytes = Convert.FromBase64String(importDto.Base64Content);

                using var stream = new MemoryStream(bytes);
                using var spreadsheet = SpreadsheetDocument.Open(stream, false);

                var workbookPart = spreadsheet.WorkbookPart;
                if (workbookPart == null)
                    return new List<string>();

                return workbookPart.Workbook
                    .Descendants<Sheet>()
                    .Select(s => s.Name?.Value ?? "بدون نام")
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        // ===== متدهای کمکی =====

        private Sheet GetSheet(WorkbookPart workbookPart, int sheetIndex)
        {
            var sheets = workbookPart.Workbook.Descendants<Sheet>().ToList();
            if (sheetIndex < 0 || sheetIndex >= sheets.Count)
                return null;

            return sheets[sheetIndex];
        }

        private string GetCellValue(Cell cell, WorkbookPart workbookPart)
        {
            if (cell == null)
                return "";

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                var sharedStringTable = workbookPart.SharedStringTablePart;
                if (sharedStringTable != null)
                {
                    var index = int.Parse(cell.InnerText);
                    return sharedStringTable.SharedStringTable
                        .Elements<SharedStringItem>()
                        .ElementAtOrDefault(index)?
                        .InnerText ?? "";
                }
                return "";
            }

            return cell.InnerText?.Trim() ?? "";
        }
    }

    public class ExcelPreviewDto
    {
        public string SheetName { get; set; } = null!;
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public List<List<string>> PreviewRows { get; set; } = new();
    }
}