import React, {useEffect, useState} from 'react';
import { useNavigate } from 'react-router-dom';
import './UserCreatePage.css';
import {ArrowLeft} from "lucide-react";
import Header from "../../components/layout/header/Header";
import {getAllDepartmentsShort} from "../../api-handlers/departmentsHandler";
import {createUser} from "../../api-handlers/usersHandler";

const UserCreatePage = () => {
    const navigate = useNavigate();
    const [formData, setFormData] = useState({
        email: '',
        username: '',
        fullName: '', // Добавлено новое поле
        password: '',
        hiredAt: '',
        birthday: '',
        isMale: true,
        isAdmin: false,
        hasPublishedRights: false,
        departmentId: ''
    });

    const [departments, setDepartments] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchDepartments = async () => {
            try {
                const response = await getAllDepartmentsShort();
                setDepartments(response);
                setLoading(false);
            } catch (err) {
                setError('Не удалось загрузить отделы');
                setLoading(false);
                console.error('Ошибка при загрузке отделов:', err);
            }
        };

        fetchDepartments();
    }, []);

    const handleChange = (e) => {
        const { name, value, type, checked } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: type === 'checkbox' ? checked : value
        }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        const requiredFields = ['email', 'username', 'fullName', 'password', 'birthday', 'departmentId'];
        const missingFields = requiredFields.filter(field => !formData[field]);

        if (missingFields.length > 0) {
            alert(`Пожалуйста, заполните все обязательные поля: ${missingFields.join(', ')}`);
            return;
        }

        try {
            const response = await createUser(
                formData.fullName,
                formData.email,
                formData.username,
                formData.password,
                formData.hiredAt || null,
                formData.birthday,
                formData.isMale,
                formData.isAdmin,
                formData.hasPublishedRights,
                formData.departmentId
            );

            alert('Пользователь успешно создан!');
            navigate('/users');
        } catch (error) {
            console.error('Ошибка при создании пользователя:', error);
            alert(error.message || 'Произошла ошибка при создании пользователя');
        }
    };

    const handleBack = () => {
        navigate(-1);
    };

    return (
        <div className="user-create-page">
            <Header title={"Создание нового пользователя"} />

            <div className="page-header">
                <button className="back-button" onClick={handleBack}>
                    <ArrowLeft size={18} className="back-icon" />
                    Назад
                </button>
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
                            <label htmlFor="fullName">ФИО*</label>
                            <input
                                type="text"
                                id="fullName"
                                name="fullName"
                                value={formData.fullName}
                                onChange={handleChange}
                                required
                                placeholder="Иванов Иван Иванович"
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
                            {loading ? (
                                <p>Загрузка отделов...</p>
                            ) : error ? (
                                <p className="error">{error}</p>
                            ) : (
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
                            )}
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
                    <button type="button" className="cancel-button" onClick={handleBack}>
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