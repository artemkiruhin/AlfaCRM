import React from 'react';
import { Link } from 'react-router-dom';
import GradientCircle from '../../components/extensions/GradientCircle'; // Импортируем компонент
import "./ChatListPage.css"
import Header from "../../components/layout/header/Header";

const ChatList = () => {
    const chats = [
        { id: 1, name: 'Иван Иванов', lastMessage: 'Привет! Как дела?', time: '10:15', isOnline: true, unreadCount: 2 },
        { id: 2, name: 'Анна Петрова', lastMessage: 'Давай встретимся завтра.', time: 'Вчера', isOnline: false, unreadCount: 0 },
    ];

    return (
        <>

            <div className="chat-list-container">
                <Header title={"Мои чаты"} info={`Всего: ${chats.length}`} />
                {/*<h1 className="chat-list-title">Беседы</h1>*/}
                <div className="chat-list">
                    {chats.map((chat) => (
                        // <Link to={`/chat/${chat.id}`} key={chat.id} className="chat-item">
                        //     <div className="chat-avatar">
                        //         <GradientCircle name={chat.name} size={40} />
                        //     </div>
                        //     <div className="chat-info">
                        //         <h3 className="chat-name">{chat.name}</h3>
                        //         <p className="chat-last-message">{chat.lastMessage}</p>
                        //         <span className="chat-time">{chat.time}</span>
                        //     </div>
                        //     {chat.isOnline && <div className="chat-status online" />}
                        //     {chat.unreadCount > 0 && (
                        //         <div className="chat-notification">
                        //             {chat.unreadCount}
                        //         </div>
                        //     )}
                        // </Link>
                        <div className="chat-item">
                            <div className="chat-avatar">
                                <GradientCircle name={chat.name} size={40}/>
                            </div>
                            <div className="chat-info">
                                <h3 className="chat-name">{chat.name}</h3>
                                <p className="chat-last-message">{chat.lastMessage}</p>
                                <span className="chat-time">{chat.time}</span>
                            </div>
                            {/*{chat.isOnline && <div className="chat-status online" />}*/}
                            {/*{chat.unreadCount > 0 && (*/}
                            {/*    <div className="chat-notification">*/}
                            {/*        {chat.unreadCount}*/}
                            {/*    </div>*/}
                            {/*)}*/}
                        </div>
                    ))}
                </div>
            </div>
        </>
    );
};

export default ChatList;