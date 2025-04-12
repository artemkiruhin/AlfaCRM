import React, { useState } from 'react';
import TicketItem from './TicketItem';

const TicketList = ({type, tickets, onDelete }) => {
    const [searchQuery, setSearchQuery] = useState('');
    const [showClosed, setShowClosed] = useState(false);

    const filteredTickets = tickets.filter((ticket) => {
        const matchesSearch = ticket.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
            ticket.text.toLowerCase().includes(searchQuery.toLowerCase());
        const matchesStatus = showClosed ? true : ticket.status !== 'Закрыто';
        return matchesSearch && matchesStatus;
    });

    return (
        <div className="ticket-list-container">
            <div className="ticket-list-controls">
                <input
                    type="text"
                    placeholder={type === 0 ? "Поиск по заявкам..." : "Поиск по предложениям"}
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
                    {type === 0 ? "Отобразить закрытые заявки" : "Отобразить закрытые предложения"}
                </label>
            </div>
            <div className="ticket-list">
                {filteredTickets.map((ticket) => (
                    <TicketItem key={ticket.id} ticket={ticket} onDelete={onDelete} />
                ))}
            </div>
        </div>
    );
};

export default TicketList;