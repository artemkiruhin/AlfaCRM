import React, { useState, useEffect } from 'react';
import { ArrowLeft, Save, Trash, Eye, Edit, ThumbsUp, ThumbsDown } from 'lucide-react';
import { useNavigate, useParams } from 'react-router-dom';
import ReactMarkdown from 'react-markdown';
import './NewsEditPage.css';
import Header from "../../components/layout/header/Header";

const NewsEditPage = () => {
    // const navigate = useNavigate();
    // const { id } = useParams(); // Получаем ID новости из URL


    const id = 1
    const [isPreview, setIsPreview] = useState(false);
    const [editedTitle, setEditedTitle] = useState("");
    const [editedSubtitle, setEditedSubtitle] = useState("");
    const [editedContent, setEditedContent] = useState("");
    const [selectedDepartment, setSelectedDepartment] = useState("all");
    const [isImportant, setIsImportant] = useState(false);

    const newsItem = {
        id: 1,
        title: "Обновление корпоративной политики безопасности",
        subtitle: "Важные изменения в правилах безопасности",
        content: `Уважаемые коллеги! С 1 апреля вступают в силу обновленные правила корпоративной безопасности. Всем сотрудникам необходимо ознакомиться с новыми требованиями и пройти обязательное обучение до конца месяца.

### Основные изменения:
1. **Новые требования к паролям**.
2. **Обязательное двухфакторное подтверждение**.
3. **Ежеквартальные проверки безопасности**.

Подробнее можно узнать [здесь](#).`,
        createdAt: "2025-03-20T10:30:00",
        updatedAt: "2025-03-21T14:45:00",
        department: "IT отдел",
        author: "Иван Иванов",
    };

    useEffect(() => {
        setEditedTitle(newsItem.title);
        setEditedSubtitle(newsItem.subtitle);
        setEditedContent(newsItem.content);
        setSelectedDepartment(newsItem.department);
        setIsImportant(newsItem.isImportant);
    }, [id]);

    const handleSave = () => {
        const newsData = {
            title: editedTitle,
            subtitle: editedSubtitle,
            content: editedContent,
            department: selectedDepartment,
            isImportant: isImportant,
        };
        console.log("Сохранено:", newsData);
        alert("Изменения сохранены!");
        //navigate(`/news/${id}`);
    };

    const handleDelete = () => {
        if (window.confirm("Вы уверены, что хотите удалить эту новость?")) {
            console.log("Новость удалена");
            //navigate('/news');
        }
    };

    const togglePreview = () => {
        setIsPreview((prev) => !prev);
    };

    const handleLike = () => {
        console.log('Лайк');
    };

    const handleDislike = () => {
        console.log('Дизлайк');
    };

    return (
        <div className="news-edit-page">
            <Header />
            <div className="news-header">
                {/*<button className="back-button" onClick={() => navigate(-1)}>*/}
                <button className="back-button">
                    <ArrowLeft size={20} /> Назад
                </button>
                <div className="action-buttons">
                    <button className="preview-button" onClick={togglePreview}>
                        {isPreview ? <Edit size={18} /> : <Eye size={18} />}
                        {isPreview ? "Редактировать" : "Предпросмотр"}
                    </button>
                    <button className="save-button" onClick={handleSave}>
                        <Save size={18} /> Сохранить
                    </button>
                    <button className="delete-button" onClick={handleDelete}>
                        <Trash size={18} /> Удалить
                    </button>
                </div>
            </div>

            {isPreview ? (
                <div className="preview-mode">
                    <h1 className="news-title">{editedTitle}</h1>
                    {editedSubtitle && <h2 className="news-subtitle">{editedSubtitle}</h2>}

                    <div className="news-meta">
                        <span className="meta-item">
                            Создано: {new Date(newsItem.createdAt).toLocaleString()}
                        </span>
                        {newsItem.updatedAt && (
                            <span className="meta-item">
                                Обновлено: {new Date(newsItem.updatedAt).toLocaleString()}
                            </span>
                        )}
                        {isImportant && (
                            <span className="badge important-badge">Важно</span>
                        )}
                        <span className="meta-item">
                            Отдел: {selectedDepartment}
                        </span>
                        <span className="meta-item">
                            Автор: {newsItem.author}
                        </span>
                    </div>

                    <div className="news-content">
                        <ReactMarkdown>{editedContent}</ReactMarkdown>
                    </div>

                    <div className="reaction-buttons">
                        <button onClick={handleLike} className="like-button">
                            <ThumbsUp size={18} /> Лайк
                        </button>
                        <button onClick={handleDislike} className="dislike-button">
                            <ThumbsDown size={18} /> Дизлайк
                        </button>
                    </div>
                </div>
            ) : (
                <div className="edit-mode">
                    <label className="edit-label">
                        Заголовок
                        <input
                            type="text"
                            value={editedTitle}
                            onChange={(e) => setEditedTitle(e.target.value)}
                            placeholder="Введите заголовок новости"
                            className="edit-title-input"
                        />
                    </label>

                    <label className="edit-label">
                        Подзаголовок
                        <input
                            type="text"
                            value={editedSubtitle}
                            onChange={(e) => setEditedSubtitle(e.target.value)}
                            placeholder="Введите подзаголовок новости"
                            className="edit-subtitle-input"
                        />
                    </label>

                    <label className="edit-label">
                        Содержание
                        <textarea
                            value={editedContent}
                            onChange={(e) => setEditedContent(e.target.value)}
                            placeholder="Введите содержимое новости"
                            className="edit-content-textarea"
                        />
                    </label>

                    <label className="edit-label">
                        Отдел
                        <select
                            value={selectedDepartment}
                            onChange={(e) => setSelectedDepartment(e.target.value)}
                            className="edit-select"
                        >
                            <option value="all">Все</option>
                            <option value="hr">HR</option>
                            <option value="it">IT</option>
                            <option value="finance">Финансы</option>
                            <option value="marketing">Маркетинг</option>
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
            )}
        </div>
    );
};

export default NewsEditPage;