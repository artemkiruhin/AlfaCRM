import React, { useState } from 'react';
import SuggestionItem from './SuggestionItem';

const SuggestionList = ({ tickets, onDelete }) => {
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
                    Отобразить закрытые заявки
                </label>
            </div>
            <div className="ticket-list">
                {filteredTickets.map((ticket) => (
                    <SuggestionItem key={ticket.id} ticket={ticket} onDelete={onDelete} />
                ))}
            </div>
        </div>
    );
};

export default SuggestionList;