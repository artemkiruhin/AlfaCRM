import React, { useState } from 'react';
import './UserListPage.css';
import GradientCircle from "../../components/extensions/GradientCircle";
import Header from "../../components/layout/header/Header";

const UserListPage = () => {
    const allUsers = [
        { id: 1, username: "ivanov", name: 'Иван Иванов', email: 'ivan@example.com', department: 'Разработка', status: 'active', role: 'Администратор' },
        { id: 2, username: "petrov", name: 'Петр Петров', email: 'petr@example.com', department: 'Дизайн', status: 'active', role: 'Менеджер' },
        { id: 3, username: "sidorov", name: 'Сидор Сидоров', email: 'sidor@example.com', department: 'Маркетинг', status: 'inactive', role: 'Сотрудник' },
        { id: 4, username: "annova", name: 'Анна Аннова', email: 'anna@example.com', department: 'Разработка', status: 'pending', role: 'Сотрудник' },
        { id: 5, username: "smirnov", name: 'Алексей Смирнов', email: 'alex@example.com', department: 'Тестирование', status: 'active', role: 'Сотрудник' },
        { id: 6, username: "kuznets", name: 'Елена Кузнецова', email: 'elena@example.com', department: 'Дизайн', status: 'active', role: 'Менеджер' },
        { id: 7, username: "popova", name: 'Ольга Попова', email: 'olga@example.com', department: 'HR', status: 'active', role: 'HR' },
        { id: 8, username: "volkov", name: 'Дмитрий Волков', email: 'dmitry@example.com', department: 'Разработка', status: 'inactive', role: 'Сотрудник' },
        { id: 9, username: "kozlov", name: 'Сергей Козлов', email: 'sergey@example.com', department: 'Маркетинг', status: 'active', role: 'Сотрудник' },
        { id: 10, username: "novikov", name: 'Андрей Новиков', email: 'andrey@example.com', department: 'Разработка', status: 'active', role: 'Сотрудник' },
        { id: 11, username: "morozov", name: 'Ирина Морозова', email: 'irina@example.com', department: 'Дизайн', status: 'pending', role: 'Сотрудник' },
        { id: 12, username: "pavlova", name: 'Мария Павлова', email: 'maria@example.com', department: 'Тестирование', status: 'active', role: 'Сотрудник' },
    ];

    const [users] = useState(allUsers);
    const [searchTerm, setSearchTerm] = useState('');
    const [filter, setFilter] = useState('all');
    const [currentPage, setCurrentPage] = useState(1);
    const usersPerPage = 10;

    const filteredUsers = users.filter(user => {
        const matchesSearch = user.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
            user.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
            user.username.toLowerCase().includes(searchTerm.toLowerCase());
        const matchesFilter = filter === 'all' || user.status === filter;
        return matchesSearch && matchesFilter;
    });

    const indexOfLastUser = currentPage * usersPerPage;
    const indexOfFirstUser = indexOfLastUser - usersPerPage;
    const currentUsers = filteredUsers.slice(indexOfFirstUser, indexOfLastUser);
    const totalPages = Math.ceil(filteredUsers.length / usersPerPage);

    const paginate = (pageNumber) => setCurrentPage(pageNumber);
    const nextPage = () => setCurrentPage(prev => Math.min(prev + 1, totalPages));
    const prevPage = () => setCurrentPage(prev => Math.max(prev - 1, 1));

    return (
        <div className="user-list-page">
                <Header title={"Управление пользователями"} />
            <div className="user-list-header">
                <h1 className="user-list-title"></h1>
                <div className="header-actions">
                    <button
                        className="admin-button primary"
                        // onClick={handleAddUser}
                    >
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                            <path d="M12 5v14"></path>
                            <path d="M5 12h14"></path>
                        </svg>
                        Добавить сотрудника
                    </button>
                </div>
            </div>

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
                                    <span className="user-name">{user.name}</span>
                                    {/*<div className="user-avatar">*/}
                                    {/*    /!*<GradientCircle name={user.name} size={35} />*!/*/}
                                    {/*</div>*/}
                                </td>
                                <td>{user.username}</td>
                                <td>{user.email}</td>
                                <td>{user.department}</td>
                                <td>
                                        <span className={`user-status status-${user.status}`}>
                                            {user.status === 'active' && 'Активен'}
                                            {user.status === 'inactive' && 'Неактивен'}
                                            {user.status === 'pending' && 'Ожидает'}
                                        </span>
                                </td>
                                <td>{user.role}</td>
                                <td>
                                    <div className="user-actions">
                                        <button
                                            className="action-button edit"
                                            title="Редактировать"
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
                            disabled={currentPage === 1}
                            className={`pagination-button ${currentPage === 1 ? 'disabled' : ''}`}
                        >
                            ←
                        </button>

                        {Array.from({length: Math.min(5, totalPages)}, (_, i) => {
                            // Показываем только 5 страниц вокруг текущей
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
                            >
                                {totalPages}
                            </button>
                        )}

                        <button
                            onClick={nextPage}
                            disabled={currentPage === totalPages}
                            className={`pagination-button ${currentPage === totalPages ? 'disabled' : ''}`}
                        >
                            →
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
};

export default UserListPage;