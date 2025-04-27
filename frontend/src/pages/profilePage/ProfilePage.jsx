import React, { useState, useEffect } from 'react';
import {getProfile} from "../../api-handlers/usersHandler";
import './ProfilePage.css'

const ProfilePage = () => {
    const [profile, setProfile] = useState({
        id: '',
        fullname: '',
        email: '',
        username: '',
        birthday: '',
        hiredAt: '',
        firedAt: '',
        isMale: '',
        isAdmin: '',
        hasPublishedRights: '',
        departmentName: '',
        postsAmount: '',
        commentsAmount: '',
        messagesAmount: ''
    });
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchProfile = async () => {
            try {
                const data = await getProfile();
                console.log(data);
                setProfile(data);
                setLoading(false);
            } catch (err) {
                setError(err.message);
                setLoading(false);
            }
        };

        fetchProfile();
    }, []);

    const handleChangePassword = () => {
        alert('Функция изменения пароля в разработке');
    };

    if (loading) {
        return <div className="loading-spinner">Загрузка профиля...</div>;
    }

    if (error) {
        return <div className="error-message">Ошибка: {error}</div>;
    }

    if (!profile) {
        return <div className="no-profile">Профиль не найден</div>;
    }

    return (
        <div className="profile-page">
            <div className="profile-header">
                <h1 className="profile-title">Мой профиль</h1>
            </div>

            <div className="profile-content">
                <div className="profile-section">
                    <h2 className="section-title">Основная информация</h2>
                    <div className="profile-grid">
                        <div className="profile-field">
                            <label className="field-label">Полное имя</label>
                            <input
                                type="text"
                                value={profile.fullname || ''}
                                readOnly
                                className="field-input"
                            />
                        </div>

                        <div className="profile-field">
                            <label className="field-label">Email</label>
                            <input
                                type="email"
                                value={profile.email || ''}
                                readOnly
                                className="field-input"
                            />
                        </div>

                        <div className="profile-field">
                            <label className="field-label">Имя пользователя</label>
                            <input
                                type="text"
                                value={profile.username || ''}
                                readOnly
                                className="field-input"
                            />
                        </div>

                        <div className="profile-field">
                            <label className="field-label">Пол</label>
                            <input
                                type="text"
                                value={profile.isMale ? 'Мужской' : 'Женский'}
                                readOnly
                                className="field-input"
                            />
                        </div>

                        <div className="profile-field">
                            <label className="field-label">Дата рождения</label>
                            <input
                                type="text"
                                value={profile.birthday || 'Не указано'}
                                readOnly
                                className="field-input"
                            />
                        </div>
                    </div>
                </div>

                <div className="profile-section">
                    <h2 className="section-title">Рабочая информация</h2>
                    <div className="profile-grid">
                        <div className="profile-field">
                            <label className="field-label">Дата приема</label>
                            <input
                                type="text"
                                value={profile.hiredAt || 'Не указано'}
                                readOnly
                                className="field-input"
                            />
                        </div>

                        <div className="profile-field">
                            <label className="field-label">Дата увольнения</label>
                            <input
                                type="text"
                                value={profile.firedAt || 'Не уволен'}
                                readOnly
                                className="field-input"
                            />
                        </div>

                        <div className="profile-field">
                            <label className="field-label">Отдел</label>
                            <input
                                type="text"
                                value={profile.departmentName || 'Не указан'}
                                readOnly
                                className="field-input"
                            />
                        </div>

                        <div className="profile-field">
                            <label className="field-label">Роль</label>
                            <input
                                type="text"
                                value={profile.isAdmin ? 'Администратор' : 'Пользователь'}
                                readOnly
                                className="field-input"
                            />
                        </div>

                        <div className="profile-field">
                            <label className="field-label">Права на публикацию</label>
                            <input
                                type="text"
                                value={profile.hasPublishedRights ? 'Да' : 'Нет'}
                                readOnly
                                className="field-input"
                            />
                        </div>
                    </div>
                </div>

                <div className="profile-section">
                    <h2 className="section-title">Статистика</h2>
                    <div className="profile-grid">
                        <div className="profile-field">
                            <label className="field-label">Количество постов</label>
                            <input
                                type="text"
                                value={profile.postsAmount || 0}
                                readOnly
                                className="field-input"
                            />
                        </div>

                        <div className="profile-field">
                            <label className="field-label">Количество комментариев</label>
                            <input
                                type="text"
                                value={profile.commentsAmount || 0}
                                readOnly
                                className="field-input"
                            />
                        </div>

                        <div className="profile-field">
                            <label className="field-label">Количество сообщений</label>
                            <input
                                type="text"
                                value={profile.messagesAmount || 0}
                                readOnly
                                className="field-input"
                            />
                        </div>
                    </div>
                </div>

                <div className="profile-actions">
                    <button
                        onClick={handleChangePassword}
                        className="change-password-btn"
                    >
                        Изменить пароль
                    </button>
                </div>
            </div>
        </div>
    );
};

export default ProfilePage;