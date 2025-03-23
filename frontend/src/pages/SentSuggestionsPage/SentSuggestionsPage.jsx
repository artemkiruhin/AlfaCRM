import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import "./SentSuggestionsPage.css";
import Header from "../../components/layout/header/Header";

const SentSuggestionsPage = () => {
    const [sentSuggestions, setSentSuggestions] = useState([]);
    const [selectedSuggestion, setSelectedSuggestion] = useState(null);
    const [feedback, setFeedback] = useState('');
    const [isAdmin, setIsAdmin] = useState(false);
    const [searchQuery, setSearchQuery] = useState('');
    const [filteredSuggestions, setFilteredSuggestions] = useState([]);
    const [showRejected, setShowRejected] = useState(false);

    const mockSentSuggestions = [
        {
            id: 4,
            title: 'Обновление программного обеспечения',
            text: 'Необходимо обновить ПО для бухгалтерии до последней версии.',
            department: 'IT-отдел',
            date: '2023-11-12 10:00',
            status: 'На рассмотрении',
            assignee: null,
            employeeId: '1002',
            feedback: '',
        },
        {
            id: 5,
            title: 'Внедрение системы электронного документооборота',
            text: 'Предлагаю внедрить систему электронного документооборота для ускорения согласования документов.',
            department: 'Администрация',
            date: '2023-11-08 15:30',
            status: 'Одобрено',
            assignee: 'ivanov',
            employeeId: '1003',
            feedback: 'Согласовано. Выделен бюджет на реализацию в первом квартале следующего года.',
        },
        {
            id: 6,
            title: 'Улучшение освещения в офисе',
            text: 'Предлагаю заменить существующие лампы на светодиодные с регулируемой яркостью для создания более комфортной рабочей атмосферы.',
            department: 'Администрация',
            date: '2023-10-30 13:15',
            status: 'Отклонено',
            assignee: 'petrov',
            employeeId: '1004',
            feedback: 'Данный вопрос будет рассмотрен при следующем ремонте офиса.',
        },
        {
            id: 7,
            title: 'Еженедельные тренинги по повышению квалификации',
            text: 'Предлагаю организовать еженедельные внутренние тренинги, где сотрудники могут делиться опытом и знаниями.',
            department: 'HR-отдел',
            date: '2023-11-01 09:20',
            status: 'Одобрено',
            assignee: 'smirnova',
            employeeId: '1002',
            feedback: 'Отличная инициатива! Начинаем с декабря.',
        },
    ];

    useEffect(() => {
        setSentSuggestions(mockSentSuggestions);
        setFilteredSuggestions(mockSentSuggestions);
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

        setFilteredSuggestions(filterSuggestions(sentSuggestions));
    }, [searchQuery, showRejected, sentSuggestions]);

    const handleApproveSuggestion = (suggestionId) => {
        if (!feedback) {
            alert('Необходимо добавить комментарий перед одобрением предложения');
            return;
        }

        const updatedSuggestions = sentSuggestions.map((suggestion) =>
            suggestion.id === suggestionId
                ? { ...suggestion, status: 'Одобрено', feedback, assignee: 'currentUser' }
                : suggestion
        );

        setSentSuggestions(updatedSuggestions);
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

        const updatedSuggestions = sentSuggestions.map((suggestion) =>
            suggestion.id === suggestionId
                ? { ...suggestion, status: 'Отклонено', feedback, assignee: 'currentUser' }
                : suggestion
        );

        setSentSuggestions(updatedSuggestions);
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
            <Header title={"Отправленные предложения"} />

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

export default SentSuggestionsPage;