import React, { useState, useEffect } from 'react';
import { ArrowLeft, Save, Trash, Eye, Edit, ThumbsUp, ThumbsDown } from 'lucide-react';
import { useNavigate, useParams } from 'react-router-dom';
import ReactMarkdown from 'react-markdown';
import './NewsEditPage.css';
import Header from "../../components/layout/header/Header";
import { getPostById, editPost, deletePost, reactPost, deleteAllReactsByPost } from "../../api-handlers/postsHandler";
import { getAllDepartments } from "../../api-handlers/departmentsHandler";
import { validateAdminOrPublisher } from "../../api-handlers/authHandler";

const NewsEditPage = () => {
    const navigate = useNavigate();
    const { id } = useParams();

    const [isPreview, setIsPreview] = useState(false);
    const [editedTitle, setEditedTitle] = useState("");
    const [editedSubtitle, setEditedSubtitle] = useState("");
    const [editedContent, setEditedContent] = useState("");
    const [selectedDepartment, setSelectedDepartment] = useState("all");
    const [isImportant, setIsImportant] = useState(false);
    const [departments, setDepartments] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState(null);
    const [isAdminOrAuthor, setIsAdminOrAuthor] = useState(false);
    const [userReaction, setUserReaction] = useState(null);
    const [reactions, setReactions] = useState([]);
    const [initialData, setInitialData] = useState(null);

    const currentUserId = localStorage.getItem('uid');
    const isAuthenticated = !!currentUserId;

    useEffect(() => {
        const fetchData = async () => {
            try {
                const [authCheck, postData, deps] = await Promise.all([
                    validateAdminOrPublisher(),
                    getPostById(id),
                    getAllDepartments(true)
                ]);

                setIsAdminOrAuthor(authCheck);
                setDepartments(deps || []);
                setInitialData(postData);

                if (postData) {
                    setEditedTitle(postData.title);
                    setEditedSubtitle(postData.subtitle || "");
                    setEditedContent(postData.content);
                    setIsImportant(postData.isImportant || false);
                    setSelectedDepartment(postData.department?.id || "all");
                    setReactions(postData.reactions || []);

                    if (currentUserId && postData.reactions) {
                        const userReact = postData.reactions.find(r => r.senderId === currentUserId);
                        if (userReact) {
                            setUserReaction(userReact.type);
                        }
                    }
                }
            } catch (err) {
                setError(err.message);
                console.error('Error fetching data:', err);
            } finally {
                setIsLoading(false);
            }
        };

        fetchData();
    }, [id, currentUserId]);

    const handleSave = async () => {
        if (!editedTitle.trim()) {
            setError("Заголовок обязателен");
            return;
        }

        if (!editedContent.trim()) {
            setError("Содержание обязательно");
            return;
        }

        setIsLoading(true);
        setError(null);

        try {
            const departmentId = selectedDepartment === "all" ? null : selectedDepartment;

            const changes = {
                postId: id,
                title: editedTitle.trim(),
                subtitle: editedSubtitle.trim(),
                content: editedContent.trim(),
                isImportant: isImportant,
                departmentId: departmentId,
                editDepartment: true
            };

            const result = await editPost(
                changes.postId,
                changes.title,
                changes.subtitle,
                changes.content,
                changes.isImportant,
                changes.departmentId,
                changes.editDepartment
            );

            if (result?.id) {
                navigate(`/news/${id}`);
            }
        } catch (err) {
            console.error("Error saving post:", err);
            setError(err.message || "Ошибка при сохранении");
        } finally {
            setIsLoading(false);
        }
    };

    const handleDelete = async () => {
        if (window.confirm("Вы уверены, что хотите удалить эту новость?")) {
            setIsLoading(true);
            try {
                const result = await deletePost(id);
                if (result) {
                    navigate('/news');
                } else {
                    setError("Не удалось удалить пост");
                }
            } catch (err) {
                console.error("Error deleting post:", err);
                setError("Ошибка при удалении: " + err.message);
            } finally {
                setIsLoading(false);
            }
        }
    };

    const handleReaction = async (type) => {
        try {
            if (userReaction === type) {
                const result = await deleteAllReactsByPost(id);
                if (result) {
                    setUserReaction(null);
                    setReactions(prev => prev.filter(r => r.senderId !== currentUserId));
                }
            } else {
                const result = await reactPost(id, type);
                if (result) {
                    setUserReaction(type);
                    setReactions(prev => {
                        const filtered = prev.filter(r => r.senderId !== currentUserId);
                        return [
                            ...filtered,
                            {
                                id: result,
                                postId: id,
                                senderId: currentUserId,
                                type: type
                            }
                        ];
                    });
                }
            }
        } catch (err) {
            console.error('Error updating reaction:', err);
        }
    };

    const togglePreview = () => {
        if (!editedTitle.trim()) {
            setError("Заголовок обязателен для предпросмотра");
            return;
        }
        setIsPreview(prev => !prev);
        setError(null);
    };

    const handleDepartmentChange = (e) => {
        setSelectedDepartment(e.target.value === "all" ? "all" : e.target.value);
    };

    const handleImportanceChange = (e) => {
        setIsImportant(e.target.checked);
    };

    const countReactions = (type) => {
        return reactions.filter(r => r.type === type).length;
    };

    const formatDate = (dateString) => {
        if (!dateString) return "";
        const options = {
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        };
        return new Date(dateString).toLocaleDateString('ru-RU', options);
    };

    if (isLoading) {
        return (
            <div className="news-edit-page">
                <Header />
                <div className="loading">Загрузка...</div>
            </div>
        );
    }

    if (error && !initialData) {
        return (
            <div className="news-edit-page">
                <Header />
                <div className="error">Ошибка: {error}</div>
                <button onClick={() => navigate(-1)} className="back-button">
                    <ArrowLeft size={20} /> Назад
                </button>
            </div>
        );
    }

    return (
        <div className="news-edit-page">
            <Header />
            <div className="news-header">
                <button className="back-button" onClick={() => navigate(-1)}>
                    <ArrowLeft size={20} /> Назад
                </button>
                <div className="action-buttons">
                    <button className="preview-button" onClick={togglePreview}>
                        {isPreview ? <Edit size={18} /> : <Eye size={18} />}
                        {isPreview ? "Редактировать" : "Предпросмотр"}
                    </button>
                    <button
                        className="save-button"
                        onClick={handleSave}
                        disabled={isLoading}
                    >
                        {isLoading ? "Сохранение..." : (
                            <>
                                <Save size={18} /> Сохранить
                            </>
                        )}
                    </button>
                    {isAdminOrAuthor && (
                        <button
                            className="danger-action-button"
                            onClick={() => handleDelete()}
                            disabled={isLoading}
                        >
                            <Trash size={18}/> Удалить
                        </button>
                    )}
                </div>
            </div>

            {error && (
                <div className="error-message">
                    {error}
                </div>
            )}

            {isPreview ? (
                <div className="preview-mode">
                    <h1 className="news-title">{editedTitle}</h1>
                    {editedSubtitle && <h2 className="news-subtitle">{editedSubtitle}</h2>}

                    <div className="news-meta">
                        <span className="meta-item">
                            Последнее обновление: {formatDate(new Date().toISOString())}
                        </span>
                        {isImportant && (
                            <span className="badge important-badge">Важно</span>
                        )}
                        <span className="meta-item">
                            Отдел: {selectedDepartment === "all"
                            ? "Все"
                            : departments.find(d => d.id === selectedDepartment)?.name || "Неизвестно"}
                        </span>
                    </div>

                    <div className="news-content">
                        <ReactMarkdown>{editedContent}</ReactMarkdown>
                    </div>

                    <div className="reaction-buttons">
                        <button
                            onClick={() => handleReaction(0)}
                            className={`like-button ${userReaction === 0 ? 'active' : ''}`}
                            disabled={!isAuthenticated}
                        >
                            <ThumbsUp size={18}/> Лайк ({countReactions(0)})
                        </button>
                        <button
                            onClick={() => handleReaction(1)}
                            className={`dislike-button ${userReaction === 1 ? 'active' : ''}`}
                            disabled={!isAuthenticated}
                        >
                            <ThumbsDown size={18}/> Дизлайк ({countReactions(1)})
                        </button>
                    </div>
                </div>
            ) : (
                <div className="edit-mode">
                    <label className="edit-label">
                        Заголовок*
                        <input
                            type="text"
                            value={editedTitle}
                            onChange={(e) => setEditedTitle(e.target.value)}
                            placeholder="Введите заголовок новости"
                            className="edit-title-input"
                            required
                        />
                    </label>

                    <label className="edit-label">
                        Подзаголовок
                        <input
                            type="text"
                            value={editedSubtitle}
                            onChange={(e) => setEditedSubtitle(e.target.value)}
                            placeholder="Введите подзаголовок новости (необязательно)"
                            className="edit-subtitle-input"
                        />
                    </label>

                    <label className="edit-label">
                        Содержание*
                        <textarea
                            value={editedContent}
                            onChange={(e) => setEditedContent(e.target.value)}
                            placeholder="Введите содержимое новости (поддерживается Markdown)"
                            className="edit-content-textarea"
                            rows={10}
                            required
                        />
                    </label>

                    <label className="edit-label">
                        Отдел
                        <select
                            value={selectedDepartment}
                            onChange={handleDepartmentChange}
                            className="edit-select"
                        >
                            <option value="all">Все отделы</option>
                            {departments.map((dep) => (
                                <option key={dep.id} value={dep.id}>
                                    {dep.name}
                                </option>
                            ))}
                        </select>
                    </label>

                    <label className="edit-label checkbox-label">
                        <input
                            type="checkbox"
                            checked={isImportant}
                            onChange={handleImportanceChange}
                            className="edit-checkbox"
                        />
                        Важная новость
                    </label>

                    <div className="markdown-hint">
                        <p>Вы можете использовать Markdown для форматирования:</p>
                        <ul>
                            <li>**Жирный текст**</li>
                            <li>*Курсив*</li>
                            <li># Заголовок 1</li>
                            <li>## Заголовок 2</li>
                            <li>[Ссылка](https://example.com)</li>
                            <li>- Список</li>
                        </ul>
                    </div>
                </div>
            )}
        </div>
    );
};

export default NewsEditPage;