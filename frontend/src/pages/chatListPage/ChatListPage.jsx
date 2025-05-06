import React, { useEffect, useState } from 'react';
import {Link, useNavigate} from 'react-router-dom';
import GradientCircle from '../../components/extensions/GradientCircle';
import "./ChatListPage.css"
import Header from "../../components/layout/header/Header";
import { getAllChats } from "../../api-handlers/chatHandler"
import NewsSearchPanel from "../../components/news/NewsSearchPanel";
import {Plus} from "lucide-react";

const ChatListPage = () => {
    const navigate = useNavigate();

    const [chats, setChats] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchChats = async () => {
            try {
                const response = await getAllChats();
                if (response) {
                    console.log('API Response:', response);
                    const chatData = response.data || response;
                    const formattedChats = chatData.map(chat => ({
                        id: chat.id,
                        name: chat.name,
                        lastMessage: chat.messages && chat.messages.length > 0
                            ? chat.messages[0].content
                            : 'Нет сообщений',
                        time: chat.messages && chat.messages.length > 0
                            ? formatTime(chat.messages[0].createdAt)
                            : '',
                        isOnline: false,
                        unreadCount: 0
                    }));
                    setChats(formattedChats);
                }
            } catch (err) {
                setError('Не удалось загрузить чаты');
                console.error('Error fetching chats:', err);
            } finally {
                setLoading(false);
            }
        };

        fetchChats();
    }, []);

    const handleAddChat = () => {
        navigate(`/chat/create`);
    }

    const formatTime = (dateString) => {
        const date = new Date(dateString);
        const now = new Date();

        if (date.toDateString() === now.toDateString()) {
            return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        }

        const yesterday = new Date(now);
        yesterday.setDate(yesterday.getDate() - 1);
        if (date.toDateString() === yesterday.toDateString()) {
            return 'Вчера';
        }

        return date.toLocaleDateString([], { day: 'numeric', month: 'short' });
    };

    if (loading) {
        return (
            <div className="chat-list-container">
                <Header title={"Мои чаты"} info="Загрузка..." />
                <div className="loading">Загрузка чатов...</div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="chat-list-container">
                <Header title={"Мои чаты"} />
                <div className="error">{error}</div>
            </div>
        );
    }

    return (
        <div className="chat-list-container">
            <Header title={"Мои чаты"} info={`Всего: ${chats.length}`}/>
            <div className="chats-controls">
                <button className="add-chat-button" onClick={handleAddChat}>
                    <Plus size={18} className="button-icon"/>
                    <span className="button-text">Добавить чат</span>
                </button>
            </div>
            <div className="chat-list">
                {chats.length > 0 ? (
                    chats.map((chat) => (
                        <Link to={`/chat/${chat.id}`} key={chat.id} className="chat-item">
                            <div className="chat-avatar">
                                <GradientCircle name={chat.name} size={40}/>
                            </div>
                            <div className="chat-info">
                                <h3 className="chat-name">{chat.name}</h3>
                                <p className="chat-last-message">{chat.lastMessage}</p>
                                <span className="chat-time">{chat.time}</span>
                            </div>
                            {chat.isOnline && <div className="chat-status online"/>}
                            {chat.unreadCount > 0 && (
                                <div className="chat-notification">
                                    {chat.unreadCount}
                                </div>
                            )}
                        </Link>
                    ))
                ) : (
                    <div className="no-chats">У вас пока нет чатов</div>
                )}
            </div>
        </div>
    );
};

export default ChatListPage;