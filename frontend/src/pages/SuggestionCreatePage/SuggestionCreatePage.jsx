import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import "./SuggestionCreatePage.css"; // Используем те же стили
import Header from "../../components/layout/header/Header";
import {ArrowLeft} from "lucide-react";

const SuggestionCreatePage = () => {
    //const navigate = useNavigate();
    const [title, setTitle] = useState('');
    const [text, setText] = useState('');
    const [department, setDepartment] = useState('Администрация');
    const [error, setError] = useState('');

    const handleSubmit = (e) => {
        e.preventDefault();

        if (!title || !text) {
            setError('Заполните все обязательные поля');
            return;
        }

        const newSuggestion = {
            id: Date.now(),
            title,
            text,
            department,
            date: new Date().toLocaleString(),
            status: 'На рассмотрении',
            assignee: null,
            employeeId: '1001',
            feedback: '',
        };

        console.log('Новое предложение:', newSuggestion);
        alert('Предложение успешно создано');
        //navigate('/my-suggestions');
    };

    return (
        <div className="suggestions-page">
            <Header title={"Создание нового предложения"}/>
            <button className="back-button">
                <ArrowLeft size={20}/> Назад
            </button>
            <div className="suggestions-list-container">
                <form onSubmit={handleSubmit} className="create-suggestion-form">
                    <div className="form-field">
                        <label>Заголовок:</label>
                        <input
                            type="text"
                            value={title}
                            onChange={(e) => setTitle(e.target.value)}
                            placeholder="Введите заголовок предложения"
                        />
                    </div>

                    <div className="form-field">
                        <label>Текст предложения:</label>
                        <textarea
                            value={text}
                            onChange={(e) => setText(e.target.value)}
                            placeholder="Опишите ваше предложение"
                        />
                    </div>

                    <div className="form-field">
                        <label>Отдел:</label>
                        <select
                            value={department}
                            onChange={(e) => setDepartment(e.target.value)}
                        >
                            <option value="Администрация">Администрация</option>
                            <option value="IT-отдел">IT-отдел</option>
                            <option value="Финансы">Финансы</option>
                            <option value="Маркетинг">Маркетинг</option>
                        </select>
                    </div>

                    {error && <div className="error-message">{error}</div>}

                    <div className="form-actions">
                        <button type="submit" className="create-suggestion-button">
                            Создать предложение
                        </button>
                        <button
                            type="button"
                            className="cancel-button"
                            //onClick={() => navigate('/my-suggestions')}
                        >
                            Отмена
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default SuggestionCreatePage;