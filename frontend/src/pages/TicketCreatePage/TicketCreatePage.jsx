import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import "./TicketCreatePage.css";
import Header from "../../components/layout/header/Header";
import { ArrowLeft } from "lucide-react";

const TicketCreatePage = () => {
    //const navigate = useNavigate();
    const [editedTitle, setEditedTitle] = useState('');
    const [editedText, setEditedText] = useState('');
    const [editedDepartment, setEditedDepartment] = useState('it');
    const [isEditable, setIsEditable] = useState(true);

    const handleSave = () => {
        const newTicket = {
            id: Date.now(), // Генерация уникального ID
            title: editedTitle,
            text: editedText,
            department: editedDepartment,
        };

        console.log('Новая заявка:', newTicket);
        alert('Заявка успешно создана');
        //navigate('/tickets');
    };

    const handleCancel = () => {
        //navigate('/tickets');
    };

    return (
        <div className="ticket-details-page">
            <Header title={"Создание новой заявки"} />
            <button className="back-button" onClick={handleCancel}>
                <ArrowLeft size={20} /> Назад
            </button>
            <div className="ticket-details-container">
                <div className="edit-field">
                    <label>Заголовок:</label>
                    <input
                        type="text"
                        value={editedTitle}
                        onChange={(e) => setEditedTitle(e.target.value)}
                    />
                </div>

                <div className="edit-field">
                    <label>Текст заявки:</label>
                    <textarea
                        value={editedText}
                        onChange={(e) => setEditedText(e.target.value)}
                    />
                </div>

                <div className="edit-field">
                    <label className="edit-label">
                        Отдел:
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
                    </label>
                </div>

                <div className="ticket-details-actions">
                    <button className="ticket-save-button" onClick={handleSave}>
                        Создать заявку
                    </button>
                    <button className="ticket-back-button" onClick={handleCancel}>
                        Отмена
                    </button>
                </div>
            </div>
        </div>
    );
};

export default TicketCreatePage;