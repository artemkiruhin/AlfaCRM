import React from 'react';
import './ConfirmationModal.css';
import Modal from '../base/Modal';

const ConfirmationModal = ({
                               isOpen,
                               onClose,
                               onConfirm,
                               title,
                               message,
                               confirmText = "Подтвердить",
                               cancelText = "Отмена"
                           }) => {
    if (!isOpen) return null;

    return (
        <Modal isOpen={isOpen} onClose={onClose} title={title}>
            <div className="confirmation-modal-content">
                <p className="confirmation-message">{message}</p>
                <div className="confirmation-actions">
                    <button
                        className="confirmation-cancel-button"
                        onClick={onClose}
                    >
                        {cancelText}
                    </button>
                    <button
                        className="confirmation-confirm-button"
                        onClick={onConfirm}
                    >
                        {confirmText}
                    </button>
                </div>
            </div>
        </Modal>
    );
};

export default ConfirmationModal;