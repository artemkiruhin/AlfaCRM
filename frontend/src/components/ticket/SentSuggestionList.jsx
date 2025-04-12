import React from 'react';

const SentSuggestionList = ({ suggestions, isAdmin, onApprove, onReject, onDelete }) => {
    return (
        <div className="sent-suggestions-list">
            {suggestions.length > 0 ? (
                suggestions.map((suggestion) => (
                    <div key={suggestion.id} className="suggestion-item">
                        <div className="suggestion-header">
                            <h3 className="suggestion-title">{suggestion.title}</h3>
                            <span className={`suggestion-status ${suggestion.status.toLowerCase().replace(/\s+/g, '-')}`}>
                                {suggestion.status}
                            </span>
                        </div>
                        <p className="suggestion-text">{suggestion.text}</p>
                        <div className="suggestion-meta">
                            <span>Отдел: {suggestion.department}</span>
                            <span>Дата создания: {suggestion.date}</span>
                            {suggestion.employeeId && <span>ID сотрудника: {suggestion.employeeId}</span>}
                            {suggestion.assignee && <span>Обработал: {suggestion.assignee}</span>}
                        </div>
                        {suggestion.feedback && (
                            <div className="suggestion-feedback">
                                <strong>Комментарий:</strong> {suggestion.feedback}
                            </div>
                        )}
                        <div className="suggestion-actions">
                            {suggestion.status === 'На рассмотрении' && isAdmin && (
                                <>
                                    <button
                                        className="suggestion-action-button approve"
                                        onClick={() => onApprove(suggestion)}
                                    >
                                        Одобрить
                                    </button>
                                    <button
                                        className="suggestion-action-button reject"
                                        onClick={() => onReject(suggestion)}
                                    >
                                        Отклонить
                                    </button>
                                </>
                            )}
                            <button
                                className="suggestion-action-button delete"
                                onClick={() => onDelete(suggestion.id)}
                            >
                                Удалить
                            </button>
                        </div>
                    </div>
                ))
            ) : (
                <div className="no-suggestions-message">
                    Нет предложений, соответствующих фильтру
                </div>
            )}
        </div>
    );
};

export default SentSuggestionList;