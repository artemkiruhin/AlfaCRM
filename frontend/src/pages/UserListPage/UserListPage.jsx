import React, {useEffect, useState} from 'react';
import './UserListPage.css';
import Header from "../../components/layout/header/Header";
import {deleteUser, getAllUsers} from "../../api-handlers/usersHandler";
import {useNavigate} from "react-router-dom";
import {FileDown} from 'lucide-react';
import ExportModal from '../../components/layout/modal/export/ExportModal';
import {exportToExcel} from "../../api-handlers/reportsHandler";

const UserListPage = () => {
    const navigate = useNavigate();

    const [users, setUsers] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [filter, setFilter] = useState('all');
    const [currentPage, setCurrentPage] = useState(1);
    const usersPerPage = 10;

    const [showDeleteModal, setShowDeleteModal] = useState(false);
    const [userToDelete, setUserToDelete] = useState(null);
    const [deleteError, setDeleteError] = useState(null);
    const [isExportModalOpen, setIsExportModalOpen] = useState(false);

    const filteredUsers = users.filter(user => {
        const matchesSearch = user.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
            user.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
            user.username.toLowerCase().includes(searchTerm.toLowerCase());
        const matchesFilter = filter === 'all' ||
            (filter === 'active' && !user.isBlocked) ||
            (filter === 'inactive' && user.isBlocked);
        return matchesSearch && matchesFilter;
    });

    const indexOfLastUser = currentPage * usersPerPage;
    const indexOfFirstUser = indexOfLastUser - usersPerPage;
    const currentUsers = filteredUsers.slice(indexOfFirstUser, indexOfLastUser);
    const totalPages = Math.ceil(filteredUsers.length / usersPerPage);

    const paginate = (pageNumber) => setCurrentPage(pageNumber);
    const nextPage = () => setCurrentPage(prev => Math.min(prev + 1, totalPages));
    const prevPage = () => setCurrentPage(prev => Math.max(prev - 1, 1));
    const handleAddUser = () => navigate('/users/create');
    const handleEditUser = (userId) => navigate(`/users/edit/${userId}`);

    const fetchUsers = async () => {
        try {
            setIsLoading(true);
            const response = await getAllUsers(true, true);
            setUsers(response);
        } catch (error) {
            console.error("Ошибка при загрузке пользователей:", error);
        } finally {
            setIsLoading(false);
        }
    };

    const handleDeleteClick = (userId) => {
        setUserToDelete(userId);
        setShowDeleteModal(true);
        setDeleteError(null);
    };

    const handleConfirmDelete = async () => {
        try {
            setIsLoading(true);
            await deleteUser(userToDelete);
            await fetchUsers();
            setShowDeleteModal(false);
            setUserToDelete(null);

            if (currentUsers.length === 1 && currentPage > 1) {
                setCurrentPage(currentPage - 1);
            }
        } catch (error) {
            console.error("Ошибка при удалении пользователя:", error);
            setDeleteError("Не удалось удалить пользователя. Пожалуйста, попробуйте снова.");
        } finally {
            setIsLoading(false);
        }
    };

    const handleCancelDelete = () => {
        setShowDeleteModal(false);
        setUserToDelete(null);
        setDeleteError(null);
    };

    const handleExportClick = () => {
        setIsExportModalOpen(true);
    };

    const handleExportConfirm = async (filename, description) => {
        try {
            await exportToExcel(0, filename || "Отчет_по_пользователям", description || "");
            setIsExportModalOpen(false);
        } catch (error) {
            console.error('Ошибка при экспорте:', error);
            alert('Произошла ошибка при экспорте данных');
        }
    };

    useEffect(() => {
        fetchUsers();
    }, []);

    return (
        <div className="user-list-page">
            {showDeleteModal && (
                <div className="modal-overlay">
                    <div className="delete-confirmation-modal">
                        <h3>Подтверждение удаления</h3>
                        <p>Вы уверены, что хотите удалить этого пользователя?</p>
                        {deleteError && <p className="error-message">{deleteError}</p>}
                        <div className="modal-actions">
                            <button
                                className="admin-button secondary"
                                onClick={handleCancelDelete}
                                disabled={isLoading}
                            >
                                Отмена
                            </button>
                            <button
                                className="admin-button danger"
                                onClick={handleConfirmDelete}
                                disabled={isLoading}
                            >
                                {isLoading ? 'Удаление...' : 'Удалить'}
                            </button>
                        </div>
                    </div>
                </div>
            )}

            <Header title={"Управление пользователями"} />
            <div className="user-list-header">
                <h1 className="user-list-title"></h1>
                <div className="header-actions">
                    <button
                        className="admin-button primary"
                        onClick={handleAddUser}
                        disabled={isLoading}
                    >
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                            <path d="M12 5v14"></path>
                            <path d="M5 12h14"></path>
                        </svg>
                        Добавить сотрудника
                    </button>
                    <button
                        className="admin-button secondary"
                        onClick={handleExportClick}
                        disabled={isLoading || users.length === 0}
                    >
                        <FileDown size={16} />
                        Экспорт в Excel
                    </button>
                </div>
            </div>

            {isLoading && users.length === 0 ? (
                <div className="loading-state">
                    <div className="loading-spinner"></div>
                    <p>Загрузка пользователей...</p>
                </div>
            ) : (
                <>
                    <div className="user-list-toolbar">
                        <div className="search-container">
                            <input
                                type="text"
                                placeholder="Поиск по имени, email или логину..."
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
                                <option value="all">Все статусы</option>
                                <option value="active">Активные</option>
                                <option value="inactive">Заблокированные</option>
                            </select>

                            <div className="results-count">
                                Найдено: {filteredUsers.length}
                            </div>
                        </div>
                    </div>

                    <div className="user-table-container">
                        <table className="user-table">
                            <thead>
                            <tr>
                                <th>ФИО</th>
                                <th>Логин</th>
                                <th>Email</th>
                                <th>Отдел</th>
                                <th>Статус</th>
                                <th>Роль</th>
                                <th>Действия</th>
                            </tr>
                            </thead>
                            <tbody>
                            {currentUsers.length > 0 ? (
                                currentUsers.map(user => (
                                    <tr key={user.id}>
                                        <td>
                                            <span className="user-name">{user.fullName}</span>
                                        </td>
                                        <td>{user.username}</td>
                                        <td>{user.email}</td>
                                        <td>{user.departmentName}</td>
                                        <td>
                                            <span className={`user-status status-${user.isBlocked ? "inactive" : "active"}`}>
                                                {user.isBlocked ? "Заблокирован" : "Активен"}
                                            </span>
                                        </td>
                                        <td>{user.isAdmin ? "Администратор" : ""}</td>
                                        <td>
                                            <div className="user-actions">
                                                <button
                                                    className="action-button edit"
                                                    title="Редактировать"
                                                    onClick={() => handleEditUser(user.id)}
                                                    disabled={isLoading}
                                                >
                                                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none"
                                                         stroke="currentColor">
                                                        <path
                                                            d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path>
                                                        <path
                                                            d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path>
                                                    </svg>
                                                </button>
                                                <button
                                                    className="action-button delete"
                                                    title="Удалить"
                                                    onClick={() => handleDeleteClick(user.id)}
                                                    disabled={isLoading}
                                                >
                                                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none"
                                                         stroke="currentColor">
                                                        <path d="M3 6h18"></path>
                                                        <path
                                                            d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
                                                    </svg>
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                ))
                            ) : (
                                <tr>
                                    <td colSpan="7" className="empty-state">
                                        <div className="empty-icon">😕</div>
                                        <h3 className="empty-title">Пользователи не найдены</h3>
                                        <p className="empty-description">Попробуйте изменить параметры поиска</p>
                                    </td>
                                </tr>
                            )}
                            </tbody>
                        </table>
                    </div>

                    {filteredUsers.length > usersPerPage && (
                        <div className="pagination-container">
                            <div className="pagination-info">
                                Показано {indexOfFirstUser + 1}-{Math.min(indexOfLastUser, filteredUsers.length)} из {filteredUsers.length}
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
                defaultFilename="Отчет_по_пользователям"
            />
        </div>
    );
};

export default UserListPage;