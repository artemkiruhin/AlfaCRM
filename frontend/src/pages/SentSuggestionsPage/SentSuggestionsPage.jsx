import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import "./SentSuggestionsPage.css";
import Header from "../../components/layout/header/Header";
import SentSuggestionList from "../../components/ticket/SentSuggestionList";
import { getAllTickets, changeTicketStatus, deleteTicket } from '../../api-handlers/ticketsHandler';

const SentSuggestionsPage = () => {
    const navigate = useNavigate();
    const [suggestions, setSuggestions] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [filterEmployeeId, setFilterEmployeeId] = useState('');
    const [filteredSuggestions, setFilteredSuggestions] = useState([]);
    const [isAdmin, setIsAdmin] = useState(false);
    const [selectedSuggestion, setSelectedSuggestion] = useState(null);
    const [feedback, setFeedback] = useState('');
    const [showRejected, setShowRejected] = useState(false);

    useEffect(() => {
        const fetchSuggestions = async () => {
            try {
                setLoading(true);
                const isAdm = localStorage.getItem('adm') === "true";
                // Используем type=1 для получения предложений
                const response = await getAllTickets(isAdm ? null : localStorage.getItem('did'), null, 1);

                if (response && Array.isArray(response)) {
                    const formattedSuggestions = response.map(suggestion => ({
                        id: suggestion.id,
                        title: suggestion.title,
                        text: suggestion.text,
                        department: suggestion.department?.name || 'Не указан',
                        date: new Date(suggestion.createdAt).toLocaleString(),
                        status: suggestion.status,
                        feedback: suggestion.feedback || '',
                        assignee: suggestion.assignee ? suggestion.assignee.username : 'Не назначен',
                        employeeId: suggestion.creator?.username || 'Неизвестно',
                        closedAt: suggestion.closedAt ? new Date(suggestion.closedAt).toLocaleString() : null,
                        creator: suggestion.creator?.username || 'Неизвестно'
                    }));

                    setSuggestions(formattedSuggestions);
                    setFilteredSuggestions(formattedSuggestions);
                } else {
                    throw new Error('Некорректный формат данных от сервера');
                }

                setIsAdmin(isAdm);
            } catch (err) {
                console.error('Failed to fetch suggestions:', err);
                setError('Не удалось загрузить предложения');
            } finally {
                setLoading(false);
            }
        };

        fetchSuggestions();
    }, []);

    useEffect(() => {
        const filtered = suggestions.filter(suggestion => {
            const matchesEmployee = filterEmployeeId
                ? suggestion.employeeId && suggestion.employeeId.toLowerCase().includes(filterEmployeeId.toLowerCase())
                : true;
            const matchesStatus = showRejected ? true : suggestion.status !== 'Отклонено';
            return matchesEmployee && matchesStatus;
        });
        setFilteredSuggestions(filtered);
    }, [filterEmployeeId, showRejected, suggestions]);

    const handleApproveSuggestion = async (suggestionId) => {
        if (!feedback) {
            alert('Необходимо добавить комментарий перед одобрением предложения');
            return;
        }

        try {
            // Используем статус 2 (Выполнено) для одобренных предложений
            await changeTicketStatus(suggestionId, 2, feedback);

            const updatedSuggestions = suggestions.map((suggestion) =>
                suggestion.id === suggestionId ? {
                    ...suggestion,
                    status: 'Одобрено',
                    feedback,
                    assignee: localStorage.getItem('username') || 'currentUser',
                    closedAt: new Date().toLocaleString()
                } : suggestion
            );

            setSuggestions(updatedSuggestions);
            setSelectedSuggestion(null);
            setFeedback('');
            alert(`Предложение ${suggestionId} одобрено`);
        } catch (err) {
            console.error('Failed to approve suggestion:', err);
            alert('Не удалось одобрить предложение');
        }
    };

    const handleRejectSuggestion = async (suggestionId) => {
        if (!feedback) {
            alert('Необходимо добавить комментарий перед отклонением предложения');
            return;
        }

        try {
            // Используем статус 3 (Отменено) для отклоненных предложений
            await changeTicketStatus(suggestionId, 3, feedback);

            const updatedSuggestions = suggestions.map((suggestion) =>
                suggestion.id === suggestionId ? {
                    ...suggestion,
                    status: 'Отклонено',
                    feedback,
                    assignee: localStorage.getItem('username') || 'currentUser',
                    closedAt: new Date().toLocaleString()
                } : suggestion
            );

            setSuggestions(updatedSuggestions);
            setSelectedSuggestion(null);
            setFeedback('');
            alert(`Предложение ${suggestionId} отклонено`);
        } catch (err) {
            console.error('Failed to reject suggestion:', err);
            alert('Не удалось отклонить предложение');
        }
    };

    const handleDelete = async (id) => {
        try {
            await deleteTicket(id);
            setSuggestions(suggestions.filter((suggestion) => suggestion.id !== id));
        } catch (err) {
            console.error('Failed to delete suggestion:', err);
        }
    };

    return (
        <div className="sent-suggestions-page">
            <Header title={"Отправленные предложения"} info={`Всего: ${filteredSuggestions.length}`} />

            <div className="filter-container">
                <input
                    type="text"
                    placeholder="Поиск по ID сотрудника..."
                    value={filterEmployeeId}
                    onChange={(e) => setFilterEmployeeId(e.target.value)}
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

            <SentSuggestionList
                suggestions={filteredSuggestions}
                isAdmin={isAdmin}
                onApprove={(suggestion) => setSelectedSuggestion({...suggestion, actionType: 'approve'})}
                onReject={(suggestion) => setSelectedSuggestion({...suggestion, actionType: 'reject'})}
                onDelete={handleDelete}
            />

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
                            required
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