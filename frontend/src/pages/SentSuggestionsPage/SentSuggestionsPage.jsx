import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import "./SentSuggestionsPage.css";
import Header from "../../components/layout/header/Header";
import TicketList from '../../components/ticket/TicketList';
import {getAllTickets, changeTicketStatus, deleteTicket} from '../../api-handlers/ticketsHandler';
import SentSuggestionList from "../../components/ticket/SentSuggestionList";

const SentSuggestionsPage = () => {
    const navigate = useNavigate();
    const [tickets, setTickets] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [filterEmployeeId, setFilterEmployeeId] = useState('');
    const [filteredTickets, setFilteredTickets] = useState([]);
    const [isAdmin, setIsAdmin] = useState(false);

    useEffect(() => {
        const fetchTickets = async () => {
            try {
                setLoading(true);
                const isAdm = localStorage.getItem('adm') === "true";
                console.log(isAdm);
                const response = await getAllTickets(isAdm ? null : localStorage.getItem('did'), null, 1);

                if (response && Array.isArray(response)) {
                    const formattedTickets = response.map(ticket => ({
                        id: ticket.id,
                        title: ticket.title,
                        text: ticket.text,
                        department: ticket.department?.name || 'Не указан',
                        date: new Date(ticket.createdAt).toLocaleString(),
                        status: ticket.status,
                        feedback: ticket.feedback || '',
                        assignee: ticket.assignee ? ticket.assignee.username : 'Не назначен',
                        employeeId: ticket.creator?.username || 'Неизвестно',
                        closedAt: ticket.closedAt ? new Date(ticket.closedAt).toLocaleString() : null,
                        creator: ticket.creator?.username || 'Неизвестно'
                    }));

                    setTickets(formattedTickets);
                    setFilteredTickets(formattedTickets);
                } else {
                    throw new Error('Некорректный формат данных от сервера');
                }

                setIsAdmin(isAdm)
            } catch (err) {
                console.error('Failed to fetch tickets:', err);
                setError('Не удалось загрузить заявки');
            } finally {
                setLoading(false);
            }
        };

        fetchTickets();
    }, []);

    useEffect(() => {
        if (filterEmployeeId) {
            const filtered = tickets.filter(ticket =>
                ticket.employeeId && ticket.employeeId.toLowerCase().includes(filterEmployeeId.toLowerCase())
            );
            setFilteredTickets(filtered);
        } else {
            setFilteredTickets(tickets);
        }
    }, [filterEmployeeId, tickets]);

    const handleTakeToWork = async (ticketId) => {
        try {
            await changeTicketStatus(ticketId, 1, null);

            const updatedTickets = tickets.map((ticket) =>
                ticket.id === ticketId ? {
                    ...ticket,
                    status: 'В работе',
                    assignee: 'currentUser'
                } : ticket
            );

            setTickets(updatedTickets);
            alert(`Заявка ${ticketId} взята в работу`);
        } catch (err) {
            console.error('Failed to take ticket to work:', err);
            alert('Не удалось взять заявку в работу');
        }
    };

    const handleCloseTicket = async (ticketId, feedback) => {
        try {
            await changeTicketStatus(ticketId, 3, feedback);

            const updatedTickets = tickets.map((ticket) =>
                ticket.id === ticketId ? {
                    ...ticket,
                    status: 'Отменено',
                    feedback,
                    closedAt: new Date().toLocaleString()
                } : ticket
            );

            setTickets(updatedTickets);
            alert(`Заявка ${ticketId} закрыта`);
        } catch (err) {
            console.error('Failed to close ticket:', err);
            alert('Не удалось закрыть заявку');
        }
    };

    const handleCompleteTicket = async (ticketId, feedback) => {
        try {
            await changeTicketStatus(ticketId, 2, feedback);

            const updatedTickets = tickets.map((ticket) =>
                ticket.id === ticketId ? {
                    ...ticket,
                    status: 'Выполнено',
                    feedback,
                    closedAt: new Date().toLocaleString()
                } : ticket
            );

            setTickets(updatedTickets);
            alert(`Заявка ${ticketId} выполнена`);
        } catch (err) {
            console.error('Failed to complete ticket:', err);
            alert('Не удалось выполнить заявку');
        }
    };

    const handleDelete = async (id) => {
        try {
            await deleteTicket(id);
            setTickets(tickets.filter((ticket) => ticket.id !== id));
        } catch (err) {
            console.error('Failed to delete ticket:', err);
        }
    };

    return (
        <div className="sent-tickets-page">
            <Header title={"Отправленные предложения"} info={`Всего: ${filteredTickets.length}`} />

            <div className="filter-container">
                <input
                    type="text"
                    placeholder="Поиск по ID сотрудника..."
                    value={filterEmployeeId}
                    onChange={(e) => setFilterEmployeeId(e.target.value)}
                    className="filter-input"
                />
            </div>

            <SentSuggestionList
                tickets={filteredTickets}
                isAdmin={isAdmin}
                onTakeToWork={handleTakeToWork}
                onClose={handleCloseTicket}
                onComplete={handleCompleteTicket}
                onDelete={handleDelete}
            />
        </div>
    );
};

export default SentSuggestionsPage;