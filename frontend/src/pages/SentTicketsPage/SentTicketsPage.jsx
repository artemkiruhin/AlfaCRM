import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import "./SentTicketsPage.css";
import Header from "../../components/layout/header/Header";

const SentTicketsPage = () => {
    //const navigate = useNavigate();
    const [tickets, setTickets] = useState([]);
    const [selectedTicket, setSelectedTicket] = useState(null);
    const [feedback, setFeedback] = useState('');
    const [isAdmin, setIsAdmin] = useState(false);
    const [filterEmployeeId, setFilterEmployeeId] = useState('');
    const [filteredTickets, setFilteredTickets] = useState([]);

    const mockTickets = [
        {
            id: 1,
            title: 'Проблема с принтером',
            text: 'Принтер не печатает документы.',
            department: 'IT-отдел',
            date: '2023-10-01 14:30',
            status: 'Создано',
            assignee: null,
            employeeId: '1001',
            feedback: '',
        },
        {
            id: 2,
            title: 'Замена картриджа',
            text: 'Необходимо заменить картридж в принтере.',
            department: 'Администрация',
            date: '2023-10-02 10:15',
            status: 'В работе',
            assignee: 'ivanov',
            employeeId: '1002',
            feedback: '',
        },
        {
            id: 3,
            title: 'Настройка почты',
            text: 'Необходимо настроить почту для нового сотрудника.Необходимо настроить почту для нового сотрудника. Необходимо настроить почту для нового сотрудника.Необходимо настроить почту для нового сотрудника. Необходимо настроить почту для нового сотрудника.Необходимо настроить почту для нового сотрудника.Необходимо настроить почту для нового сотрудника. Необходимо настроить почту для нового сотрудника. Необходимо настроить почту для нового сотрудника.',
            department: 'IT-отдел',
            date: '2023-10-03 09:00',
            status: 'Выполнено',
            assignee: 'petrov',
            employeeId: '1003',
            feedback: 'Почта настроена.',
        },
        {
            id: 4,
            title: 'Настройка почты',
            text: 'Необходимо настроить почту для нового сотрудника.',
            department: 'IT-отдел',
            date: '2023-10-03 09:00',
            status: 'В работе',
            assignee: 'ivanov',
            employeeId: '1001',
            feedback: 'Почта настроена.',
        },
    ];

    useEffect(() => {
        setTickets(mockTickets);
        setFilteredTickets(mockTickets);
        // Проверка, является ли пользователь администратором (в реальном приложении это будет запрос к серверу)
        setIsAdmin(true); // Пример: пользователь — администратор
    }, []);

    // Фильтрация заявок по ID сотрудника
    useEffect(() => {
        if (filterEmployeeId) {
            const filtered = tickets.filter(ticket =>
                ticket.employeeId && ticket.employeeId.includes(filterEmployeeId)
            );
            setFilteredTickets(filtered);
        } else {
            setFilteredTickets(tickets);
        }
    }, [filterEmployeeId, tickets]);

    const handleTakeToWork = (ticketId) => {
        const updatedTickets = tickets.map((ticket) =>
            ticket.id === ticketId ? { ...ticket, status: 'В работе', assignee: 'currentUser' } : ticket
        );
        setTickets(updatedTickets);
        alert(`Заявка ${ticketId} взята в работу`);
    };

    const handleCloseTicket = (ticketId) => {
        if (!feedback) {
            alert('Укажите сообщение-ответ перед закрытием заявки');
            return;
        }
        const updatedTickets = tickets.map((ticket) =>
            ticket.id === ticketId ? { ...ticket, status: 'Закрыто', feedback } : ticket
        );
        setTickets(updatedTickets);
        setSelectedTicket(null);
        setFeedback('');
        alert(`Заявка ${ticketId} закрыта`);
    };

    const handleCompleteTicket = (ticketId) => {
        if (!feedback) {
            alert('Укажите сообщение-ответ перед выполнением заявки');
            return;
        }
        const updatedTickets = tickets.map((ticket) =>
            ticket.id === ticketId ? { ...ticket, status: 'Выполнено', feedback } : ticket
        );
        setTickets(updatedTickets);
        setSelectedTicket(null);
        setFeedback('');
        alert(`Заявка ${ticketId} выполнена`);
    };

    return (
        <div className="sent-tickets-page">
            <Header title={"Отправленные заявками"} />

            <div className="filter-container">
                <input
                    type="text"
                    placeholder="Поиск..."
                    value={filterEmployeeId}
                    onChange={(e) => setFilterEmployeeId(e.target.value)}
                    className="filter-input"
                />
            </div>

            <div className="ticket-list-container">
                <div className="ticket-list">
                    {filteredTickets.length > 0 ? (
                        filteredTickets.map((ticket) => (
                            <div key={ticket.id} className="ticket-item">
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
                                    <span>ID сотрудника: {ticket.employeeId}</span>
                                    {ticket.assignee && <span>Исполнитель: {ticket.assignee}</span>}
                                </div>
                                {ticket.feedback && (
                                    <div className="ticket-feedback">
                                        <strong>Ответ:</strong> {ticket.feedback}
                                    </div>
                                )}
                                <div className="ticket-actions">
                                    {ticket.status === 'Создано' && isAdmin && (
                                        <button
                                            className="ticket-action-button take-to-work"
                                            onClick={() => handleTakeToWork(ticket.id)}
                                        >
                                            Взять в работу
                                        </button>
                                    )}
                                    {(ticket.status === 'В работе' || ticket.status === 'Создано') && isAdmin && (
                                        <button
                                            className="ticket-action-button close-ticket"
                                            onClick={() => setSelectedTicket({...ticket, actionType: 'close'})}
                                        >
                                            Закрыть
                                        </button>
                                    )}
                                    {(ticket.status === 'В работе' || ticket.status === 'Создано') && isAdmin && (
                                        <button
                                            className="ticket-action-button complete-ticket"
                                            onClick={() => setSelectedTicket({...ticket, actionType: 'complete'})}
                                        >
                                            Выполнить
                                        </button>
                                    )}
                                </div>
                            </div>
                        ))
                    ) : (
                        <div className="no-tickets-message">
                            Нет заявок, соответствующих фильтру
                        </div>
                    )}
                </div>
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
                        />
                        <div className="modal-actions">
                            {selectedTicket.actionType === 'close' ? (
                                <button
                                    className="modal-button close"
                                    onClick={() => handleCloseTicket(selectedTicket.id)}
                                >
                                    Закрыть заявку
                                </button>
                            ) : (
                                <button
                                    className="modal-button complete"
                                    onClick={() => handleCompleteTicket(selectedTicket.id)}
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

export default SentTicketsPage;