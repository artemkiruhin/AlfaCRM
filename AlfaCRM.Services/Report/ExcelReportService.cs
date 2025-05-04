using System.Data;
using AlfaCRM.Domain.Models.DTOs;
using AlfaCRM.Domain.Models.DTOs.Report;
using ClosedXML.Excel;

namespace AlfaCRM.Services.Report
{
    public class ExcelReportService
    {
        private const string DefaultFontName = "Inter";
        private const int DefaultFontSize = 11;
        private const string DefaultAccentColor = "#4f46e5";
        private const string DefaultAltRowColor = "#f9fafb";
        private const string DefaultTitleColor = "#1f2937";
        private const string DefaultDescriptionColor = "#4b5563";
        private const int MaxColumnWidth = 50;
        
        #region Specific Reports
        
        public byte[] GenerateDepartmentsReport(IEnumerable<DepartmentReportDTO> departments, string? description)
        {
            var table = new ReportTable
            {
                TableName = "Отделы",
                Columns = new List<ReportColumn>
                {
                    new ReportColumn { ColumnName = "ID", DataType = "string" },
                    new ReportColumn { ColumnName = "Название", DataType = "string" },
                    new ReportColumn { ColumnName = "Специфический", DataType = "boolean" },
                    new ReportColumn { ColumnName = "Количество участников", DataType = "number" }
                },
                Rows = departments.Select(dept => new List<object>
                {
                    dept.Id.ToString(),
                    dept.Name,
                    dept.IsSpecific,
                    dept.MembersCount
                }).ToList()
            };

            var request = new ExcelReportRequest
            {
                DocumentTitle = description ?? "Отчет по отделам",
                Description = GenerateReportDescription(),
                Tables = new List<ReportTable> { table },
                AutoFitColumns = true
            };

            return GenerateExcelReport(request);
        }

        public byte[] GeneratePostsReport(IEnumerable<PostReportDTO> posts, string? description)
        {
            var table = new ReportTable
            {
                TableName = "Новости",
                Columns = new List<ReportColumn>
                {
                    new ReportColumn { ColumnName = "ID", DataType = "string" },
                    new ReportColumn { ColumnName = "Заголовок", DataType = "string" },
                    new ReportColumn { ColumnName = "Подзаголовок", DataType = "string" },
                    new ReportColumn { ColumnName = "Содержание", DataType = "string" },
                    new ReportColumn { ColumnName = "Дата создания", DataType = "date" },
                    new ReportColumn { ColumnName = "Дата изменения", DataType = "date" },
                    new ReportColumn { ColumnName = "Важный", DataType = "boolean" },
                    new ReportColumn { ColumnName = "Актуальный", DataType = "boolean" },
                    new ReportColumn { ColumnName = "Автор (логин)", DataType = "string" },
                    new ReportColumn { ColumnName = "Автор (ФИО)", DataType = "string" },
                    new ReportColumn { ColumnName = "Отдел (ID)", DataType = "string" },
                    new ReportColumn { ColumnName = "Отдел", DataType = "string" },
                    new ReportColumn { ColumnName = "Лайки", DataType = "number" },
                    new ReportColumn { ColumnName = "Дизлайки", DataType = "number" },
                    new ReportColumn { ColumnName = "Комментарии", DataType = "number" }
                },
                Rows = posts.Select(post => new List<object>
                {
                    post.Id.ToString(),
                    post.Title,
                    post.Subtitle,
                    post.Content,
                    post.CreatedAt,
                    post.ModifiedAt,
                    post.IsImportant,
                    post.IsActual,
                    post.PublisherUsername,
                    post.PublisherFullName,
                    post.DepartmentGuid,
                    post.DepartmentName,
                    post.LikesCount,
                    post.DislikesCount,
                    post.CommentsCount
                }).ToList()
            };

            var request = new ExcelReportRequest
            {
                DocumentTitle = description ?? "Отчет по новостям",
                Description = GenerateReportDescription(),
                Tables = new List<ReportTable> { table },
                ColumnFormats = new Dictionary<string, string>
                {
                    { "Дата создания", "dd.MM.yyyy HH:mm" },
                    { "Дата изменения", "dd.MM.yyyy HH:mm" }
                },
                AutoFitColumns = true
            };

            return GenerateExcelReport(request);
        }

