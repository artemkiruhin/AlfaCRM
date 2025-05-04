import React, { useState, useEffect } from 'react';
import {getProfile, resetPassword} from "../../api-handlers/usersHandler";
import './ProfilePage.css';
import { useNavigate } from 'react-router-dom';
import { logout } from "../../api-handlers/authHandler";

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
    const [showChangePasswordModal, setShowChangePasswordModal] = useState(false);
    const [currentPassword, setCurrentPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [passwordError, setPasswordError] = useState('');
    const navigate = useNavigate();

    useEffect(() => {
        const fetchProfile = async () => {
            try {
                const data = await getProfile();
                setProfile(data);
                setLoading(false);
            } catch (err) {
                setError(err.message);
                setLoading(false);
            }
        };

        fetchProfile();
    }, []);

    const handleChangePasswordClick = () => {
        setShowChangePasswordModal(true);
        setCurrentPassword('');
        setNewPassword('');
        setConfirmPassword('');
        setPasswordError('');
    };

    const handleCloseModal = () => {
        setShowChangePasswordModal(false);
    };

    const handlePasswordSubmit = async (e) => {
        e.preventDefault();

        // Валидация
        if (!currentPassword || !newPassword || !confirmPassword) {
            setPasswordError('Все поля обязательны для заполнения');
            return;
        }

        if (newPassword !== confirmPassword) {
            setPasswordError('Новый пароль и подтверждение не совпадают');
            return;
        }

        if (newPassword.length < 6) {
            setPasswordError('Пароль должен содержать минимум 6 символов');
            return;
        }

        try {
            await resetPassword(localStorage.getItem('uid'), newPassword, true, currentPassword);
            setShowChangePasswordModal(false);
        } catch (err) {
            setPasswordError(err.message || 'Ошибка при изменении пароля');
        }
    };

    const handleLogout = async () => {
        await logout();
        localStorage.clear();
        navigate('/login');
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
            {showChangePasswordModal && (
                <div className="modal-overlay">
                    <div className="password-modal">
                        <button className="close-modal-btn" onClick={handleCloseModal}>×</button>
                        <h2>Изменение пароля</h2>
                        <form onSubmit={handlePasswordSubmit}>
                            <div className="form-group">
                                <label>Текущий пароль</label>
                                <input
                                    type="password"
                                    value={currentPassword}
                                    onChange={(e) => setCurrentPassword(e.target.value)}
                                    placeholder="Введите текущий пароль"
                                    autoComplete="current-password"
                                />
                            </div>
                            <div className="form-group">
                                <label>Новый пароль</label>
                                <input
                                    type="password"
                                    value={newPassword}
                                    onChange={(e) => setNewPassword(e.target.value)}
                                    placeholder="Введите новый пароль"
                                    autoComplete="new-password"
                                />
                            </div>
                            <div className="form-group">
                                <label>Подтвердите новый пароль</label>
                                <input
                                    type="password"
                                    value={confirmPassword}
                                    onChange={(e) => setConfirmPassword(e.target.value)}
                                    placeholder="Повторите новый пароль"
                                    autoComplete="new-password"
                                />
                            </div>
                            {passwordError && <div className="error-message">{passwordError}</div>}
                            <div className="modal-actions">
                                <button type="button" className="cancel-btn" onClick={handleCloseModal}>
                                    Отмена
                                </button>
                                <button type="submit" className="submit-btn">
                                    Изменить пароль
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

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
                        onClick={handleChangePasswordClick}
                        className="change-password-btn"
                    >
                        Изменить пароль
                    </button>
                    <button
                        onClick={handleLogout}
                        className="logout-btn"
                    >
                        Выйти из аккаунта
                    </button>
                </div>
            </div>
        </div>
    );
};

export default ProfilePage;