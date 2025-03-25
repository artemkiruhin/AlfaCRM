import React, { useState, useEffect } from 'react';
import './AuthPage.css';
import {login} from "../../api-handlers/authHandler";
import {useNavigate} from "react-router-dom";

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
                console.log(result);
                localStorage.setItem('uid', result.id);
                showSuccess('Успешная авторизация!');
                navigate("/news")
            } else {
                showError('Неверное имя пользователя или пароль');
            }
        }, 20);
    };

    const showError = (message) => {
        setMessage(message);
        setMessageType('error-msg');
        setShake(true);

        setTimeout(() => {
            setShake(false);
        }, 500);
    };

    const showSuccess = (message) => {
        setMessage(message);
        setMessageType('success-msg');
    };

    const handleKeyPress = (event) => {
        if (event.key === 'Enter') {
            handleLogin();
        }
    };

    return (
        <div className={`login-container ${shake ? 'shake' : ''}`}>
            <div className="login-header">
                <h1>Авторизация</h1>
            </div>
            <div className="form-group">
                <label htmlFor="username">Имя пользователя</label>
                <input
                    type="text"
                    id="username"
                    placeholder="Введите имя пользователя"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                />
            </div>
            <div className="form-group">
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
                className="login-btn"
                onClick={handleLogin}
            >
                Войти
            </button>
            <div className={`login-msg ${messageType}`}>
                {message}
            </div>
        </div>
    );
};