        public byte[] GenerateTicketsReport(IEnumerable<TicketReportDTO> tickets, string? description)
        {
            var table = new ReportTable
            {
                TableName = "Заявки",
                Columns = new List<ReportColumn>
                {
                    new ReportColumn { ColumnName = "ID", DataType = "string" },
                    new ReportColumn { ColumnName = "Заголовок", DataType = "string" },
                    new ReportColumn { ColumnName = "Текст", DataType = "string" },
                    new ReportColumn { ColumnName = "Обратная связь", DataType = "string" },
                    new ReportColumn { ColumnName = "Отдел (ID)", DataType = "string" },
                    new ReportColumn { ColumnName = "Отдел", DataType = "string" },
                    new ReportColumn { ColumnName = "Дата создания", DataType = "date" },
                    new ReportColumn { ColumnName = "Статус", DataType = "string" },
                    new ReportColumn { ColumnName = "Исполнитель (логин)", DataType = "string" },
                    new ReportColumn { ColumnName = "Исполнитель (ФИО)", DataType = "string" },
                    new ReportColumn { ColumnName = "Создатель (логин)", DataType = "string" },
                    new ReportColumn { ColumnName = "Создатель (ФИО)", DataType = "string" },
                    new ReportColumn { ColumnName = "Дата закрытия", DataType = "date" },
                    new ReportColumn { ColumnName = "Тип", DataType = "string" }
                },
                Rows = tickets.Select(ticket => new List<object>
                {
                    ticket.Id.ToString(),
                    ticket.Title,
                    ticket.Text,
                    ticket.Feedback,
                    ticket.DepartmentGuid,
                    ticket.DepartmentName,
                    ticket.CreatedAt,
                    ticket.Status,
                    ticket.AssigneeUsername,
                    ticket.AssigneeFullName,
                    ticket.CreatorUsername,
                    ticket.CreatorFullName,
                    ticket.ClosedAt,
                    ticket.Type
                }).ToList()
            };

            var request = new ExcelReportRequest
            {
                DocumentTitle = description ?? "Отчет по заявкам",
                Description = GenerateReportDescription(),
                Tables = new List<ReportTable> { table },
                ColumnFormats = new Dictionary<string, string>
                {
                    { "Дата создания", "dd.MM.yyyy HH:mm" },
                    { "Дата закрытия", "dd.MM.yyyy HH:mm" }
                },
                AutoFitColumns = true
            };

            return GenerateExcelReport(request);
        }

        public byte[] GenerateUsersReport(IEnumerable<UserReportDTO> users, string? description)
        {
            var table = new ReportTable
            {
                TableName = "Пользователи",
                Columns = new List<ReportColumn>
                {
                    new ReportColumn { ColumnName = "ID", DataType = "string" },
                    new ReportColumn { ColumnName = "ФИО", DataType = "string" },
                    new ReportColumn { ColumnName = "Логин", DataType = "string" },
                    new ReportColumn { ColumnName = "Email", DataType = "string" },
                    new ReportColumn { ColumnName = "Дата приема", DataType = "date" },
                    new ReportColumn { ColumnName = "Дата увольнения", DataType = "date" },
                    new ReportColumn { ColumnName = "Дата рождения", DataType = "date" },
                    new ReportColumn { ColumnName = "Пол", DataType = "string" },
                    new ReportColumn { ColumnName = "Админ", DataType = "boolean" },
                    new ReportColumn { ColumnName = "Права публикации", DataType = "boolean" },
                    new ReportColumn { ColumnName = "Заблокирован", DataType = "boolean" },
                    new ReportColumn { ColumnName = "Отдел (ID)", DataType = "string" },
                    new ReportColumn { ColumnName = "Отдел", DataType = "string" },
                    new ReportColumn { ColumnName = "Опубликовано постов", DataType = "number" }
                },
                Rows = users.Select(user => new List<object>
                {
                    user.Id.ToString(),
                    user.FullName,
                    user.Username,
                    user.Email,
                    user.HiredAt,
                    user.FiredAt,
                    user.Birthday,
                    user.Sex,
                    user.IsAdmin,
                    user.HasPublishedRights,
                    user.IsBlocked,
                    user.DepartmentGuid,
                    user.DepartmentName,
                    user.PublishedPostsCount
                }).ToList()
            };

            var request = new ExcelReportRequest
            {
                DocumentTitle = description ?? "Отчет по пользователям",
                Description = GenerateReportDescription(),
                Tables = new List<ReportTable> { table },
                ColumnFormats = new Dictionary<string, string>
                {
                    { "Дата приема", "dd.MM.yyyy" },
                    { "Дата увольнения", "dd.MM.yyyy" },
                    { "Дата рождения", "dd.MM.yyyy" }
                },
                AutoFitColumns = true
            };

            return GenerateExcelReport(request);
        }
        
