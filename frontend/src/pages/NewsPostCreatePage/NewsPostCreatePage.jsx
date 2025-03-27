import React, { useState, useEffect } from 'react';
import { ArrowLeft, Save, Eye, Edit } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import ReactMarkdown from 'react-markdown';
import './NewsPostCreatePage.css';
import Header from "../../components/layout/header/Header";
import { createPost } from '../../api-handlers/postsHandler';
import { getAllDepartments } from '../../api-handlers/departmentsHandler';

const NewsPostCreatePage = () => {
    const navigate = useNavigate();

    const [isPreview, setIsPreview] = useState(false);
    const [newTitle, setNewTitle] = useState("");
    const [newSubtitle, setNewSubtitle] = useState("");
    const [newContent, setNewContent] = useState("");
    const [selectedDepartment, setSelectedDepartment] = useState(null);
    const [isImportant, setIsImportant] = useState(false);
    const [departments, setDepartments] = useState([]);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchDepartments = async () => {
            try {
                const deps = await getAllDepartments(true);
                setDepartments(deps || []);
            } catch (err) {
                console.error("Failed to fetch departments:", err);
                setError("Не удалось загрузить отделы");
            }
        };

        fetchDepartments();
    }, []);

    const handleCreate = async () => {
        if (!newTitle.trim()) {
            setError("Заголовок обязателен");
            return;
        }

        if (!newContent.trim()) {
            setError("Содержание обязательно");
            return;
        }

        setIsLoading(true);
        setError(null);

        try {
            const postId = await createPost(
                newTitle.trim(),
                newSubtitle.trim(),
                newContent.trim(),
                isImportant,
                selectedDepartment === "all" ? null : selectedDepartment
            );

            if (postId) {
                navigate(`/news/${postId}`);
            } else {
                setError("Не удалось создать пост");
            }
        } catch (err) {
            console.error("Post creation failed:", err);
            setError("Ошибка при создании поста");
        } finally {
            setIsLoading(false);
        }
    };

    const togglePreview = () => {
        if (!newTitle.trim()) {
            setError("Заголовок обязателен для предпросмотра");
            return;
        }
        setIsPreview((prev) => !prev);
        setError(null);
    };

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
                        disabled={isLoading}
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
                    <h1 className="news-title">{newTitle}</h1>
                    {newSubtitle && <h2 className="news-subtitle">{newSubtitle}</h2>}

                    <div className="news-meta">
                        <span className="meta-item">
                            Создано: {new Date().toLocaleString()}
                        </span>
                        {isImportant && (
                            <span className="badge important-badge">Важно</span>
                        )}
                        <span className="meta-item">
                            Отдел: {selectedDepartment === "all" || !selectedDepartment
                            ? "Все"
                            : departments.find(d => d.id === selectedDepartment)?.name || "Неизвестно"}
                        </span>
                    </div>

                    <div className="news-content">
                        <ReactMarkdown>{newContent}</ReactMarkdown>
                    </div>
                </div>
            ) : (
                <div className="edit-mode">
                    <label className="edit-label">
                        Заголовок*
                        <input
                            type="text"
                            value={newTitle}
                            onChange={(e) => setNewTitle(e.target.value)}
                            placeholder="Введите заголовок новости"
                            className="edit-title-input"
                            required
                        />
                    </label>

                    <label className="edit-label">
                        Подзаголовок
                        <input
                            type="text"
                            value={newSubtitle}
                            onChange={(e) => setNewSubtitle(e.target.value)}
                            placeholder="Введите подзаголовок новости (необязательно)"
                            className="edit-subtitle-input"
                        />
                    </label>

                    <label className="edit-label">
                        Содержание*
                        <textarea
                            value={newContent}
                            onChange={(e) => setNewContent(e.target.value)}
                            placeholder="Введите содержимое новости (поддерживается Markdown)"
                            className="edit-content-textarea"
                            rows={10}
                            required
                        />
                    </label>

                    <label className="edit-label">
                        Отдел
                        <select
                            value={selectedDepartment || "all"}
                            onChange={(e) => setSelectedDepartment(e.target.value === "all" ? null : e.target.value)}
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

export default NewsPostCreatePage;