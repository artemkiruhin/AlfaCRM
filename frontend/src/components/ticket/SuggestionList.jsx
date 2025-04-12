import React from 'react';

const SuggestionList = ({ suggestions }) => {
    return (
        <div className="suggestion-list">
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
                            {suggestion.assignee && <span>Обработал: {suggestion.assignee}</span>}
                        </div>
                        {suggestion.feedback && (
                            <div className="suggestion-feedback">
                                <strong>Комментарий:</strong> {suggestion.feedback}
                            </div>
                        )}
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

export default SuggestionList;