        #endregion

        #region Generic Report Generator
        public byte[] GenerateExcelReport(ExcelReportRequest request)
        {
            using var workbook = new XLWorkbook();
            
            foreach (var reportTable in request.Tables)
            {
                var dataTable = ConvertToDataTable(reportTable);
                var worksheet = workbook.Worksheets.Add(dataTable.TableName ?? "Данные");
                
                ApplyBaseWorksheetStyle(worksheet);
                AddTitleSection(worksheet, request);
                AddColumnHeaders(worksheet, dataTable);
                AddDataRows(worksheet, dataTable, request.ColumnFormats ?? new Dictionary<string, string>());
                
                if (request.AutoFitColumns)
                {
                    AutoFitColumns(worksheet);
                }
                
                AddReportFooter(worksheet, dataTable.Columns.Count);
            }

            return SaveWorkbookToByteArray(workbook);
        }
        
        #endregion

        #region Helper Methods
        private void ApplyNumberFormat(IXLCell cell, string columnName, Dictionary<string, string> columnFormats)
        {
            cell.Style.NumberFormat.Format = columnFormats.TryGetValue(columnName, out string format) ? format : "#,##0.00";
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        }
        private string GenerateReportDescription(string additionalInfo = null)
        {
            var baseDescription = $"Отчет сформирован с помощью платформы AlfaCRM {DateTime.Now:dd.MM.yyyy HH:mm:ss}";
            
            if (!string.IsNullOrWhiteSpace(additionalInfo))
            {
                return $"{additionalInfo}\n{baseDescription}";
            }
            
            return baseDescription;
        }
        
        private void ApplyBaseWorksheetStyle(IXLWorksheet worksheet)
        {
            worksheet.Style.Font.FontName = DefaultFontName;
            worksheet.Style.Font.FontSize = DefaultFontSize;
        }

        private void AddTitleSection(IXLWorksheet worksheet, ExcelReportRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DocumentTitle))
                return;
            
