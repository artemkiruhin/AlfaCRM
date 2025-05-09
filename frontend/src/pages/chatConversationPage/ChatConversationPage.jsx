import React, {useEffect, useRef, useState} from 'react';
import {useNavigate, useParams} from 'react-router-dom';
import * as signalR from '@microsoft/signalr';
import "./ChatConversationPage.css";
import Header from "../../components/layout/header/Header";
import {getAllMessages, getChatById, sendMessage, pinMessage} from '../../api-handlers/chatHandler';
import {getUserById} from '../../api-handlers/usersHandler';
import {CHAT_URL} from '../../api-handlers/baseHandler';
import {formatDate} from "../../extensions/utils";

const ChatConversationPage = () => {
    const { id } = useParams();
    const chatId = id;
    const navigate = useNavigate();
    const [messages, setMessages] = useState([]);
    const [pinnedMessages, setPinnedMessages] = useState([]);
    const [newMessage, setNewMessage] = useState('');
    const [chatPartner, setChatPartner] = useState({ name: 'Loading...', id: null });
    const [currentUser, setCurrentUser] = useState(null);
    const [loading, setLoading] = useState(true);
    const [connectionStatus, setConnectionStatus] = useState('disconnected');
    const messagesEndRef = useRef(null);
    const connectionRef = useRef(null);
    const [currentChat, setCurrentChat] = useState(null);

    const scrollToBottom = () => {
        messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
    };

    useEffect(() => {
        const fetchChat = async () => {
            const response = await getChatById(id, false);
            const c = {
                id: response.data.id,
                name: response.data.name
            }
            setCurrentChat(c);
        }
        fetchChat();
    }, [chatId]);

    useEffect(() => {
        const fetchMessages = async () => {
            const result = await getAllMessages(chatId);
            const pinned = result.filter(msg => msg.isPinned);
            const regular = result.filter(msg => !msg.isPinned);
            setPinnedMessages(pinned);
            setMessages(regular);
        }
        fetchMessages();
    }, [chatId]);

    useEffect(() => {
        scrollToBottom();
    }, [messages]);

    useEffect(() => {
        const fetchCurrentUser = async () => {
            try {
                const userId = localStorage.getItem('uid');
                if (userId) {
                    const user = await getUserById(userId);
                    setCurrentUser(user);
                }
            } catch (error) {
                console.error('Error fetching current user:', error);
            } finally {
                setLoading(false);
            }
        };

        fetchCurrentUser();
    }, []);

    useEffect(() => {
        const initializeSignalRConnection = async () => {
            connectionRef.current = new signalR.HubConnectionBuilder()
                .withUrl(CHAT_URL)
                .withAutomaticReconnect()
                .build();

            connectionRef.current.onclose(() => {
                setConnectionStatus('disconnected');
                console.log('SignalR connection closed');
            });

            connectionRef.current.onreconnecting(() => {
                setConnectionStatus('reconnecting');
                console.log('SignalR reconnecting...');
            });

            connectionRef.current.onreconnected(async () => {
                setConnectionStatus('connected');
                console.log('SignalR reconnected');

                if (currentUser) {
                    try {
                        const chatInfo = await getChatById(chatId);
                        await connectionRef.current.invoke("JoinChat", currentUser.username, chatInfo.data.name)
                    } catch (err) {
                        console.error("Error rejoining chat:", err);
                    }
                }
            });

            connectionRef.current.on("ReceiveMessage", (senderId, text, createdAt, senderUsername) => {
                const isCurrentUser = senderId === localStorage.getItem("uid");
                const parsedDate = new Date(createdAt);

                setMessages(prevMessages => {
                    const newMessage = {
                        id: Date.now(),
                        content: text,
                        isMyMessage: isCurrentUser,
                        createdAt: parsedDate,
                        senderName: isCurrentUser ? 'You' : senderUsername,
                        isPinned: false
                    };

                    return [...prevMessages, newMessage];
                });
            });

            try {
                await connectionRef.current.start();
                setConnectionStatus('connected');
                console.log("Connected to SignalR Hub");

                if (currentUser) {
                    const chatInfo = await getChatById(chatId, false);
                    const chatName = chatInfo.data.name;
                    await connectionRef.current.invoke("JoinChat", currentUser.username, chatName);
                }
            } catch (err) {
                console.error("SignalR connection error:", err);
                setConnectionStatus('disconnected');
            }

            return connectionRef.current;
        };

        if (currentUser) {
            initializeSignalRConnection();
        }

        return () => {
            if (connectionRef.current) {
                const cleanup = async () => {
                    try {
                        if (connectionRef.current.state === signalR.HubConnectionState.Connected) {
                            const chatInfo = await getChatById(chatId);
                            await connectionRef.current.invoke("LeaveChat", chatInfo.name);
                            await connectionRef.current.stop();
                            console.log("SignalR connection closed and left chat");
                        }
                    } catch (err) {
                        console.error("Error during cleanup:", err);
                    }
                };
                cleanup();
            }
        };
    }, [chatId, currentUser]);

    useEffect(() => {
        if (!currentUser) return;

        const loadInitialData = async () => {
            try {
                const [messagesData, chatInfo] = await Promise.all([
                    getAllMessages(chatId),
                    getChatById(chatId)
                ]);

                if (chatInfo.participants) {
                    const partner = chatInfo.participants.find(p => p.id !== currentUser.id);
                    if (partner) {
                        setChatPartner({
                            name: partner.username,
                            id: partner.id
                        });
                    }
                }

                if (messagesData) {
                    const pinned = messagesData.filter(msg => msg.isPinned);
                    const regular = messagesData.filter(msg => !msg.isPinned);

                    const formattedMessages = regular.map(msg => ({
                        id: msg.id,
                        content: msg.content,
                        createdAt: msg.createdAt,
                        isMyMessage: msg.sender?.id === currentUser.id,
                        senderName: msg.sender?.id === currentUser.id ? 'You' : msg.sender?.username,
                        isPinned: msg.isPinned
                    }));

                    const formattedPinned = pinned.map(msg => ({
                        id: msg.id,
                        content: msg.content,
                        createdAt: msg.createdAt,
                        isMyMessage: msg.sender?.id === currentUser.id,
                        senderName: msg.sender?.id === currentUser.id ? 'You' : msg.sender?.username,
                        isPinned: msg.isPinned
                    }));

                    setMessages(formattedMessages);
                    setPinnedMessages(formattedPinned);
                }
            } catch (error) {
                console.error('Error loading initial data:', error);
            }
        };

        loadInitialData();
    }, [chatId, currentUser]);

    const handleSendMessage = async () => {
        if (!newMessage.trim() || !connectionRef.current || connectionStatus !== 'connected') return;

        const messageText = newMessage.trim();
        setNewMessage('');

        try {
            const resp = await sendMessage({
                'content': messageText,
                'senderId': localStorage.getItem('uid'),
                'repliedMessageId': null,
                'chatId': chatId
            });
            const chatInfo = await getChatById(chatId, false);
            const chatName = chatInfo.data.name;
            await connectionRef.current.invoke(
                "SendMessage",
                currentUser.id,
                messageText,
                chatName,
                currentUser.username
            );

        } catch (error) {
            console.error("Error sending message:", error);
            setNewMessage(messageText);
        }
    };

    const handlePinMessage = async (messageId, isCurrentlyPinned) => {
        try {
            const result = await pinMessage(messageId, !isCurrentlyPinned);
            if (result) {
                if (isCurrentlyPinned) {
                    setPinnedMessages(prev => prev.filter(msg => msg.id !== messageId));
                    setMessages(prev => [...prev, {...result, isPinned: false}]);
                } else {
                    setPinnedMessages(prev => [...prev, {...result, isPinned: true}]);
                    setMessages(prev => prev.filter(msg => msg.id !== messageId));
                }
            }
        } catch (error) {
            console.error("Error pinning message:", error);
        }
    };

    const handleKeyPress = (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            handleSendMessage();
        }
    };

    const handleLeaveChat = async () => {
        try {
            if (connectionRef.current && connectionRef.current.state === signalR.HubConnectionState.Connected) {
                const chatInfo = await getChatById(chatId);
                await connectionRef.current.invoke("LeaveChat", chatInfo.name);
                await connectionRef.current.stop();
            }
        } catch (error) {
            console.error("Error leaving chat:", error);
        } finally {
            navigate("/messages");
        }
    };

    const renderMessage = (message) => (
        <div key={message.id}
             className={`message ${message.isMyMessage ? 'my-message' : 'partner-message'}`}>
            <div className="message-header">
                {!message.isMyMessage && (
                    <div className="message-sender">{message.senderName}</div>
                )}
                <button
                    className="pin-message-button"
                    onClick={() => handlePinMessage(message.id, message.isPinned)}
                    title={message.isPinned ? "Открепить сообщение" : "Закрепить сообщение"}
                >
                    {message.isPinned ? '📌' : '📍'}
                </button>
            </div>
            <p className="message-text">{message.content}</p>
            <span className="message-time">{formatDate(message.createdAt)}</span>
            {message.isPinned && <span className="pinned-badge">Закреплено</span>}
        </div>
    );

    if (loading || !currentUser) {
        return (
            <div className="loading-container">
                <Header />
                <div className="loading-content">Загрузка данных...</div>
            </div>
        );
    }

    return (
        <>
            <Header />
            <div className="chat-conversation-container">
                <div className="chat-header">
                    <div className="back-button" onClick={() => navigate(-1)}>
                        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                            <path d="M15 18L9 12L15 6" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
                        </svg>
                        Назад
                    </div>
                    <div className="chat-partner">
                        <h2 className="chat-partner-name">{currentChat?.name || 'Загрузка...'}</h2>
                    </div>
                </div>

                {pinnedMessages.length > 0 && (
                    <div className="pinned-messages-section">
                        <div className="pinned-messages-header">
                            <span>📌 Закрепленные сообщения</span>
                        </div>
                        <div className="pinned-messages-list">
                            {pinnedMessages.map(renderMessage)}
                        </div>
                    </div>
                )}

                <div className="chat-messages">
                    {messages.length === 0 && pinnedMessages.length === 0 ? (
                        <div className="no-messages">Сообщений еще нет. Начните беседу!</div>
                    ) : (
                        messages.map(renderMessage)
                    )}
                    <div ref={messagesEndRef} />
                </div>

                <div className="chat-input-container">
                    <textarea
                        className="chat-input"
                        placeholder="Введите ваше сообщение..."
                        value={newMessage}
                        onChange={(e) => setNewMessage(e.target.value)}
                        onKeyPress={handleKeyPress}
                        disabled={connectionStatus !== 'connected'}
                    />
                    <button
                        className="send-button"
                        onClick={handleSendMessage}
                        disabled={!newMessage.trim() || connectionStatus !== 'connected'}
                    >
                        {connectionStatus === 'connected' ? 'Отправить' : 'Соединение...'}
                    </button>
                </div>
            </div>
        </>
    );
};

export default ChatConversationPage;