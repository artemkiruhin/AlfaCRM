import React from 'react';
import { Link } from 'react-router-dom';

const CreateTicketButton = ({onCreatePageHandler}) => {
    return (
        // <Link to="/tickets/create" className="create-ticket-button">
        //     Создать заявку
        // </Link>
    <div className="create-ticket-button" onClick={() => onCreatePageHandler()}>
        Создать заявку
    </div>
    );
};

export default CreateTicketButton;