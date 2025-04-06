import React, { useState } from 'react';
import './AuthPage.css'; // Обычный CSS файл
import { login } from "../../api-handlers/authHandler";
import { useNavigate } from "react-router-dom";

export const AuthPage = () => {
    const navigate = useNavigate();

    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [message, setMessage] = useState('');
    const [messageType, setMessageType] = useState('');
    const [shake, setShake] = useState(false);

    const handleLogin = () => {
        if (!username || !password) {
            showError('Пожалуйста, заполните все поля');
            return;
        }

        setTimeout(async () => {
            const result = await login(username, password);
            if (result) {
                localStorage.setItem('uid', result.id);
                localStorage.setItem('username', result.username);
                localStorage.setItem('did', result.department);
                localStorage.setItem('spec', result.isSpecDepartment);
                showSuccess('Успешная авторизация!');
                navigate("/news");
            } else {
                showError('Неверное имя пользователя или пароль');
            }
        }, 20);
    };

    const showError = (message) => {
        setMessage(message);
        setMessageType('error-msg');
        setShake(true);
        setTimeout(() => setShake(false), 500);
    };

    const showSuccess = (message) => {
        setMessage(message);
        setMessageType('success-msg');
    };

    const handleKeyPress = (event) => {
        if (event.key === 'Enter') handleLogin();
    };

    return (
        <div className="auth-page">
            <div className={`auth-page__container ${shake ? 'auth-page__shake' : ''}`}>
                <div className="auth-page__header">
                    <h1>Авторизация</h1>
                </div>
                <div className="auth-page__form-group">
                    <label htmlFor="username">Имя пользователя</label>
                    <input
                        type="text"
                        id="username"
                        placeholder="Введите имя пользователя"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                    />
                </div>
                <div className="auth-page__form-group">
                    <label htmlFor="password">Пароль</label>
                    <input
                        type="password"
                        id="password"
                        placeholder="Введите пароль"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        onKeyPress={handleKeyPress}
                    />
                </div>
                <button
                    className="auth-page__btn"
                    onClick={handleLogin}
                >
                    Войти
                </button>
                <div className={`auth-page__message ${messageType}`}>
                    {message}
                </div>
            </div>
        </div>
    );
};