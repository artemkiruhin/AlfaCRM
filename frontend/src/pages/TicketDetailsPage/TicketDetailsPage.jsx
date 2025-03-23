import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import "./TicketDetailsPage.css";
import Header from "../../components/layout/header/Header";
import {ArrowLeft} from "lucide-react";

const TicketDetailsPage = () => {
    //const { id } = useParams();
    const id = 1
    //const navigate = useNavigate();
    const [ticket, setTicket] = useState(null);
    const [editedTitle, setEditedTitle] = useState('');
    const [editedText, setEditedText] = useState('');
    const [editedDepartment, setEditedDepartment] = useState('');
    const [editedFeedback, setEditedFeedback] = useState('');
    const [isEditable, setIsEditable] = useState(false);

    const mockTickets = [
        {
            id: 1,
            title: 'Проблема с принтером',
            text: 'Принтер не печатает документы.',
            department: 'it',
            date: '2023-10-01 14:30',
            status: 'Создано',
            assignee: 'ivanov',
            feedback: '',
        },
        {
            id: 2,
            title: 'Замена картриджа',
            text: 'Необходимо заменить картридж в принтере.',
            department: 'finance',
            date: '2023-10-02 10:15',
            status: 'Выполнено',
            feedback: 'Картридж заменен.',
            assignee: 'petrov',
        },
    ];

    useEffect(() => {
        const foundTicket = mockTickets.find((ticket) => ticket.id === parseInt(id));
        if (foundTicket) {
            setTicket(foundTicket);
            setEditedTitle(foundTicket.title);
            setEditedText(foundTicket.text);
            setEditedDepartment(foundTicket.department);
            setEditedFeedback(foundTicket.feedback);
            // Проверяем, можно ли редактировать заявку (например, если статус "Создано")
            setIsEditable(foundTicket.status === 'Создано');
        } else {
            //navigate('/tickets');
        }
    }, [id]);

    const handleDelete = () => {
        alert(`Заявка ${id} удалена`);
        //navigate('/tickets');
    };

    const handleSave = () => {
        //
        // const updatedTicket = {
        //     ...ticket,
        //     title: editedTitle,
        //     text: editedText,
        //     department: editedDepartment,
        //     feedback: editedFeedback,
        // };
        // setTicket(updatedTicket);
        // alert('Изменения сохранены');
        // setIsEditable(false);
        alert('Изменения сохранены');

    };

    if (!ticket) {
        return <div>Загрузка...</div>;
    }

    return (
        <div className="ticket-details-page">
            <Header title={"Детали заявки"}/>
            <button className="back-button">
                <ArrowLeft size={20}/> Назад
            </button>
            <div className="ticket-details-container">
                <div className="edit-field">
                    <label>Заголовок:</label>
                    {isEditable ? (
                        <input
                            type="text"
                            value={editedTitle}
                            onChange={(e) => setEditedTitle(e.target.value)}
                        />
                    ) : (
                        <h2 className="ticket-details-title">{ticket.title}</h2>
                    )}
                </div>

                <div className="edit-field">
                    <label>Текст заявки:</label>
                    {isEditable ? (
                        <textarea
                            value={editedText}
                            onChange={(e) => setEditedText(e.target.value)}
                        />
                    ) : (
                        <p className="ticket-details-text">{ticket.text}</p>
                    )}
                </div>

                <div className="edit-field">
                    <label className="edit-label">
                        Отдел:
                        {isEditable ? (
                            <select
                                value={editedDepartment}
                                onChange={(e) => setEditedDepartment(e.target.value)}
                                className="edit-select"
                            >
                                <option value="hr">HR</option>
                                <option value="it">IT</option>
                                <option value="finance">Финансы</option>
                                <option value="marketing">Маркетинг</option>
                            </select>
                        ) : (
                            <span>{ticket.department}</span>
                        )}
                    </label>
                </div>

                {(ticket.status === 'Выполнено' || ticket.status === 'Закрыто') && (
                    <div className="edit-field">
                        <label>Ответ сотрудника:</label>
                        {isEditable ? (
                            <textarea
                                value={editedFeedback}
                                onChange={(e) => setEditedFeedback(e.target.value)}
                            />
                        ) : (
                            <p>{ticket.feedback}</p>
                        )}
                    </div>
                )}

                <div className="ticket-details-meta">
                    <span>Дата создания: {ticket.date}</span>
                    <span>Статус: {ticket.status}</span>
                    {ticket.assignee && <span>Сотрудник: {ticket.assignee}</span>}
                </div>

                {/* Кнопки действий */}
                <div className="ticket-details-actions">
                    {isEditable && (
                        <button className="ticket-save-button" onClick={handleSave}>
                            Сохранить
                        </button>
                    )}
                    <button className="ticket-delete-button" onClick={handleDelete}>
                        Удалить
                    </button>
                    {/*<div className="ticket-back-button" onClick={() => navigate('/tickets')}>*/}
                    <div className="ticket-back-button">
                        Назад к списку
                    </div>
                </div>
            </div>
        </div>
    );
};

export default TicketDetailsPage;