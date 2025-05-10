import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, X, Users, User } from 'lucide-react';
import { createChat } from '../../api-handlers/chatHandler';
import { getAllUsers } from '../../api-handlers/usersHandler';
import GradientCircle from '../../components/extensions/GradientCircle';
import Header from '../../components/layout/header/Header';
import './ChatCreatePage.css';

const ChatCreatePage = () => {
    const navigate = useNavigate();
    const [chatName, setChatName] = useState('');
    const [isPersonal, setIsPersonal] = useState(true);
    const [selectedUsers, setSelectedUsers] = useState([]);
    const [users, setUsers] = useState([]);
    const [searchTerm, setSearchTerm] = useState('');
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchUsers = async () => {
            try {
                const usersData = await getAllUsers(true, false);
                if (usersData) {
                    setUsers(usersData);
                }
            } catch (err) {
                setError('Не удалось загрузить пользователей');
                console.error('Error fetching users:', err);
            } finally {
                setLoading(false);
            }
        };

        fetchUsers();
    }, []);

    const handleUserSelect = (user) => {
        if (isPersonal && selectedUsers.length >= 1) return;

        if (selectedUsers.some(u => u.id === user.id)) {
            setSelectedUsers(selectedUsers.filter(u => u.id !== user.id));
        } else {
            setSelectedUsers([...selectedUsers, user]);
        }
    };

    const handleRemoveUser = (userId) => {
        setSelectedUsers(selectedUsers.filter(user => user.id !== userId));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (isPersonal && selectedUsers.length !== 1) {
            setError('Личный чат должен содержать ровно одного участника');
            return;
        }

        if (!isPersonal && !chatName.trim()) {
            setError('Название группового чата обязательно');
            return;
        }

        try {
            const creatorId = localStorage.getItem('uid');
            if (!creatorId) {
                setError('Не удалось определить создателя чата');
                return;
            }

            const membersIds = selectedUsers.map(user => user.id);

            const chatData = {
                name: isPersonal
                    ? `Чат с ${selectedUsers[0].username}`
                    : chatName,
                creator: creatorId,
                isPersonal,
                membersIds
            };

            const chatId = await createChat(chatData);
            if (chatId) {
                navigate(`/chat`);
            }
        } catch (err) {
            setError('Не удалось создать чат');
            console.error('Error creating chat:', err);
        }
    };

    const filteredUsers = users.filter(user =>
        user.username.toLowerCase().includes(searchTerm.toLowerCase()) ||
        user.email.toLowerCase().includes(searchTerm.toLowerCase())
    );

    if (loading) {
        return (
            <div className="chat-create-container">
                <Header title="Создать чат" />
                <div className="loading">Загрузка пользователей...</div>
            </div>
        );
    }

    return (
        <div className="chat-create-container">
            <Header title="Создать чат" withBackButton />

            <form onSubmit={handleSubmit} className="chat-create-form">
                <div className="chat-type-selector">
                    <button
                        type="button"
                        className={`chat-type-btn ${isPersonal ? 'active' : ''}`}
                        onClick={() => setIsPersonal(true)}
                    >
                        <User size={18} />
                        <span>Личный чат</span>
                    </button>
                    <button
                        type="button"
                        className={`chat-type-btn ${!isPersonal ? 'active' : ''}`}
                        onClick={() => setIsPersonal(false)}
                    >
                        <Users size={18} />
                        <span>Групповой чат</span>
                    </button>
                </div>

                {!isPersonal && (
                    <div className="form-group">
                        <label>Название чата</label>
                        <input
                            type="text"
                            value={chatName}
                            onChange={(e) => setChatName(e.target.value)}
                            placeholder="Введите название чата"
                        />
                    </div>
                )}

                <div className="form-group">
                    <label>Выберите участников ({selectedUsers.length})</label>
                    <div className="selected-users">
                        {selectedUsers.map(user => (
                            <div key={user.id} className="selected-user">
                                <GradientCircle name={user.username} size={24} />
                                <span>{user.username}</span>
                                <button
                                    type="button"
                                    onClick={() => handleRemoveUser(user.id)}
                                    className="remove-user-btn"
                                >
                                    <X size={14} />
                                </button>
                            </div>
                        ))}
                    </div>
                </div>

                <div className="form-group">
                    <input
                        type="text"
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        placeholder="Поиск пользователей..."
                    />
                </div>

                <div className="users-list">
                    {filteredUsers.map(user => (
                        <div
                            key={user.id}
                            className={`user-item ${selectedUsers.some(u => u.id === user.id) ? 'selected' : ''}`}
                            onClick={() => handleUserSelect(user)}
                        >
                            <GradientCircle name={user.username} size={32} />
                            <div className="user-info">
                                <span className="username">{user.username}</span>
                                <span className="email">{user.email}</span>
                            </div>
                        </div>
                    ))}
                </div>

                {error && <div className="error-message">{error}</div>}

                <button
                    type="submit"
                    className="create-chat-btn"
                    disabled={isPersonal ? selectedUsers.length !== 1 : selectedUsers.length < 1}
                >
                    Создать чат
                </button>
            </form>
        </div>
    );
};

export default ChatCreatePage;