import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import './UserCreatePage.css';
import {ArrowLeft} from "lucide-react";
import Header from "../../components/layout/header/Header";

const UserCreatePage = () => {
    //const navigate = useNavigate();
    const [formData, setFormData] = useState({
        email: '',
        username: '',
        password: '',
        hiredAt: '',
        birthday: '',
        isMale: true,
        isAdmin: false,
        hasPublishedRights: false,
        departmentId: ''
    });

    const departments = [
        { id: '1', name: 'Разработка' },
        { id: '2', name: 'Дизайн' },
        { id: '3', name: 'Маркетинг' },
        { id: '4', name: 'HR' },
        { id: '5', name: 'Тестирование' }
    ];

    const handleChange = (e) => {
        const { name, value, type, checked } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: type === 'checkbox' ? checked : value
        }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!formData.email || !formData.username || !formData.password || !formData.birthday || !formData.departmentId) {
            alert('Пожалуйста, заполните все обязательные поля');
            return;
        }

        try {
            const userData = {
                Email: formData.email,
                Username: formData.username,
                PasswordHash: formData.password, // В реальном приложении нужно хэшировать
                HiredAt: formData.hiredAt ? new Date(formData.hiredAt) : null,
                Birthday: new Date(formData.birthday),
                IsMale: formData.isMale,
                IsAdmin: formData.isAdmin,
                HasPublishedRights: formData.hasPublishedRights,
                DepartmentId: formData.departmentId
            };

            console.log('Отправка данных:', userData);

            await new Promise(resolve => setTimeout(resolve, 1000));

            alert('Пользователь успешно создан!');
            //navigate('/users');
        } catch (error) {
            console.error('Ошибка при создании пользователя:', error);
            alert('Произошла ошибка при создании пользователя');
        }
    };

    return (
        <div className="user-create-page">
            <Header title={"Создание нового пользователя"} />

            <div className="page-header">
                <button className="back-button">
                    <ArrowLeft size={18} className="back-icon" />
                    Назад
                </button>
                {/*<button className="back-button" onClick={() => navigate(-1)}>*/}
                {/*<h1>Создание нового пользователя</h1>*/}
            </div>

            <form onSubmit={handleSubmit} className="user-create-form">
                <div className="form-section">
                    <h2 className="section-title">Основная информация</h2>

                    <div className="form-grid">
                        <div className="form-group">
                            <label htmlFor="email">Email*</label>
                            <input
                                type="email"
                                id="email"
                                name="email"
                                value={formData.email}
                                onChange={handleChange}
                                required
                                placeholder="user@example.com"
                            />
                        </div>

                        <div className="form-group">
                            <label htmlFor="username">Логин*</label>
                            <input
                                type="text"
                                id="username"
                                name="username"
                                value={formData.username}
                                onChange={handleChange}
                                required
                                placeholder="Введите логин"
                            />
                        </div>

                        <div className="form-group">
                            <label htmlFor="password">Пароль*</label>
                            <input
                                type="password"
                                id="password"
                                name="password"
                                value={formData.password}
                                onChange={handleChange}
                                required
                                placeholder="Не менее 8 символов"
                            />
                        </div>

                        <div className="form-row">
                            <div className="form-group">
                                <label htmlFor="birthday">Дата рождения*</label>
                                <input
                                    type="date"
                                    id="birthday"
                                    name="birthday"
                                    value={formData.birthday}
                                    onChange={handleChange}
                                    required
                                />
                            </div>

                            <div className="form-group">
                                <label htmlFor="hiredAt">Дата приема</label>
                                <input
                                    type="date"
                                    id="hiredAt"
                                    name="hiredAt"
                                    value={formData.hiredAt}
                                    onChange={handleChange}
                                    placeholder="Не указано"
                                />
                            </div>
                        </div>

                        <div className="form-group">
                            <label htmlFor="departmentId">Отдел*</label>
                            <select
                                id="departmentId"
                                name="departmentId"
                                value={formData.departmentId}
                                onChange={handleChange}
                                required
                            >
                                <option value="" disabled>Выберите отдел</option>
                                {departments.map(dept => (
                                    <option key={dept.id} value={dept.id}>{dept.name}</option>
                                ))}
                            </select>
                        </div>
                    </div>
                </div>

                <div className="form-section">
                    <h2 className="section-title">Дополнительные настройки</h2>

                    <div className="checkbox-container">
                        <div className="checkbox-group">
                            <input
                                type="checkbox"
                                id="isMale"
                                name="isMale"
                                checked={formData.isMale}
                                onChange={handleChange}
                            />
                            <label htmlFor="isMale">Мужской пол</label>
                        </div>

                        <div className="checkbox-group">
                            <input
                                type="checkbox"
                                id="isAdmin"
                                name="isAdmin"
                                checked={formData.isAdmin}
                                onChange={handleChange}
                            />
                            <label htmlFor="isAdmin">Администратор</label>
                        </div>

                        <div className="checkbox-group">
                            <input
                                type="checkbox"
                                id="hasPublishedRights"
                                name="hasPublishedRights"
                                checked={formData.hasPublishedRights}
                                onChange={handleChange}
                            />
                            <label htmlFor="hasPublishedRights">Права на публикацию</label>
                        </div>
                    </div>
                </div>

                <div className="form-actions">
                    {/*<button type="button" className="cancel-button" onClick={() => navigate('/users')}>*/}
                    <button type="button" className="cancel-button">
                        Отмена
                    </button>
                    <button type="submit" className="submit-button">
                        Создать пользователя
                    </button>
                </div>
            </form>
        </div>
    );
};

export default UserCreatePage;