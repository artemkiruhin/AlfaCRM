import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import './UserEditPage.css';
import { ArrowLeft } from "lucide-react";
import Header from "../../components/layout/header/Header";
import { getAllDepartmentsShort } from "../../api-handlers/departmentsHandler";
import {blockUser, editUser, getUserById} from "../../api-handlers/usersHandler";
import { setDateToInputFormat } from "../../extensions/utils";

const UserEditPage = () => {
    const navigate = useNavigate();
    const { id } = useParams();
    const [initialData, setInitialData] = useState(null);
    const [formData, setFormData] = useState({
        email: '',
        username: '',
        fullName: '',
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
    const [hasChanges, setHasChanges] = useState(false);

    useEffect(() => {
        const fetchData = async () => {
            try {
                setLoading(true);

                const [departmentsResponse, userResponse] = await Promise.all([
                    getAllDepartmentsShort(),
                    id ? getUserById(id) : Promise.resolve(null)
                ]);

                setDepartments(departmentsResponse);

                if (userResponse) {
                    setInitialData({
                        email: userResponse.email,
                        username: userResponse.username,
                        fullName: userResponse.fullName,
                        hiredAt: userResponse.hiredAt,
                        birthday: userResponse.birthday,
                        isMale: userResponse.isMale,
                        isAdmin: userResponse.isAdmin,
                        hasPublishedRights: userResponse.hasPublishedRights,
                        departmentId: userResponse.departmentId
                    });

                    setFormData({
                        email: userResponse.email,
                        username: userResponse.username,
                        fullName: userResponse.fullName,
                        password: '',
                        hiredAt: userResponse.hiredAt,
                        birthday: userResponse.birthday,
                        isMale: userResponse.isMale,
                        isAdmin: userResponse.isAdmin,
                        hasPublishedRights: userResponse.hasPublishedRights,
                        departmentId: userResponse.departmentId
                    });
                }
            } catch (err) {
                setError('Не удалось загрузить данные');
                console.error('Ошибка при загрузке данных:', err);
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, [id]);

    useEffect(() => {
        if (initialData) {
            const changesDetected = (
                formData.email !== initialData.email ||
                formData.isAdmin !== initialData.isAdmin ||
                formData.hasPublishedRights !== initialData.hasPublishedRights ||
                formData.departmentId !== initialData.departmentId
            );
            setHasChanges(changesDetected);
        }
    }, [formData, initialData]);

    const handleChange = (e) => {
        const { name, value, type, checked } = e.target;

        if (type === 'checkbox') {
            setFormData(prev => ({ ...prev, [name]: checked }));
        } else {
            setFormData(prev => ({ ...prev, [name]: value }));
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!hasChanges) {
            alert('Нет изменений для сохранения');
            return;
        }

        try {
            const changes = { id };

            // Добавляем только измененные поля из контракта
            if (formData.email !== initialData.email) {
                changes.email = formData.email || null;
            }
            if (formData.isAdmin !== initialData.isAdmin) {
                changes.isAdmin = formData.isAdmin;
            }
            if (formData.hasPublishedRights !== initialData.hasPublishedRights) {
                changes.hasPublishedRights = formData.hasPublishedRights;
            }
            if (formData.departmentId !== initialData.departmentId) {
                changes.departmentId = formData.departmentId || null;
            }

            await editUser(
                changes.id,
                changes.email,
                changes.isAdmin,
                changes.hasPublishedRights,
                changes.departmentId
            );

            alert('Данные пользователя успешно обновлены!');
            navigate('/users');
        } catch (error) {
            console.error('Ошибка при обновлении пользователя:', error);
            alert(`Произошла ошибка: ${error.message}`);
        }
    };

    const handleBack = () => {
        navigate('/users');
    };

    const handleBlockUser =  async () => {
        if (window.confirm('Вы уверены, что хотите заблокировать эту учетную запись?')) {
            await blockUser(id);
            navigate('/users');
        }
    };

    if (loading) {
        return (
            <div className="user-edit-page">
                <Header title={`Редактирование пользователя #${id}`} />
                <div className="loading-container">
                    <p>Загрузка данных...</p>
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="user-edit-page">
                <Header title={`Редактирование пользователя #${id}`} />
                <div className="error-container">
                    <p className="error">{error}</p>
                    <button className="back-button" onClick={handleBack}>
                        <ArrowLeft size={18} className="back-icon" />
                        Назад
                    </button>
                </div>
            </div>
        );
    }

    return (
        <div className="user-edit-page">
            <Header title={`Редактирование пользователя #${id}`} />

            <div className="page-header">
                <button className="back-button" onClick={handleBack}>
                    <ArrowLeft size={18} className="back-icon" />
                    Назад
                </button>
            </div>

            <form onSubmit={handleSubmit} className="user-edit-form">
                <div className="form-section">
                    <h2 className="section-title">Основная информация</h2>

                    <div className="form-grid">
                        <div className="form-group">
                            <label htmlFor="email">Email</label>
                            <input
                                type="email"
                                id="email"
                                name="email"
                                value={formData.email}
                                onChange={handleChange}
                                placeholder="user@example.com"
                                className={formData.email !== initialData?.email ? 'field-changed' : ''}
                            />
                        </div>

                        <div className="form-group">
                            <label htmlFor="username">Логин</label>
                            <input
                                type="text"
                                id="username"
                                name="username"
                                value={formData.username}
                                readOnly
                                placeholder="Логин пользователя"
                            />
                        </div>

                        <div className="form-group">
                            <label htmlFor="fullName">ФИО</label>
                            <input
                                type="text"
                                id="fullName"
                                name="fullName"
                                value={formData.fullName}
                                readOnly
                                placeholder="ФИО пользователя"
                            />
                        </div>

                        <div className="form-group">
                            <label htmlFor="password">Новый пароль</label>
                            <input
                                type="password"
                                id="password"
                                name="password"
                                value={formData.password}
                                readOnly
                                placeholder="Пароль можно изменить в отдельном разделе"
                            />
                        </div>

                        <div className="form-row">
                            <div className="form-group">
                                <label htmlFor="birthday">Дата рождения</label>
                                <input
                                    type="date"
                                    id="birthday"
                                    name="birthday"
                                    value={setDateToInputFormat(formData.birthday)}
                                    readOnly
                                />
                            </div>

                            <div className="form-group">
                                <label htmlFor="hiredAt">Дата приема</label>
                                <input
                                    type="date"
                                    id="hiredAt"
                                    name="hiredAt"
                                    value={setDateToInputFormat(formData.hiredAt)}
                                    readOnly
                                    placeholder="Не указано"
                                />
                            </div>
                        </div>

                        <div className="form-group">
                            <label htmlFor="departmentId">Отдел</label>
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
                                    className={formData.departmentId !== initialData?.departmentId ? 'field-changed' : ''}
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
                    <h2 className="section-title">Права доступа</h2>

                    <div className="checkbox-container">
                        <div className="checkbox-group">
                            <input
                                type="checkbox"
                                id="isMale"
                                name="isMale"
                                checked={formData.isMale}
                                onChange={() => {}}
                                disabled
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
                                className={formData.isAdmin !== initialData?.isAdmin ? 'field-changed' : ''}
                            />
                            <label htmlFor="isAdmin">Администратор</label>
                            {formData.isAdmin !== initialData?.isAdmin && <span className="checkbox-hint">{formData.isAdmin ? '(установлен)' : '(снят)'}</span>}
                        </div>

                        <div className="checkbox-group">
                            <input
                                type="checkbox"
                                id="hasPublishedRights"
                                name="hasPublishedRights"
                                checked={formData.hasPublishedRights}
                                onChange={handleChange}
                                className={formData.hasPublishedRights !== initialData?.hasPublishedRights ? 'field-changed' : ''}
                            />
                            <label htmlFor="hasPublishedRights">Права на публикацию</label>
                            {formData.hasPublishedRights !== initialData?.hasPublishedRights && <span className="checkbox-hint">{formData.hasPublishedRights ? '(установлен)' : '(снят)'}</span>}
                        </div>
                    </div>
                </div>

                <div className="form-actions">
                    <div className="danger-actions">
                        <button
                            type="button"
                            className="danger-button block-button"
                            onClick={handleBlockUser}
                        >
                            Заблокировать учетную запись
                        </button>
                    </div>

                    <div className="main-actions">
                        <button type="button" className="cancel-button" onClick={handleBack}>
                            Отмена
                        </button>
                        <button
                            type="submit"
                            className="submit-button"
                            disabled={!hasChanges}
                        >
                            Сохранить изменения
                        </button>
                    </div>
                </div>
            </form>
        </div>
    );
};

export default UserEditPage;