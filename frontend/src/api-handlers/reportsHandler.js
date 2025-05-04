import { API_URL } from "./baseHandler";
import { saveAs } from 'file-saver';

/**
 * Экспорт данных в Excel через API бекенда
 * @param {string} table - Тип таблицы: 'Departments', 'Posts', 'Tickets' или 'Users'
 * @param {string} title - Заголовок отчета
 * @param {string} [description] - Описание отчета (опционально)
 * @param {Object} [options] - Дополнительные параметры
 * @param {string} [options.filename] - Имя файла (по умолчанию 'Title_дата.xlsx')
 * @returns {Promise<boolean>} Возвращает true при успешном экспорте
 * @throws {Error} При ошибке запроса
 */
export const exportToExcel = async (
    table,
    title,
    description,
    options = {}
) => {
    try {
        const {
            filename = `${title}_${new Date().toISOString().slice(0, 10)}.xlsx`
        } = options;

        const requestData = {
            title: title ?? "Отчет",
            description: description ?? "",
            table: table
        };

        const response = await fetch(`${API_URL}/reports/export-excel`, {
            method: "POST",
            body: JSON.stringify(requestData),
            credentials: "include",
            headers: {
                "Content-Type": "application/json"
            }
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.error );
        }

        const blob = await response.blob();
        saveAs(blob, filename);
        return true;
    } catch (error) {
        console.error('Ошибка при экспорте в Excel:', error);
        throw error;
    }
};