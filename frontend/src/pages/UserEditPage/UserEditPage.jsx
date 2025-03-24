import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import './UserEditPage.css';
import { ArrowLeft } from "lucide-react";
import Header from "../../components/layout/header/Header";

const UserEditPage = () => {
    //const navigate = useNavigate();
    const { userId } = useParams();
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

    useEffect(() => {
        const fetchUserData = async () => {
            try {
                const mockUser = {
                    id: userId,
                    email: 'ivan@example.com',
                    username: 'ivanov',
                    password: '',
                    hiredAt: '2020-01-15',
                    birthday: '1990-05-20',
                    isMale: true,
                    isAdmin: true,
                    hasPublishedRights: false,
                    departmentId: '1'
                };

                setFormData({
                    email: mockUser.email,
                    username: mockUser.username,
                    password: '',
                    hiredAt: mockUser.hiredAt,
                    birthday: mockUser.birthday,
                    isMale: mockUser.isMale,
                    isAdmin: mockUser.isAdmin,
                    hasPublishedRights: mockUser.hasPublishedRights,
                    departmentId: mockUser.departmentId
                });
            } catch (error) {
                console.error('Ошибка при загрузке данных пользователя:', error);
                alert('Произошла ошибка при загрузке данных пользователя');
            }
        };

        fetchUserData();
    }, [userId]);

    const handleChange = (e) => {
        const { name, value, type, checked } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: type === 'checkbox' ? checked : value
        }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!formData.email || !formData.username || !formData.birthday || !formData.departmentId) {
            alert('Пожалуйста, заполните все обязательные поля');
            return;
        }

        try {
            const userData = {
                Email: formData.email,
                Username: formData.username,
                PasswordHash: formData.password || undefined, // Отправляем только если изменен
                HiredAt: formData.hiredAt ? new Date(formData.hiredAt) : null,
                Birthday: new Date(formData.birthday),
                IsMale: formData.isMale,
                IsAdmin: formData.isAdmin,
                HasPublishedRights: formData.hasPublishedRights,
                DepartmentId: formData.departmentId
            };

            console.log('Отправка данных для обновления:', userData);

            await new Promise(resolve => setTimeout(resolve, 1000));

            alert('Данные пользователя успешно обновлены!');
            //navigate('/users');
        } catch (error) {
            console.error('Ошибка при обновлении пользователя:', error);
            alert('Произошла ошибка при обновлении пользователя');
        }
    };

    return (
        <div className="user-edit-page">
            <Header title={`Редактирование пользователя #${userId}`} />

            <div className="page-header">
                <button className="back-button">
                    <ArrowLeft size={18} className="back-icon" />
                    Назад
                </button>
                {/*<button className="back-button" onClick={() => navigate(-1)}>*/}
            </div>

            <form onSubmit={handleSubmit} className="user-edit-form">
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
                            <label htmlFor="password">Новый пароль</label>
                            <input
                                type="password"
                                id="password"
                                name="password"
                                value={formData.password}
                                onChange={handleChange}
                                placeholder="Оставьте пустым, чтобы не менять"
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
                        Сохранить изменения
                    </button>
                </div>
            </form>
        </div>
    );
};

export default UserEditPage;