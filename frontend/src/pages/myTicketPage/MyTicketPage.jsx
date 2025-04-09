import React, { useState, useEffect } from 'react';
import TicketList from '../../components/ticket/TicketList';
import CreateTicketButton from '../../components/ticket/CreateTicketButton';
import './MyTicketsPage.css';
import Header from "../../components/layout/header/Header";
import { getUserTickets, deleteTicket } from '../../api-handlers/ticketsHandler';
import {useNavigate} from "react-router-dom";

const MyTicketsPage = () => {
    const navigate = useNavigate();
    const [tickets, setTickets] = useState([]);
    const [loading, setLoading] = useState(true);

    const onCreatePageHandler = () => {
        navigate("/tickets/create");
    }

    useEffect(() => {
        const fetchTickets = async () => {
            try {
                setLoading(true);
                const data = await getUserTickets(false, 0);
                console.log(data);
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

    }, []);

    const handleDelete = async (id) => {
        try {
            await deleteTicket(id);
            setTickets(tickets.filter((ticket) => ticket.id !== id));
        } catch (err) {
            console.error('Failed to delete ticket:', err);
        }
    };

    // if (loading) {
    //     return <LoadingSpinner />;
    // }

    // if (error) {
    //     return <ErrorMessage message={error} />;
    // }

    return (
        <div className="my-tickets-page">
            <Header title={"Мои заявки"} info={`Всего: ${tickets.length}`} />
            <CreateTicketButton onCreatePageHandler={onCreatePageHandler} />
            <TicketList tickets={tickets} onDelete={handleDelete} />
        </div>
    );
};

export default MyTicketsPage;