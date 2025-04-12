import React, { useState, useEffect } from 'react';
import TicketList from '../../components/ticket/TicketList';
import CreateTicketButton from '../../components/ticket/CreateTicketButton';
import './MyTicketsPage.css';
import Header from "../../components/layout/header/Header";
import { getUserTickets, deleteTicket } from '../../api-handlers/ticketsHandler';
import { useNavigate, useLocation } from "react-router-dom";

const MyTicketsPage = ({ type }) => {
    const navigate = useNavigate();
    const location = useLocation();
    const [tickets, setTickets] = useState([]);
    const [loading, setLoading] = useState(true);

    const onCreatePageHandler = () => {
        if (type === 0) navigate("/tickets/create");
        else if (type === 1) navigate("/suggestions/create");
    }

    useEffect(() => {
        const fetchTickets = async () => {
            try {
                setLoading(true);
                const data = await getUserTickets(false, type);
                setTickets(data.map(ticket => ({
                    id: ticket.id,
                    title: ticket.title,
                    text: ticket.text,
                    department: ticket.department.name,
                    date: new Date(ticket.createdAt).toLocaleString(),
                    status: ticket.status,
                    feedback: ticket.feedback,
                    assignee: ticket.assignee ? ticket.assignee.username : 'Не назначен',
                    closedAt: ticket.closedAt ? new Date(ticket.closedAt).toLocaleString() : null,
                    creator: ticket.creator.username
                })));
            } catch (err) {
                console.error('Failed to fetch tickets:', err);
            } finally {
                setLoading(false);
            }
        };

        fetchTickets();
    }, [type, location.key]);

    const handleDelete = async (id) => {
        try {
            await deleteTicket(id);
            setTickets(tickets.filter((ticket) => ticket.id !== id));
        } catch (err) {
            console.error('Failed to delete ticket:', err);
        }
    };

    return (
        <div className="my-tickets-page">
            <Header title={type === 0 ? "Мои заявки" : "Мои предложения"} info={`Всего: ${tickets.length}`} />
            <CreateTicketButton type={type} onCreatePageHandler={onCreatePageHandler} />
            <TicketList type={type} tickets={tickets} onDelete={handleDelete} />
        </div>
    );
};

export default MyTicketsPage;