import React, {useEffect, useState} from 'react';
import { ArrowLeft, Edit, ThumbsUp, ThumbsDown } from 'lucide-react';
import { useNavigate, useParams } from 'react-router-dom';
import ReactMarkdown from 'react-markdown';
import './NewsPostPage.css';
import Header from "../../components/layout/header/Header";
import {validateAdminOrPublisher} from "../../api-handlers/authHandler";
import {
    createComment,
    deleteAllReactsByPost,
    getPostById,
    reactPost
} from "../../api-handlers/postsHandler";

const NewsPostPage = () => {
    const navigate = useNavigate();
    const {id} = useParams();

    const currentUserId = localStorage.getItem('uid');
    const currentUsername = localStorage.getItem('username');
    const isAuthenticated = !!currentUserId;

    const [isAdminOrAuthor, setIsAdminOrAuthor] = useState(false);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState(null);

    const [newsItem, setNewsItem] = useState({
        id: "",
        title: "",
        subtitle: "",
        createdAt: "",
        modifiedAt: "",
        isImportant: false,
        department: "",
        author: "",
        content: "",
        reactions: [],
    });

    const [comments, setComments] = useState([]);
    const [newComment, setNewComment] = useState('');
    const [userReaction, setUserReaction] = useState(null);

    useEffect(() => {
        const fetchData = async () => {
            try {
                const isAuthorized = await validateAdminOrPublisher();
                setIsAdminOrAuthor(isAuthorized);

                const response = await getPostById(id);
                if (response) {
                    setNewsItem({
                        id: response.id,
                        title: response.title,
                        subtitle: response.subtitle,
                        content: response.content,
                        createdAt: response.createdAt,
                        modifiedAt: response.modifiedAt,
                        isImportant: response.isImportant,
                        author: response.publisher.username,
                        department: response.department?.name || null,
                        reactions: response.reactions || []
                    });
                    setComments(response.comments || []);

                    if (currentUserId && response.reactions) {
                        const userReact = response.reactions.find(r => r.senderId === currentUserId);
                        if (userReact) {
                            setUserReaction(userReact.type);
                        }
                    }
                }
            } catch (err) {
                setError(err.message);
                console.error('Error fetching post:', err);
            } finally {
                setIsLoading(false);
            }
        };

        fetchData();
    }, [id, currentUserId]);

    const handleBackClick = () => {
        navigate("/news");
    };

    const handleEditNews = () => {
        navigate(`/news/edit/${id}`);
    };

    const handleCommentSubmit = async (e) => {
        e.preventDefault();
        const commentText = newComment.trim();
        if (!commentText) return;
        try {
            const commentId = await createComment(commentText, id);

            if (commentId) {
                const comment = {
                    id: commentId,
                    content: commentText,
                    sender: {
                        username: currentUsername || "Аноним",
                        id: currentUserId
                    },
                    createdAt: new Date().toISOString()
                };

                setComments([...comments, comment]);
                setNewComment('');

                console.log('Comment added successfully');
            }
        } catch (err) {
            console.error('Error adding comment:', err);
        }
    };

    const handleReaction = async (type) => {
        try {
            if (userReaction === type) {
                const result = await deleteAllReactsByPost(id, type);
                if (result) {
                    setUserReaction(null);
                    setNewsItem(prev => ({
                        ...prev,
                        reactions: prev.reactions.filter(r => r.senderId !== currentUserId)
                    }));
                }
            } else {
                const result = await reactPost(id, type);
                if (result) {
                    setUserReaction(type);
                    setNewsItem(prev => {
                        const filtered = prev.reactions.filter(r => r.senderId !== currentUserId);
                        return {
                            ...prev,
                            reactions: [
                                ...filtered,
                                {
                                    id: result,
                                    postId: id,
                                    senderId: currentUserId,
                                    type: type,
                                    sender: {
                                        username: currentUsername
                                    }
                                }
                            ]
                        };
                    });
                }
            }
        } catch (err) {
            console.error('Error updating reaction:', err);
        }
    };

    const formatDate = (dateString) => {
        const options = {
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        };
        return new Date(dateString).toLocaleDateString('ru-RU', options);
    };

    const countReactions = (type) => {
        return newsItem.reactions.filter(r => r.type === type).length;
    };

    if (isLoading) {
        return <div className="loading">Загрузка...</div>;
    }

    if (error) {
        return <div className="error">Ошибка: {error}</div>;
    }

    return (
        <div className="news-post-page">
            <Header/>
            <div className="news-header">
                <button className="back-button" onClick={handleBackClick}>
                    <ArrowLeft size={20}/> Назад
                </button>
                {isAdminOrAuthor && (
                    <button className="edit-news-button" onClick={handleEditNews}>
                        <Edit size={18}/> Редактировать
                    </button>
                )}
            </div>

            <div className="news-content-container">
                <h1 className="news-title">{newsItem.title}</h1>
                {newsItem.subtitle && <h2 className="news-subtitle">{newsItem.subtitle}</h2>}

                <div className="news-meta">
                    <span className="meta-item">
                        Создано: {formatDate(newsItem.createdAt)}
                    </span>
                    {newsItem.modifiedAt && (
                        <span className="meta-item">
                            Обновлено: {formatDate(newsItem.modifiedAt)}
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
                    <button
                        onClick={() => handleReaction(0)}
                        className={`like-button ${userReaction === 0 ? 'active' : ''}`}
                        disabled={!isAuthenticated}
                    >
                        <ThumbsUp size={18}/>
                        Лайк ({countReactions(0)})
                    </button>
                    <button
                        onClick={() => handleReaction(1)}
                        className={`dislike-button ${userReaction === 1 ? 'active' : ''}`}
                        disabled={!isAuthenticated}
                    >
                        <ThumbsDown size={18}/>
                        Дизлайк ({countReactions(1)})
                    </button>
                </div>

                <div className="comments-section">
                    <h3>Комментарии ({comments.length})</h3>

                    {comments.length > 0 ? (
                        comments
                            .slice()
                            .reverse()
                            .map((comment) => (
                                <div key={comment.id} className="comment">
                                    <div className="comment-header">
                                        <span className="comment-author">{comment.sender?.username || 'Аноним'}</span>
                                        <span className="comment-date">
                                            {formatDate(comment.createdAt)}
                                        </span>
                                    </div>
                                    <div className="comment-text">{comment.content}</div>
                                </div>
                            ))
                    ) : (
                        <p className="no-comments">Пока нет комментариев. Будьте первым!</p>
                    )}

                    {isAuthenticated ? (
                        <form onSubmit={handleCommentSubmit} className="comment-form">
                            <textarea
                                value={newComment}
                                onChange={(e) => setNewComment(e.target.value)}
                                placeholder="Напишите комментарий..."
                                required
                            />
                            <button type="submit">Отправить</button>
                        </form>
                    ) : (
                        <p className="login-to-comment">Войдите, чтобы оставить комментарий</p>
                    )}
                </div>
            </div>
        </div>
    );
};

export default NewsPostPage;