            var titleCell = worksheet.Cell(1, 1);
            titleCell.SetValue(request.DocumentTitle);
            titleCell.Style.Font.Bold = true;
            titleCell.Style.Font.FontSize = 18;
            titleCell.Style.Font.FontColor = XLColor.FromHtml(DefaultTitleColor);
            titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            
            int columnsCount = Math.Max(worksheet.ColumnsUsed().Count(), 10);
            worksheet.Range(1, 1, 1, columnsCount).Merge();

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                var descCell = worksheet.Cell(2, 1);
                descCell.SetValue(request.Description);
                descCell.Style.Font.FontColor = XLColor.FromHtml(DefaultDescriptionColor);
                descCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                descCell.Style.Font.Italic = true;
                worksheet.Range(2, 1, 2, columnsCount).Merge();
            }
        }
        private void AddColumnHeaders(IXLWorksheet worksheet, DataTable dataTable)
        {
            int headerRowIndex = 4;
            var headerRow = worksheet.Row(headerRowIndex);
            
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                var cell = headerRow.Cell(i + 1);
                cell.SetValue(dataTable.Columns[i].ColumnName);
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml(DefaultAccentColor);
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.OutsideBorderColor = XLColor.FromHtml(DefaultAccentColor);
            }
            
            worksheet.SheetView.Freeze(headerRowIndex, 0);
        }
        private void AddDataRows(IXLWorksheet worksheet, DataTable dataTable, Dictionary<string, string> columnFormats)
        {
            int dataStartRow = 5;
            
            for (int rowIndex = 0; rowIndex < dataTable.Rows.Count; rowIndex++)
            {
                var dataRow = dataTable.Rows[rowIndex];
                var worksheetRow = worksheet.Row(dataStartRow + rowIndex);
                
                worksheetRow.Style.Fill.BackgroundColor = rowIndex % 2 == 0 
                    ? XLColor.White 
                    : XLColor.FromHtml(DefaultAltRowColor);

                for (int colIndex = 0; colIndex < dataTable.Columns.Count; colIndex++)
                {
                    var cell = worksheetRow.Cell(colIndex + 1);
                    var columnName = dataTable.Columns[colIndex].ColumnName;
                    var value = dataRow[colIndex];
                    
                    if (value == DBNull.Value)
                    {
                        cell.SetValue(string.Empty);
                        continue;
                    }
                    
                    if (value is DateTime dateValue)
                    {
                        cell.SetValue(dateValue);

                        if (columnFormats.TryGetValue(columnName, out string format))
                        {
                            cell.Style.DateFormat.Format = format;
                        }
                        else if (!dateValue.TimeOfDay.Equals(TimeSpan.Zero))
                        {
                            cell.Style.DateFormat.Format = "dd.MM.yyyy HH:mm";
                        }
                        else
                        {
                            cell.Style.DateFormat.Format = "dd.MM.yyyy";
                        }
                    }
                    else if (value is bool boolValue)
                    {
                        cell.SetValue(boolValue ? "Да" : "Нет");
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else if (value is decimal decValue)
                    {
                        cell.SetValue(decValue);
                        ApplyNumberFormat(cell, columnName, columnFormats);
                    }
                    else if (value is double dblValue)
                    {
                        cell.SetValue(dblValue);
                        ApplyNumberFormat(cell, columnName, columnFormats);
                    }
                    else if (value is float fltValue)
                    {
                        cell.SetValue(fltValue);
                        ApplyNumberFormat(cell, columnName, columnFormats);
                    }
                    else if (value is int intValue)
                    {
                        cell.SetValue(intValue);
                        ApplyNumberFormat(cell, columnName, columnFormats);
                    }
                    else
                    {
                        cell.SetValue(value.ToString());
                    }
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#e5e7eb");
                }
            }
        }
        private void AutoFitColumns(IXLWorksheet worksheet)
        {
            worksheet.Columns().AdjustToContents();
            worksheet.Columns().Where(c => c.Width > MaxColumnWidth).ToList()
                .ForEach(c => c.Width = MaxColumnWidth);
        }
        private void AddReportFooter(IXLWorksheet worksheet, int columnsCount)
        {
            int lastRow = worksheet.LastRowUsed().RowNumber();
            int footerRow = lastRow + 2;
            
            var footerCell = worksheet.Cell(footerRow, 1);
            footerCell.SetValue($"Отчет сформирован с помощью платформы AlfaCRM {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            footerCell.Style.Font.Italic = true;
            footerCell.Style.Font.FontColor = XLColor.FromHtml(DefaultDescriptionColor);
            
            if (columnsCount > 1)
            {
                worksheet.Range(footerRow, 1, footerRow, columnsCount).Merge();
            }
        }
        private DataTable ConvertToDataTable(ReportTable reportTable)
        {
            var dataTable = new DataTable(reportTable.TableName);
            foreach (var column in reportTable.Columns)
            {
                dataTable.Columns.Add(column.ColumnName, GetColumnType(column.DataType));
            }

            foreach (var row in reportTable.Rows)
            {
                var dataRow = dataTable.NewRow();
                
                for (int i = 0; i < Math.Min(row.Count, reportTable.Columns.Count); i++)
                {
                    dataRow[i] = ConvertValue(row[i], reportTable.Columns[i].DataType);
                }
                
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }
        private Type GetColumnType(string dataType)
        {
            return dataType?.ToLower() switch
            {
                "number" => typeof(decimal),
                "date" => typeof(DateTime),
                "boolean" => typeof(bool),
                _ => typeof(string)
            };
        }
        private object ConvertValue(object value, string dataType)
        {
            if (value == null)
                return DBNull.Value;

            try
            {
                return dataType?.ToLower() switch
                {
                    "number" => Convert.ToDecimal(value),
                    "date" => value is DateTime ? value : Convert.ToDateTime(value),
                    "boolean" => value is bool ? value : Convert.ToBoolean(value),
                    _ => value.ToString()
                };
            }
            catch (Exception)
            {
                return value.ToString();
            }
        }
        
        private byte[] SaveWorkbookToByteArray(XLWorkbook workbook)
        {
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
        
        #endregion
    }
}