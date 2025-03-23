import React from 'react';
import { Link, useParams } from 'react-router-dom';
import "./ChatConversationPage.css"
import Header from "../../components/layout/header/Header";

const ChatConversationPage = () => {
    //const { chatId } = useParams(); // Получаем ID беседы из URL

    const chatId = 1

    const title = "Chat Conversation";

    const messages = [
        { id: 1, text: 'Привет! Как дела?', time: '10:15', isMyMessage: false },
        { id: 2, text: 'Привет! Все отлично, спасибо!', time: '10:16', isMyMessage: true },{ id: 1, text: 'Привет! Как дела?', time: '10:15', isMyMessage: false },
        { id: 2, text: 'Привет! Все отлично, спасибо!', time: '10:16', isMyMessage: true },{ id: 1, text: 'Привет! Как дела?', time: '10:15', isMyMessage: false },
        { id: 2, text: 'Привет! Все отлично, спасибо!', time: '10:16', isMyMessage: true },{ id: 1, text: 'Привет! Как дела?', time: '10:15', isMyMessage: false },
        { id: 2, text: 'Привет! Все отлично, спасибо!', time: '10:16', isMyMessage: true },{ id: 1, text: 'Привет! Как дела?', time: '10:15', isMyMessage: false },
        { id: 2, text: 'Привет! Все отлично, спасибо!', time: '10:16', isMyMessage: true },{ id: 1, text: 'Привет! Как дела?', time: '10:15', isMyMessage: false },
        { id: 2, text: 'Привет! Все отлично, спасибо!', time: '10:16', isMyMessage: true },{ id: 1, text: 'Привет! Как дела?', time: '10:15', isMyMessage: false },
        { id: 2, text: 'Привет! Все отлично, спасибо!', time: '10:16', isMyMessage: true },{ id: 1, text: 'Привет! Как дела?', time: '10:15', isMyMessage: false },
        { id: 2, text: 'Привет! Все отлично, спасибо!', time: '10:16', isMyMessage: true },{ id: 1, text: 'Привет! Как дела?', time: '10:15', isMyMessage: false },
        { id: 2, text: 'Привет! Все отлично, спасибо!', time: '10:16', isMyMessage: true },{ id: 1, text: 'Привет! Как дела?', time: '10:15', isMyMessage: false },
        { id: 2, text: 'Привет! Все отлично, спасибо!', time: '10:16', isMyMessage: true },{ id: 1, text: 'Привет! Как дела?', time: '10:15', isMyMessage: false },
        { id: 2, text: 'Привет! Все отлично, спасибо!', time: '10:16', isMyMessage: true },{ id: 1, text: 'Привет! Как дела?', time: '10:15', isMyMessage: false },
        { id: 2, text: 'Привет! Все отлично, спасибо!', time: '10:16', isMyMessage: true },{ id: 1, text: 'Привет! Как дела?', time: '10:15', isMyMessage: false },
        { id: 2, text: 'Привет! Все отлично, спасибо!', time: '10:16', isMyMessage: true },{ id: 1, text: 'Привет! Как дела?', time: '10:15', isMyMessage: false },
        { id: 2, text: 'Привет! Все отлично, спасибо!', time: '10:16', isMyMessage: true },{ id: 1, text: 'Привет! Как дела?', time: '10:15', isMyMessage: false },
        { id: 2, text: 'Привет! Все отлично, спасибо!', time: '10:16', isMyMessage: true },{ id: 1, text: 'Привет! Как дела?', time: '10:15', isMyMessage: false },
        { id: 2, text: 'Привет! Все отлично, спасибо!', time: '10:16', isMyMessage: true },{ id: 1, text: 'Привет! Как дела?', time: '10:15', isMyMessage: false },
        { id: 2, text: 'Привет! Все отлично, спасибо!', time: '10:16', isMyMessage: true },{ id: 1, text: 'Привет! Как дела?', time: '10:15', isMyMessage: false },
        { id: 2, text: 'Привет! Все отлично, спасибо!', time: '10:16', isMyMessage: true },{ id: 1, text: 'Привет! Как дела?', time: '10:15', isMyMessage: false },
        { id: 2, text: 'Привет! Все отлично, спасибо!', time: '10:16', isMyMessage: true },{ id: 1, text: 'Привет! Как дела?', time: '10:15', isMyMessage: false },
        { id: 2, text: 'Привет! Все отлично, спасибо!', time: '10:16', isMyMessage: true },{ id: 1, text: 'Привет! Как дела?', time: '10:15', isMyMessage: false },
        { id: 2, text: 'Привет! Все отлично, спасибо!', time: '10:16', isMyMessage: true },
    ];

    return (
        <>
            <Header/>
            <div className="chat-conversation-container">
                <div className="chat-header">
                    {/*<Link to="/" className="back-button">*/}
                    {/*    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">*/}
                    {/*        <path d="M15 18L9 12L15 6" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>*/}
                    {/*    </svg>*/}
                    {/*    Назад*/}
                    {/*</Link>*/}
                    <div className="back-button">
                        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                            <path d="M15 18L9 12L15 6" stroke="currentColor" strokeWidth="2" strokeLinecap="round"
                                  strokeLinejoin="round"/>
                        </svg>
                        Назад
                    </div>
                    <div className="chat-partner">
                        {/*<img src="https://via.placeholder.com/40" alt="Аватар" className="chat-partner-avatar"/>*/}
                        <h2 className="chat-partner-name">Иван Иванов</h2>
                    </div>
                </div>

                <div className="chat-messages">
                    {messages.map((message) => (
                        <div key={message.id}
                             className={`message ${message.isMyMessage ? 'my-message' : 'partner-message'}`}>
                            <p className="message-text">{message.text}</p>
                            <span className="message-time">{message.time}</span>
                        </div>
                    ))}
                </div>

                <div className="chat-input-container">
                    <textarea className="chat-input" placeholder="Введите сообщение..."/>
                    <button className="send-button">Отправить</button>
                </div>
            </div>
        </>
    );
};

export default ChatConversationPage;