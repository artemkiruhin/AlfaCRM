import React, { useState } from 'react';
import { ArrowLeft, Edit, ThumbsUp, ThumbsDown } from 'lucide-react';
import { useNavigate, useParams } from 'react-router-dom';
import ReactMarkdown from 'react-markdown';
import './NewsPostPage.css';
import Header from "../../components/layout/header/Header";

const NewsPostPage = () => {
    //const navigate = useNavigate();
    //const { id } = useParams();
    const id = 1
    const newsItem = {
        id: 1,
        title: "Обновление корпоративной политики безопасности",
        subtitle: "Важные изменения в правилах безопасности",
        createdAt: "2025-03-20T10:30:00",
        updatedAt: "2025-03-21T14:45:00",
        isImportant: true,
        department: "IT отдел",
        author: "Иван Иванов",
        content: `Уважаемые коллеги! С 1 апреля вступают в силу обновленные правила корпоративной безопасности. Всем сотрудникам необходимо ознакомиться с новыми требованиями и пройти обязательное обучение до конца месяца.

### Основные изменения:
1. **Новые требования к паролям**.
2. **Обязательное двухфакторное подтверждение**.
3. **Ежеквартальные проверки безопасности**.

Подробнее можно узнать [здесь](#).`,
    };

    const [comments, setComments] = useState([
        {
            id: 1,
            text: "Спасибо за информацию!",
            author: "Петр Петров",
            createdAt: "2025-03-20T11:00:00",
        },
        {
            id: 2,
            text: "Когда будет обучение?",
            author: "Анна Сидорова",
            createdAt: "2025-03-20T12:30:00",
        },
    ]);

    const [newComment, setNewComment] = useState('');

    //stub
    const isAdminOrAuthor = true

    const handleEditNews = () => {
        //navigate(`/news/${newsItem.id}/edit`); // Переход на страницу редактирования
    };

    const handleCommentSubmit = (e) => {
        e.preventDefault();
        if (newComment.trim()) {
            const comment = {
                id: comments.length + 1,
                text: newComment,
                author: "Текущий пользователь",
                createdAt: new Date().toISOString(),
            };
            setComments([...comments, comment]);
            setNewComment('');
        }
    };

    const handleLike = () => {
        console.log('Лайк');
    };

    const handleDislike = () => {
        console.log('Дизлайк');
    };

    const handleEdit = () => {
        //navigate(`/news/${id}/edit`);
    };

    return (
        <div className="news-post-page">
            <Header />
            <div className="news-header">
                {/*<button className="back-button" onClick={() => navigate(-1)}>*/}
                <button className="back-button">
                    <ArrowLeft size={20} /> Назад
                </button>
                {isAdminOrAuthor && (
                    <button className="edit-news-button" onClick={handleEditNews}>
                        <Edit size={18} /> Редактировать
                    </button>
                )}
            </div>
            <h1 className="news-title">{newsItem.title}</h1>
            {newsItem.subtitle && <h2 className="news-subtitle">{newsItem.subtitle}</h2>}

            <div className="news-meta">
                <span className="meta-item">
                    Создано: {new Date(newsItem.createdAt).toLocaleString()}
                </span>
                {newsItem.updatedAt && (
                    <span className="meta-item">
                        Обновлено: {new Date(newsItem.updatedAt).toLocaleString()}
                    </span>
                )}
                {newsItem.isImportant && (
                    <span className="badge important-badge">Важно</span>
                )}
                <span className="meta-item">
                    Отдел: {newsItem.department}
                </span>
                <span className="meta-item">
                    Автор: {newsItem.author}
                </span>
            </div>

            <div className="news-content">
                <ReactMarkdown>{newsItem.content}</ReactMarkdown>
            </div>

            <div className="reaction-buttons">
                <button onClick={handleLike} className="like-button">
                    <ThumbsUp size={18} /> Лайк
                </button>
                <button onClick={handleDislike} className="dislike-button">
                    <ThumbsDown size={18} /> Дизлайк
                </button>
            </div>

            <div className="comments-section">
                <h3>Комментарии ({comments.length})</h3>
                {comments
                    .slice()
                    .reverse()
                    .map((comment) => (
                        <div key={comment.id} className="comment">
                            <div className="comment-header">
                                <span className="comment-author">{comment.author}</span>
                                <span className="comment-date">
                    {new Date(comment.createdAt).toLocaleString()}
                </span>
                            </div>
                            <div className="comment-text">{comment.text}</div>
                        </div>
                    ))
                }

                <form onSubmit={handleCommentSubmit} className="comment-form">
                    <textarea
                        value={newComment}
                        onChange={(e) => setNewComment(e.target.value)}
                        placeholder="Напишите комментарий..."
                        required
                    />
                    <button type="submit">Отправить</button>
                </form>
            </div>

            <button className="edit-button" onClick={handleEdit}>
                <Edit size={18} /> Редактировать
            </button>
        </div>
    );
};

export default NewsPostPage;