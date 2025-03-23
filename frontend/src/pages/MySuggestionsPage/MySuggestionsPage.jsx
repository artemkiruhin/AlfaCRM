import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import "./MySuggestionsPage.css";
import Header from "../../components/layout/header/Header";

const MySuggestionsPage = () => {
    const [mySuggestions, setMySuggestions] = useState([]);
    const [selectedSuggestion, setSelectedSuggestion] = useState(null);
    const [feedback, setFeedback] = useState('');
    const [isAdmin, setIsAdmin] = useState(false);
    const [searchQuery, setSearchQuery] = useState('');
    const [filteredSuggestions, setFilteredSuggestions] = useState([]);
    const [showRejected, setShowRejected] = useState(false);

    const mockMySuggestions = [
        {
            id: 1,
            title: 'Организация зоны отдыха',
            text: 'Предлагаю организовать комфортную зону отдыха на 3 этаже рядом с переговорными комнатами.',
            department: 'Администрация',
            date: '2023-11-10 09:45',
            status: 'На рассмотрении',
            assignee: null,
            employeeId: '1001',
            feedback: '',
        },
        {
            id: 2,
            title: 'Внедрение корпоративного мессенджера',
            text: 'Для улучшения коммуникации между отделами предлагаю внедрить единый корпоративный мессенджер с функциями видео-звонков и обмена файлами.',
            department: 'IT-отдел',
            date: '2023-11-05 14:20',
            status: 'Одобрено',
            assignee: 'ivanov',
            employeeId: '1001',
            feedback: 'Отличное предложение! Начинаем реализацию со следующего месяца.',
        },
        {
            id: 3,
            title: 'Установка кофемашины',
            text: 'Предлагаю установить профессиональную кофемашину в общей кухне для повышения комфорта сотрудников.',
            department: 'Администрация',
            date: '2023-10-25 11:30',
            status: 'Отклонено',
            assignee: 'petrov',
            employeeId: '1001',
            feedback: 'К сожалению, в текущем бюджете нет средств на данное приобретение.',
        },
    ];

    useEffect(() => {
        setMySuggestions(mockMySuggestions);
        setFilteredSuggestions(mockMySuggestions);
        setIsAdmin(true); // Пример: пользователь — администратор
    }, []);

    useEffect(() => {
        const filterSuggestions = (suggestions) => {
            return suggestions.filter((suggestion) => {
                const matchesSearch = suggestion.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
                    suggestion.text.toLowerCase().includes(searchQuery.toLowerCase());
                const matchesStatus = showRejected ? true : suggestion.status !== 'Отклонено';
                return matchesSearch && matchesStatus;
            });
        };

        setFilteredSuggestions(filterSuggestions(mySuggestions));
    }, [searchQuery, showRejected, mySuggestions]);

    const handleApproveSuggestion = (suggestionId) => {
        if (!feedback) {
            alert('Необходимо добавить комментарий перед одобрением предложения');
            return;
        }

        const updatedSuggestions = mySuggestions.map((suggestion) =>
            suggestion.id === suggestionId
                ? { ...suggestion, status: 'Одобрено', feedback, assignee: 'currentUser' }
                : suggestion
        );

        setMySuggestions(updatedSuggestions);
        setFilteredSuggestions(updatedSuggestions);
        setSelectedSuggestion(null);
        setFeedback('');
        alert(`Предложение ${suggestionId} одобрено`);
    };

    const handleRejectSuggestion = (suggestionId) => {
        if (!feedback) {
            alert('Необходимо добавить комментарий перед отклонением предложения');
            return;
        }

        const updatedSuggestions = mySuggestions.map((suggestion) =>
            suggestion.id === suggestionId
                ? { ...suggestion, status: 'Отклонено', feedback, assignee: 'currentUser' }
                : suggestion
        );

        setMySuggestions(updatedSuggestions);
        setFilteredSuggestions(updatedSuggestions);
        setSelectedSuggestion(null);
        setFeedback('');
        alert(`Предложение ${suggestionId} отклонено`);
    };

    const renderSuggestionsList = () => (
        <div className="suggestions-list">
            {filteredSuggestions.length > 0 ? (
                filteredSuggestions.map((suggestion) => (
                    <div key={suggestion.id} className="suggestion-item">
                        <div className="suggestion-header">
                            <h3 className="suggestion-title">{suggestion.title}</h3>
                            <span className={`suggestion-status ${suggestion.status.toLowerCase().replace(/\s+/g, '-')}`}>
                                {suggestion.status}
                            </span>
                        </div>
                        <p className="suggestion-text">{suggestion.text}</p>
                        <div className="suggestion-meta">
                            <span>Отдел: {suggestion.department}</span>
                            <span>Дата создания: {suggestion.date}</span>
                            {suggestion.employeeId && <span>ID сотрудника: {suggestion.employeeId}</span>}
                            {suggestion.assignee && <span>Обработал: {suggestion.assignee}</span>}
                        </div>
                        {suggestion.feedback && (
                            <div className="suggestion-feedback">
                                <strong>Комментарий:</strong> {suggestion.feedback}
                            </div>
                        )}
                        <div className="suggestion-actions">
                            {suggestion.status === 'На рассмотрении' && isAdmin && (
                                <>
                                    <button
                                        className="suggestion-action-button approve"
                                        onClick={() => setSelectedSuggestion({...suggestion, actionType: 'approve'})}
                                    >
                                        Одобрить
                                    </button>
                                    <button
                                        className="suggestion-action-button reject"
                                        onClick={() => setSelectedSuggestion({...suggestion, actionType: 'reject'})}
                                    >
                                        Отклонить
                                    </button>
                                </>
                            )}
                        </div>
                    </div>
                ))
            ) : (
                <div className="no-suggestions-message">
                    Нет предложений, соответствующих фильтру
                </div>
            )}
        </div>
    );

    return (
        <div className="suggestions-page">
            <Header title={"Мои предложения"} />

            <div className="filter-container">
                <input
                    type="text"
                    placeholder="Поиск по предложениям..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    className="filter-input"
                />
                <label className="filter-checkbox">
                    <input
                        type="checkbox"
                        checked={showRejected}
                        onChange={() => setShowRejected(!showRejected)}
                    />
                    Показать отклоненные
                </label>
                <button className="create-suggestion-button">
                    Создать предложение
                </button>
            </div>

            <div className="suggestions-list-container">
                {renderSuggestionsList()}
            </div>

            {selectedSuggestion && (
                <div className="modal-overlay">
                    <div className="modal">
                        <h3>
                            {selectedSuggestion.actionType === 'approve' ? 'Одобрение предложения' : 'Отклонение предложения'}: {selectedSuggestion.title}
                        </h3>
                        <div className="suggestion-details">
                            <p><strong>ID предложения:</strong> {selectedSuggestion.id}</p>
                            <p><strong>ID сотрудника:</strong> {selectedSuggestion.employeeId}</p>
                            <p><strong>Описание:</strong> {selectedSuggestion.text}</p>
                        </div>
                        <textarea
                            placeholder="Введите комментарий..."
                            value={feedback}
                            onChange={(e) => setFeedback(e.target.value)}
                            className="feedback-textarea"
                        />
                        <div className="modal-actions">
                            {selectedSuggestion.actionType === 'approve' ? (
                                <button
                                    className="modal-button approve"
                                    onClick={() => handleApproveSuggestion(selectedSuggestion.id)}
                                >
                                    Одобрить предложение
                                </button>
                            ) : (
                                <button
                                    className="modal-button reject"
                                    onClick={() => handleRejectSuggestion(selectedSuggestion.id)}
                                >
                                    Отклонить предложение
                                </button>
                            )}
                            <button
                                className="modal-button cancel"
                                onClick={() => {
                                    setSelectedSuggestion(null);
                                    setFeedback('');
                                }}
                            >
                                Отмена
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default MySuggestionsPage;