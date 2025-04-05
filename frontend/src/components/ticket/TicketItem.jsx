import React from 'react';
import {useNavigate} from "react-router-dom";

const TicketItem = ({ ticket, onDelete }) => {
    const navigate = useNavigate();

    const getStatusColor = (status) => {
        switch (status) {
            case 'Создано':
                return 'var(--info-color)';
            case 'В работе':
                return 'var(--warning-color)';
            case 'Отменено':
                return 'var(--error-color)';
            case 'Выполнено':
                return 'var(--success-color)';
            default:
                return 'var(--text-light)';
        }
    };

    const canDelete = ticket.status === 'Создано';

    return (
        <div className="ticket-item">
            <div className="ticket-header">
                <h3 className="ticket-title">{ticket.title}</h3>
                <span
                    className="ticket-status"
                    style={{ backgroundColor: getStatusColor(ticket.status) }}
                >
                    {ticket.status}
                </span>
            </div>
            <p className="ticket-text">{ticket.text}</p>
            <div className="ticket-meta">
                <span className="ticket-department">{ticket.department}</span>
                <span className="ticket-date">
                    {ticket.status === 'Выполнено' || ticket.status === 'Отменено' ?
                        `Закрыто: ${ticket.closedAt}` :
                        `Создано: ${ticket.date}`}
                </span>
            </div>
            {(ticket.status === 'Выполнено' || ticket.status === 'Отменено') && ticket.feedback && (
                <div className="ticket-feedback">
                    <p><strong>Комментарий:</strong> {ticket.feedback}</p>
                    <span className="ticket-assignee">Исполнитель: {ticket.assignee}</span>
                </div>
            )}
            <div className="ticket-actions">
                <div className="ticket-details-button" onClick={() => navigate(`/tickets/my/${ticket.id}`)}>
                    Подробнее
                </div>
                {canDelete && (
                    <button
                        className="ticket-delete-button"
                        onClick={() => onDelete(ticket.id)}
                    >
                        Удалить
                    </button>
                )}
            </div>
        </div>
    );
};

export default TicketItem;