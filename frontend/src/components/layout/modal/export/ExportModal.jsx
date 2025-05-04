import React, { useState } from 'react';
import Modal from '../base/Modal';

const ExportModal = ({
                         isOpen,
                         onClose,
                         onExport,
                         defaultFilename = "",
                         defaultDescription = ""
                     }) => {
    const [filename, setFilename] = useState(defaultFilename);
    const [description, setDescription] = useState(defaultDescription);

    const handleSubmit = (e) => {
        e.preventDefault();
        onExport(filename, description);
    };

    return (
        <Modal isOpen={isOpen} onClose={onClose} title="Экспорт в Excel">
            <form onSubmit={handleSubmit} className="export-form">
                <div className="form-group">
                    <label htmlFor="filename">Название файла</label>
                    <input
                        type="text"
                        id="filename"
                        value={filename}
                        onChange={(e) => setFilename(e.target.value)}
                        placeholder="Введите название файла (необязательно)"
                    />
                </div>
                <div className="form-group">
                    <label htmlFor="description">Описание</label>
                    <textarea
                        id="description"
                        value={description}
                        onChange={(e) => setDescription(e.target.value)}
                        placeholder="Введите описание (необязательно)"
                        rows={3}
                    />
                </div>
                <div className="form-actions">
                    <button
                        type="button"
                        className="cancel-button"
                        onClick={onClose}
                    >
                        Отмена
                    </button>
                    <button type="submit" className="submit-button">
                        Экспортировать
                    </button>
                </div>
            </form>
        </Modal>
    );
};

export default ExportModal;