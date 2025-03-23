import React, { useState } from 'react';
import { ArrowLeft, Save, Eye, Edit } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import ReactMarkdown from 'react-markdown';
import './NewsPostCreatePage.css';
import Header from "../../components/layout/header/Header";

const NewsPostCreatePage = () => {
    // const navigate = useNavigate();

    const [isPreview, setIsPreview] = useState(false);
    const [newTitle, setNewTitle] = useState("");
    const [newSubtitle, setNewSubtitle] = useState("");
    const [newContent, setNewContent] = useState("");
    const [selectedDepartment, setSelectedDepartment] = useState("all");
    const [isImportant, setIsImportant] = useState(false);

    const currentDate = new Date().toISOString();
    const currentUser = "Текущий пользователь";

    const handleCreate = () => {
        const newsData = {
            title: newTitle,
            subtitle: newSubtitle,
            content: newContent,
            department: selectedDepartment,
            isImportant: isImportant,
            createdAt: currentDate,
            author: currentUser
        };
        console.log("Создано:", newsData);
        alert("Новость успешно создана!");
        // navigate('/news');
    };

    const togglePreview = () => {
        setIsPreview((prev) => !prev);
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
                    <button className="save-button" onClick={handleCreate}>
                        <Save size={18} /> Создать
                    </button>
                </div>
            </div>

            {isPreview ? (
                <div className="preview-mode">
                    <h1 className="news-title">{newTitle}</h1>
                    {newSubtitle && <h2 className="news-subtitle">{newSubtitle}</h2>}

                    <div className="news-meta">
                        <span className="meta-item">
                            Создано: {new Date(currentDate).toLocaleString()}
                        </span>
                        {isImportant && (
                            <span className="badge important-badge">Важно</span>
                        )}
                        <span className="meta-item">
                            Отдел: {selectedDepartment}
                        </span>
                        <span className="meta-item">
                            Автор: {currentUser}
                        </span>
                    </div>

                    <div className="news-content">
                        <ReactMarkdown>{newContent}</ReactMarkdown>
                    </div>
                </div>
            ) : (
                <div className="edit-mode">
                    <label className="edit-label">
                        Заголовок
                        <input
                            type="text"
                            value={newTitle}
                            onChange={(e) => setNewTitle(e.target.value)}
                            placeholder="Введите заголовок новости"
                            className="edit-title-input"
                        />
                    </label>

                    <label className="edit-label">
                        Подзаголовок
                        <input
                            type="text"
                            value={newSubtitle}
                            onChange={(e) => setNewSubtitle(e.target.value)}
                            placeholder="Введите подзаголовок новости"
                            className="edit-subtitle-input"
                        />
                    </label>

                    <label className="edit-label">
                        Содержание
                        <textarea
                            value={newContent}
                            onChange={(e) => setNewContent(e.target.value)}
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

export default NewsPostCreatePage;