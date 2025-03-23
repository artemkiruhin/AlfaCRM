import React, { useState } from 'react';
import TicketList from '../../components/ticket/TicketList';
import CreateTicketButton from '../../components/ticket/CreateTicketButton';
import './MyTicketsPage.css';
import Header from "../../components/layout/header/Header";

const MyTicketPage = () => {
    const [tickets, setTickets] = useState([
        {
            id: 1,
            title: 'Проблема с принтером',
            text: 'Принтер не печатает документы.',
            department: 'IT-отдел',
            date: '2023-10-01 14:30',
            status: 'В работе',
            assignee: 'ivanov',
        },
        {
            id: 2,
            title: 'Замена картриджа',
            text: 'Необходимо заменить картридж в принтере.',
            department: 'Администрация',
            date: '2023-10-02 10:15',
            status: 'Выполнено',
            feedback: 'Картридж заменен.',
            assignee: 'petrov',
        },
    ]);

    const handleDelete = (id) => {
        setTickets(tickets.filter((ticket) => ticket.id !== id));
    };

    return (
        <div className="my-tickets-page">
            <Header title={"Мои заявки"} info={`Всего: ${tickets.length}`} />
            <CreateTicketButton />
            <TicketList tickets={tickets} onDelete={handleDelete} />
        </div>
    );
};

export default MyTicketPage;