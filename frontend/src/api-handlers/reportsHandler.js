import { API_URL } from "./baseHandler";
import { saveAs } from 'file-saver';

/**
 * Универсальный метод для экспорта данных в Excel
 * @param {Array} data - Массив объектов с данными для экспорта
 * @param {string} title - Заголовок отчета
 * @param {string} description - Описание отчета
 * @param {Object} options - Дополнительные параметры
 * @param {Array} options.columnNames - Названия колонок (по умолчанию - ключи первого объекта)
 * @param {Object} options.columnFormats - Форматы колонок (например, { "Дата": "dd.MM.yyyy" })
 * @param {string} options.endpoint - Конечная точка API (по умолчанию '/reports/export-excel')
 * @param {string} options.filename - Имя файла (по умолчанию 'Отчет_дата.xlsx')
 */
const exportToExcel = async (
    data,
    title,
    description,
    options = {}
) => {
    try {
        const {
            columnNames = [],
            columnFormats = {},
            endpoint = '/reports/export-excel',
            filename = `Отчет_${new Date().toISOString().slice(0, 10)}.xlsx`
        } = options;

        const headers = columnNames.length > 0
            ? columnNames
            : data.length > 0
                ? Object.keys(data[0])
                : [];

        const exportData = {
            DocumentTitle: title,
            Description: description,
            Tables: [{
                TableName: "Данные",
                Columns: headers.map(header => ({
                    ColumnName: header,
                    DataType: typeof data[0]?.[header] === 'number' ? 'number' : 'string'
                })),
                Rows: data.map(item =>
                    headers.map(header => item[header])
                )
            }],
            ColumnFormats: columnFormats
        };

        const response = await fetch(`${API_URL}${endpoint}`, {
            method: "POST",
            body: JSON.stringify(exportData),
            credentials: "include",
            headers: {
                "Content-Type": "application/json"
            }
        });

        const blob = await response.blob();
        saveAs(blob, filename);
        return true;
    } catch (error) {
        console.error('Ошибка при экспорте в Excel:', error);
        throw error;
    }
};

export {
    exportToExcel
}