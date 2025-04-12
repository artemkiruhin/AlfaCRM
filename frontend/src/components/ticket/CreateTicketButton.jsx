import React from 'react';
import { Link } from 'react-router-dom';

const CreateTicketButton = ({type, onCreatePageHandler}) => {
    return (
        // <Link to="/tickets/create" className="create-ticket-button">
        //     Создать заявку
        // </Link>
    <div className="create-ticket-button" onClick={() => onCreatePageHandler()}>
        {type === 0 ? "Создать заявку" : "Создать предложение"}
    </div>
    );
};

export default CreateTicketButton;