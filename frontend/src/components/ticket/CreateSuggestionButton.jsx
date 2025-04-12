import React from 'react';

const CreateSuggestionButton = ({ onCreatePageHandler }) => {
    return (
        <button className="create-suggestion-button" onClick={onCreatePageHandler}>
            Создать предложение
        </button>
    );
};

export default CreateSuggestionButton;