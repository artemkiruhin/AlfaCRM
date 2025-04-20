import React, {useEffect, useRef, useState} from 'react';
import {useNavigate, useParams} from 'react-router-dom';
import * as signalR from '@microsoft/signalr';
import "./ChatConversationPage.css";
import Header from "../../components/layout/header/Header";
import {getAllMessages, getChatById, sendMessage} from '../../api-handlers/chatHandler';
import {getUserById} from '../../api-handlers/usersHandler';
import {CHAT_URL} from '../../api-handlers/baseHandler';
import {formatDate} from "../../extensions/utils";

const ChatConversationPage = () => {
    const { id } = useParams();
    const chatId = id;
    const navigate = useNavigate();
    const [messages, setMessages] = useState([]);
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
            console.log(response.data);
            const c = {
                id: response.data.id,
                name: response.data.name
            }
            setCurrentChat(c);

        }
        fetchChat();
        console.log(currentChat);
    }, [chatId]);

    useEffect(() => {
        const fetchMessages = async () => {
            const result = await getAllMessages(chatId);
            console.log(result);
            setMessages(result);
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
                        console.log(chatInfo);
                        connectionRef.current.invoke("JoinChat", currentUser.username, chatInfo.data.name)
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
                        senderName: isCurrentUser ? 'You' : senderUsername
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
                    console.log(chatInfo);
                    const chatName = chatInfo.data.name;
                    console.log(chatName);
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
                    const formattedMessages = messagesData.map(msg => ({
                        id: msg.id,
                        content: msg.content,
                        createdAt: msg.createdAt,
                        isMyMessage: msg.sender?.id === currentUser.id,
                        senderName: msg.sender?.id === currentUser.id ? 'You' : msg.sender?.username
                    }));

                    setMessages(formattedMessages);
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

    if (loading || !currentUser) {
        return (
            <div className="loading-container">
                <Header />
                <div className="loading-content">Loading chat...</div>
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
                        <h2 className="chat-partner-name">{currentChat?.name || 'Loading...'}</h2>
                        <div className={`connection-status ${connectionStatus}`}>
                            {connectionStatus === 'connected' ? 'Online' :
                                connectionStatus === 'reconnecting' ? 'Reconnecting...' : 'Offline'}
                        </div>
                    </div>
                    <button className="leave-chat-button" onClick={handleLeaveChat}>
                        Leave Chat
                    </button>
                </div>

                <div className="chat-messages">
                    {messages.length === 0 ? (
                        <div className="no-messages">No messages yet. Start the conversation!</div>
                    ) : (
                        messages.map((message) => (
                            <div key={message.id}
                                 className={`message ${message.isMyMessage ? 'my-message' : 'partner-message'}`}>
                                {!message.isMyMessage && (
                                    <div className="message-sender">{message.senderName}</div>
                                )}
                                <p className="message-text">{message.content}</p>
                                <span className="message-time">{formatDate(message.createdAt)}</span>
                            </div>
                        ))
                    )}
                    <div ref={messagesEndRef} />
                </div>

                <div className="chat-input-container">
                    <textarea
                        className="chat-input"
                        placeholder="Type your message..."
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
                        {connectionStatus === 'connected' ? 'Send' : 'Connecting...'}
                    </button>
                </div>
            </div>
        </>
    );
};

export default ChatConversationPage;