import React, {useEffect, useState} from 'react';
import './LogListPage.css';
import Header from "../../components/layout/header/Header";
import {getAllLogs} from "../../api-handlers/logsHandler";
import {useNavigate} from "react-router-dom";
import {FileDown} from 'lucide-react';
import ExportModal from '../../components/layout/modal/export/ExportModal';
import {exportToExcel} from "../../api-handlers/reportsHandler";

const LogListPage = () => {
    const navigate = useNavigate();

    const [logs, setLogs] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [filter, setFilter] = useState('all');
    const [currentPage, setCurrentPage] = useState(1);
    const logsPerPage = 10;

    const [isExportModalOpen, setIsExportModalOpen] = useState(false);

    const filteredLogs = logs.filter(log => {
        const matchesSearch = log.message.toLowerCase().includes(searchTerm.toLowerCase()) ||
            log.username.toLowerCase().includes(searchTerm.toLowerCase()) ||
            log.type.toLowerCase().includes(searchTerm.toLowerCase());

        const matchesFilter = filter === 'all' ||
            (filter === 'info' && log.type === 'Info') ||
            (filter === 'warning' && log.type === 'Warning') ||
            (filter === 'error' && log.type === 'Error');

        return matchesSearch && matchesFilter;
    });

    const indexOfLastLog = currentPage * logsPerPage;
    const indexOfFirstLog = indexOfLastLog - logsPerPage;
    const currentLogs = filteredLogs.slice(indexOfFirstLog, indexOfLastLog);
    const totalPages = Math.ceil(filteredLogs.length / logsPerPage);

    const paginate = (pageNumber) => setCurrentPage(pageNumber);
    const nextPage = () => setCurrentPage(prev => Math.min(prev + 1, totalPages));
    const prevPage = () => setCurrentPage(prev => Math.max(prev - 1, 1));

    const fetchLogs = async () => {
        try {
            setIsLoading(true);
            const response = await getAllLogs();
            setLogs(response);
        } catch (error) {
            console.error("Ошибка при загрузке логов:", error);
        } finally {
            setIsLoading(false);
        }
    };

    const handleExportClick = () => {
        setIsExportModalOpen(true);
    };

    const handleExportConfirm = async (filename, description) => {
        try {
            await exportToExcel(6, filename || "Отчет_по_логам", description || "");
            setIsExportModalOpen(false);
        } catch (error) {
            console.error('Ошибка при экспорте:', error);
            alert('Произошла ошибка при экспорте данных');
        }
    };

    useEffect(() => {
        fetchLogs();
    }, []);

    return (
        <div className="log-list-page">
            <Header title={"Журнал событий"} />
            <div className="log-list-header">
                <h1 className="log-list-title"></h1>
                <div className="header-actions">
                    <button
                        className="admin-button secondary"
                        onClick={handleExportClick}
                        disabled={isLoading || logs.length === 0}
                    >
                        <FileDown size={16} />
                        Экспорт в Excel
                    </button>
                </div>
            </div>

            {isLoading && logs.length === 0 ? (
                <div className="loading-state">
                    <div className="loading-spinner"></div>
                    <p>Загрузка логов...</p>
                </div>
            ) : (
                <>
                    <div className="log-list-toolbar">
                        <div className="search-container">
                            <input
                                type="text"
                                placeholder="Поиск по сообщению, пользователю или типу..."
                                className="search-input"
                                value={searchTerm}
                                onChange={(e) => {
                                    setSearchTerm(e.target.value);
                                    setCurrentPage(1);
                                }}
                                disabled={isLoading}
                            />
                            <span className="search-icon">🔍</span>
                        </div>

                        <div className="filters-container">
                            <select
                                className="filter-select"
                                value={filter}
                                onChange={(e) => {
                                    setFilter(e.target.value);
                                    setCurrentPage(1);
                                }}
                                disabled={isLoading}
                            >
                                <option value="all">Все типы</option>
                                <option value="info">Информация</option>
                                <option value="warning">Предупреждения</option>
                                <option value="error">Ошибки</option>
                            </select>

                            <div className="results-count">
                                Найдено: {filteredLogs.length}
                            </div>
                        </div>
                    </div>

                    <div className="log-table-container">
                        <table className="log-table">
                            <thead>
                            <tr>
                                <th>Дата</th>
                                <th>Тип</th>
                                <th>Пользователь</th>
                                <th>Сообщение</th>
                            </tr>
                            </thead>
                            <tbody>
                            {currentLogs.length > 0 ? (
                                currentLogs.map(log => (
                                    <tr key={log.id}>
                                        <td>
                                            {new Date(log.createdAt).toLocaleString()}
                                        </td>
                                        <td>
                                            <span className={`log-type type-${log.type.toLowerCase()}`}>
                                                {log.type}
                                            </span>
                                        </td>
                                        <td>{log.username || 'Система'}</td>
                                        <td className="log-message">{log.message}</td>
                                    </tr>
                                ))
                            ) : (
                                <tr>
                                    <td colSpan="4" className="empty-state">
                                        <div className="empty-icon">😕</div>
                                        <h3 className="empty-title">Логи не найдены</h3>
                                        <p className="empty-description">Попробуйте изменить параметры поиска</p>
                                    </td>
                                </tr>
                            )}
                            </tbody>
                        </table>
                    </div>

                    {filteredLogs.length > logsPerPage && (
                        <div className="pagination-container">
                            <div className="pagination-info">
                                Показано {indexOfFirstLog + 1}-{Math.min(indexOfLastLog, filteredLogs.length)} из {filteredLogs.length}
                            </div>
                            <div className="pagination-controls">
                                <button
                                    onClick={prevPage}
                                    disabled={currentPage === 1 || isLoading}
                                    className={`pagination-button ${currentPage === 1 ? 'disabled' : ''}`}
                                >
                                    ←
                                </button>

                                {Array.from({length: Math.min(5, totalPages)}, (_, i) => {
                                    let pageNum;
                                    if (totalPages <= 5) {
                                        pageNum = i + 1;
                                    } else if (currentPage <= 3) {
                                        pageNum = i + 1;
                                    } else if (currentPage >= totalPages - 2) {
                                        pageNum = totalPages - 4 + i;
                                    } else {
                                        pageNum = currentPage - 2 + i;
                                    }

                                    return (
                                        <button
                                            key={pageNum}
                                            onClick={() => paginate(pageNum)}
                                            className={`pagination-button ${currentPage === pageNum ? 'active' : ''}`}
                                            disabled={isLoading}
                                        >
                                            {pageNum}
                                        </button>
                                    );
                                })}

                                {totalPages > 5 && currentPage < totalPages - 2 && (
                                    <span className="pagination-ellipsis">...</span>
                                )}

                                {totalPages > 5 && currentPage < totalPages - 2 && (
                                    <button
                                        onClick={() => paginate(totalPages)}
                                        className={`pagination-button ${currentPage === totalPages ? 'active' : ''}`}
                                        disabled={isLoading}
                                    >
                                        {totalPages}
                                    </button>
                                )}

                                <button
                                    onClick={nextPage}
                                    disabled={currentPage === totalPages || isLoading}
                                    className={`pagination-button ${currentPage === totalPages ? 'disabled' : ''}`}
                                >
                                    →
                                </button>
                            </div>
                        </div>
                    )}
                </>
            )}

            <ExportModal
                isOpen={isExportModalOpen}
                onClose={() => setIsExportModalOpen(false)}
                onExport={handleExportConfirm}
                defaultFilename="Отчет_по_логам"
            />
        </div>
    );
};

export default LogListPage;