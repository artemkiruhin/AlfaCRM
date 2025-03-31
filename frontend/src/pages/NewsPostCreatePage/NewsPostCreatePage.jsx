import React, { useState, useEffect } from 'react';
import { ArrowLeft, Save, Eye, Edit } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import ReactMarkdown from 'react-markdown';
import './NewsPostCreatePage.css';
import Header from "../../components/layout/header/Header";
import { createPost } from '../../api-handlers/postsHandler';
import { getAllDepartmentsShort } from '../../api-handlers/departmentsHandler';
import { validateAdminOrPublisher } from "../../api-handlers/authHandler";

const NewsPostCreatePage = () => {
    const navigate = useNavigate();
    const [username, setUsername] = useState('');

    const [isPreview, setIsPreview] = useState(false);
    const [title, setTitle] = useState("");
    const [subtitle, setSubtitle] = useState("");
    const [content, setContent] = useState("");
    const [selectedDepartment, setSelectedDepartment] = useState("all");
    const [isImportant, setIsImportant] = useState(false);
    const [departments, setDepartments] = useState([]);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState(null);
    const [isAdminOrPublisher, setIsAdminOrPublisher] = useState(false);

    useEffect(() => {
        const fetchData = async () => {
            try {
                const [authCheck, deps] = await Promise.all([
                    validateAdminOrPublisher(),
                    getAllDepartmentsShort()
                ]);

                setIsAdminOrPublisher(authCheck);
                setDepartments(deps || []);

                const storedUsername = localStorage.getItem('username');
                if (storedUsername) {
                    setUsername(storedUsername);
                } else {
                    setError('Не удалось определить пользователя');
                    navigate('/login');
                }
            } catch (err) {
                console.error("Failed to fetch data:", err);
                setError("Не удалось загрузить необходимые данные");
            }
        };

        fetchData();
    }, [navigate]);

    const handleCreate = async () => {
        if (!title.trim()) {
            setError("Заголовок обязателен");
            return;
        }

        if (!content.trim()) {
            setError("Содержание обязательно");
            return;
        }

        setIsLoading(true);
        setError(null);

        try {
            const departmentId = selectedDepartment === "all" ? null : selectedDepartment;

            const postId = await createPost(
                title.trim(),
                subtitle.trim(),
                content.trim(),
                isImportant,
                departmentId
            );

            if (postId) {
                navigate(`/news/${postId}`);
            } else {
                setError("Не удалось создать пост");
            }
        } catch (err) {
            console.error("Post creation failed:", err);
            setError(err.message || "Ошибка при создании поста");
        } finally {
            setIsLoading(false);
        }
    };

    const togglePreview = () => {
        if (!title.trim()) {
            setError("Заголовок обязателен для предпросмотра");
            return;
        }
        setIsPreview(prev => !prev);
        setError(null);
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

    if (!isAdminOrPublisher) {
        return (
            <div className="news-edit-page">
                <Header />
                <div className="error-message">
                    У вас нет прав для создания новостей
                </div>
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
                        onClick={handleCreate}
                        disabled={isLoading || !username}
                    >
                        {isLoading ? "Создание..." : (
                            <>
                                <Save size={18} /> Создать
                            </>
                        )}
                    </button>
                </div>
            </div>

            {error && (
                <div className="error-message">
                    {error}
                </div>
            )}

            {isPreview ? (
                <div className="preview-mode">
                    <h1 className="news-title">{title}</h1>
                    {subtitle && <h2 className="news-subtitle">{subtitle}</h2>}

                    <div className="news-meta">
                        <span className="meta-item">
                            Автор: {username}
                        </span>
                        <span className="meta-item">
                            Дата создания: {formatDate(new Date().toISOString())}
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
                        <ReactMarkdown>{content}</ReactMarkdown>
                    </div>
                </div>
            ) : (
                <div className="edit-mode">
                    <label className="edit-label">
                        Заголовок*
                        <input
                            type="text"
                            value={title}
                            onChange={(e) => setTitle(e.target.value)}
                            placeholder="Введите заголовок новости"
                            className="edit-title-input"
                            required
                        />
                    </label>

                    <label className="edit-label">
                        Подзаголовок
                        <input
                            type="text"
                            value={subtitle}
                            onChange={(e) => setSubtitle(e.target.value)}
                            placeholder="Введите подзаголовок новости (необязательно)"
                            className="edit-subtitle-input"
                        />
                    </label>

                    <label className="edit-label">
                        Содержание*
                        <textarea
                            value={content}
                            onChange={(e) => setContent(e.target.value)}
                            placeholder="Введите содержимое новости (поддерживается Markdown)"
                            className="edit-content-textarea"
                            rows={10}
                            required
                        />
                    </label>

                    <div className="department-importance-container">
                        <label className="edit-label">
                            Отдел
                            <select
                                value={selectedDepartment}
                                onChange={(e) => setSelectedDepartment(e.target.value)}
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
                                onChange={(e) => setIsImportant(e.target.checked)}
                                className="edit-checkbox"
                            />
                            Важная новость
                        </label>
                    </div>

                    <div className="markdown-hint">
                        <p>Вы можете использовать Markdown для форматирования:</p>
                        <ul>
                            <li>**Жирный текст**</li>
                            <li>*Курсив*</li>
                            <li># Заголовок 1</li>
                            <li>## Заголовок 2</li>
                            <li>[Ссылка](https://example.com)</li>
                            <li>- Список</li>
                            <li>1. Нумерованный список</li>
                            <li>![Альт текст](url-изображения)</li>
                            <li>&gt; Цитата</li>
                            <li>``` Код ```</li>
                        </ul>
                    </div>
                </div>
            )}
        </div>
    );
};

export default NewsPostCreatePage;