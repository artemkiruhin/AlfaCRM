import {formatDate} from "../../extensions/utils";
import {ChevronRight} from "lucide-react";
import React from "react";

const NewsPost = ({post, handleNewsClick}) => {

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

    return (
        <div
            key={post.id}
            className="news-card"
            onClick={() => handleNewsClick(post)}
        >
            <div className="news-content">
                <div className="news-header">
                    <h3 className="news-title">{post.title}</h3>
                    {post.isImportant && (
                        <span className="badge important-badge">
                            Важно
                          </span>
                    )}
                </div>
                <div className="news-date">
                    {formatDate(post.createdAt)}
                </div>
                <div className="news-department">
                    {post.department}
                </div>
            </div>
            <div className="news-footer">
                <span>Подробнее</span>
                <ChevronRight size={14} className="arrow-icon"/>
            </div>
        </div>
    )
}

export default NewsPost;