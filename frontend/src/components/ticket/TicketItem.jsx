import React from 'react';
import { Link } from 'react-router-dom';

const TicketItem = ({ ticket, onDelete }) => {
    const getStatusColor = (status) => {
        switch (status) {
            case 'Создано':
                return 'var(--info-color)';
            case 'В работе':
                return 'var(--warning-color)';
            case 'Закрыто':
                return 'var(--error-color)';
            case 'Выполнено':
                return 'var(--success-color)';
            default:
                return 'var(--text-light)';
        }
    };

    return (
        <div className="ticket-item">
            <div className="ticket-header">
                <h3 className="ticket-title">{ticket.title}</h3>
                <span className="ticket-status" style={{ backgroundColor: getStatusColor(ticket.status) }}>
                    {ticket.status}
                </span>
            </div>
            <p className="ticket-text">{ticket.text}</p>
            <div className="ticket-meta">
                <span className="ticket-department">{ticket.department}</span>
                <span className="ticket-date">{ticket.date}</span>
            </div>
            {(ticket.status === 'Выполнено' || ticket.status === 'Закрыто') && (
                <div className="ticket-feedback">
                    <p>{ticket.feedback}</p>
                    <span className="ticket-assignee">Сотрудник: {ticket.assignee}</span>
                </div>
            )}
            <div className="ticket-actions">
                {/*<Link to={`/tickets/${ticket.id}`} className="ticket-details-button">*/}
                {/*    Подробнее*/}
                {/*</Link>*/}
                <div className="ticket-details-button">
                    Подробнее
                </div>
                <button className="ticket-delete-button" onClick={() => onDelete(ticket.id)}>
                    Удалить
                </button>
            </div>
        </div>
    );
};

export default TicketItem;