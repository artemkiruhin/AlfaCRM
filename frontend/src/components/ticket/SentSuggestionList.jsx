import React, { useState } from 'react';

const SentSuggestionList = ({
                            tickets,
                            isAdmin = false,
                            onTakeToWork,
                            onClose,
                            onComplete,
                            onDelete
                        }) => {
    const [searchQuery, setSearchQuery] = useState('');
    const [showClosed, setShowClosed] = useState(false);
    const [selectedTicket, setSelectedTicket] = useState(null);
    const [feedback, setFeedback] = useState('');

    const filteredTickets = tickets.filter((ticket) => {
        const matchesSearch = ticket.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
            ticket.text.toLowerCase().includes(searchQuery.toLowerCase()) ||
            (ticket.employeeId && ticket.employeeId.toLowerCase().includes(searchQuery.toLowerCase()));
        const matchesStatus = showClosed ? true : ticket.status !== 'Отменено' && ticket.status !== 'Выполнено';
        return matchesSearch && matchesStatus;
    });

    const handleClose = (ticket) => {
        if (!feedback) {
            alert('Укажите сообщение-ответ перед закрытием заявки');
            return;
        }
        onClose(ticket.id, feedback);
        setSelectedTicket(null);
        setFeedback('');
    };

    const handleComplete = (ticket) => {
        if (!feedback) {
            alert('Укажите сообщение-ответ перед выполнением заявки');
            return;
        }
        onComplete(ticket.id, feedback);
        setSelectedTicket(null);
        setFeedback('');
    };

    const TicketItem = ({ ticket }) => {
        return (
            <div className="ticket-item">
                <div className="ticket-header">
                    <h3 className="ticket-title">{ticket.title}</h3>
                    <span className={`ticket-status ${ticket.status.toLowerCase()}`}>
                        {ticket.status}
                    </span>
                </div>
                <p className="ticket-text">{ticket.text}</p>
                <div className="ticket-meta">
                    <span>Отдел: {ticket.department}</span>
                    <span>Дата создания: {ticket.date}</span>
                    {ticket.employeeId && <span>ID сотрудника: {ticket.employeeId}</span>}
                    {ticket.assignee && <span>Исполнитель: {ticket.assignee}</span>}
                    {ticket.closedAt && <span>Дата закрытия: {ticket.closedAt}</span>}
                </div>
                {ticket.feedback && (
                    <div className="ticket-feedback">
                        <strong>Ответ:</strong> {ticket.feedback}
                    </div>
                )}
                {isAdmin && (
                    <div className="ticket-actions">
                        <button
                            className="ticket-action-button delete"
                            onClick={() => {
                                if (window.confirm('Вы уверены, что хотите удалить эту заявку?')) {
                                    onDelete(ticket.id);
                                }
                            }}
                        >
                            Удалить
                        </button>

                        {/* For "Created" status */}
                        {ticket.status === 'Создано' && (
                            <>
                                <button
                                    className="ticket-action-button take-to-work"
                                    onClick={() => onTakeToWork(ticket.id)}
                                >
                                    Взять в работу
                                </button>
                                <button
                                    className="ticket-action-button complete-ticket"
                                    onClick={() => setSelectedTicket({...ticket, actionType: 'complete'})}
                                >
                                    Выполнить
                                </button>
                                <button
                                    className="ticket-action-button close-ticket"
                                    onClick={() => setSelectedTicket({...ticket, actionType: 'close'})}
                                >
                                    Закрыть
                                </button>
                            </>
                        )}

                        {/* For "In Work" status */}
                        {ticket.status === 'В работе' && (
                            <>
                                <button
                                    className="ticket-action-button complete-ticket"
                                    onClick={() => setSelectedTicket({...ticket, actionType: 'complete'})}
                                >
                                    Выполнить
                                </button>
                                <button
                                    className="ticket-action-button close-ticket"
                                    onClick={() => setSelectedTicket({...ticket, actionType: 'close'})}
                                >
                                    Закрыть
                                </button>
                            </>
                        )}
                    </div>
                )}
            </div>
        );
    };

    return (
        <div className="ticket-list-container">
            <div className="ticket-list-controls">
                <input
                    type="text"
                    placeholder="Поиск по заявкам..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    className="ticket-search-input"
                />
                <label className="ticket-filter-checkbox">
                    <input
                        type="checkbox"
                        checked={showClosed}
                        onChange={() => setShowClosed(!showClosed)}
                    />
                    Показать закрытые заявки
                </label>
            </div>

            <div className="ticket-list">
                {filteredTickets.length > 0 ? (
                    filteredTickets.map((ticket) => (
                        <TicketItem
                            key={ticket.id}
                            ticket={ticket}
                        />
                    ))
                ) : (
                    <div className="no-tickets-message">
                        Нет заявок, соответствующих критериям поиска
                    </div>
                )}
            </div>

            {selectedTicket && (
                <div className="modal-overlay">
                    <div className="modal">
                        <h3>{selectedTicket.actionType === 'close' ? 'Закрытие заявки' : 'Выполнение заявки'}: {selectedTicket.title}</h3>
                        <div className="ticket-details">
                            <p><strong>ID заявки:</strong> {selectedTicket.id}</p>
                            <p><strong>ID сотрудника:</strong> {selectedTicket.employeeId}</p>
                            <p><strong>Описание:</strong> {selectedTicket.text}</p>
                        </div>
                        <textarea
                            placeholder="Введите сообщение-ответ..."
                            value={feedback}
                            onChange={(e) => setFeedback(e.target.value)}
                            className="feedback-textarea"
                            required
                        />
                        <div className="modal-actions">
                            {selectedTicket.actionType === 'close' ? (
                                <button
                                    className="modal-button close"
                                    onClick={() => handleClose(selectedTicket)}
                                >
                                    Закрыть заявку
                                </button>
                            ) : (
                                <button
                                    className="modal-button complete"
                                    onClick={() => handleComplete(selectedTicket)}
                                >
                                    Выполнить заявку
                                </button>
                            )}
                            <button
                                className="modal-button cancel"
                                onClick={() => {
                                    setSelectedTicket(null);
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

export default SentSuggestionList;