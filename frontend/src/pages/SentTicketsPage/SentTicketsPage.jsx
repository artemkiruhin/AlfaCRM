import React, { useState, useEffect } from 'react';
import {useLocation, useNavigate} from 'react-router-dom';
import "./SentTicketsPage.css";
import Header from "../../components/layout/header/Header";
import {getAllTickets, changeTicketStatus, deleteTicket} from '../../api-handlers/ticketsHandler';
import SentTicketList from "../../components/ticket/SentTicketList";
import { FileDown } from 'lucide-react';
import ExportModal from "../../components/layout/modal/export/ExportModal";
import {exportToExcel} from "../../api-handlers/reportsHandler";

const SentTicketsPage = ({type}) => {
    const navigate = useNavigate();
    const location = useLocation();
    const [tickets, setTickets] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [filterEmployeeId, setFilterEmployeeId] = useState('');
    const [filteredTickets, setFilteredTickets] = useState([]);
    const [isAdmin, setIsAdmin] = useState(false);
    const [isExportModalOpen, setIsExportModalOpen] = useState(false);

    useEffect(() => {
        const fetchTickets = async () => {
            try {
                setLoading(true);
                const isAdm = localStorage.getItem('adm') === "true";
                const response = await getAllTickets(isAdm ? null : localStorage.getItem('did'), false, type);

                if (response && Array.isArray(response)) {
                    const formattedTickets = response.map(ticket => ({
                        id: ticket.id,
                        title: ticket.title,
                        text: ticket.text,
                        department: ticket.department?.name || 'Не указан',
                        date: new Date(ticket.createdAt).toLocaleString(),
                        status: ticket.status,
                        feedback: ticket.feedback || '',
                        assignee: ticket.assignee ? ticket.assignee.username : 'Не назначен',
                        employeeId: ticket.creator?.username || 'Неизвестно',
                        closedAt: ticket.closedAt ? new Date(ticket.closedAt).toLocaleString() : null,
                        creator: ticket.creator?.username || 'Неизвестно'
                    }));

                    setTickets(formattedTickets);
                    setFilteredTickets(formattedTickets);
                } else {
                    throw new Error('Некорректный формат данных от сервера');
                }

                setIsAdmin(isAdm);
            } catch (err) {
                console.error('Failed to fetch tickets:', err);
                setError('Не удалось загрузить заявки');
            } finally {
                setLoading(false);
            }
        };

        fetchTickets();
    }, [type, location.key]);

    useEffect(() => {
        if (filterEmployeeId) {
            const filtered = tickets.filter(ticket =>
                ticket.employeeId && ticket.employeeId.toLowerCase().includes(filterEmployeeId.toLowerCase())
            );
            setFilteredTickets(filtered);
        } else {
            setFilteredTickets(tickets);
        }
    }, [filterEmployeeId, tickets]);

    const handleTakeToWork = async (ticketId) => {
        try {
            await changeTicketStatus(ticketId, 1, null);

            const updatedTickets = tickets.map((ticket) =>
                ticket.id === ticketId ? {
                    ...ticket,
                    status: 'В работе',
                    assignee: 'currentUser'
                } : ticket
            );

            setTickets(updatedTickets);
            alert(`Заявка ${ticketId} взята в работу`);
        } catch (err) {
            console.error('Failed to take ticket to work:', err);
            alert('Не удалось взять заявку в работу');
        }
    };

    const handleCloseTicket = async (ticketId, feedback) => {
        try {
            await changeTicketStatus(ticketId, 3, feedback);

            const updatedTickets = tickets.map((ticket) =>
                ticket.id === ticketId ? {
                    ...ticket,
                    status: 'Отменено',
                    feedback,
                    closedAt: new Date().toLocaleString()
                } : ticket
            );

            setTickets(updatedTickets);
            alert(`Заявка ${ticketId} закрыта`);
        } catch (err) {
            console.error('Failed to close ticket:', err);
            alert('Не удалось закрыть заявку');
        }
    };

    const handleCompleteTicket = async (ticketId, feedback) => {
        try {
            await changeTicketStatus(ticketId, 2, feedback);

            const updatedTickets = tickets.map((ticket) =>
                ticket.id === ticketId ? {
                    ...ticket,
                    status: 'Выполнено',
                    feedback,
                    closedAt: new Date().toLocaleString()
                } : ticket
            );

            setTickets(updatedTickets);
            alert(`Заявка ${ticketId} выполнена`);
        } catch (err) {
            console.error('Failed to complete ticket:', err);
            alert('Не удалось выполнить заявку');
        }
    };

    const handleDelete = async (id) => {
        try {
            await deleteTicket(id);
            setTickets(tickets.filter((ticket) => ticket.id !== id));
        } catch (err) {
            console.error('Failed to delete ticket:', err);
        }
    };

    const handleExportClick = () => {
        setIsExportModalOpen(true);
    };

    const handleExportConfirm = async (filename, description) => {
        try {
            const reportType = type === 0 ? 3 : 4;
            await exportToExcel(reportType, filename || (type === 0 ? "Отчет_по_заявкам" : "Отчет_по_предложениям"), description || "");
            setIsExportModalOpen(false);
        } catch (error) {
            console.error('Ошибка при экспорте:', error);
            alert('Произошла ошибка при экспорте данных');
        }
    };

    const getDefaultFilename = () => {
        return type === 0 ? "Отчет_по_заявкам" : "Отчет_по_предложениям";
    };

    return (
        <div className="sent-tickets-page">
            <Header title={type === 0 ? "Отправленные заявки" : "Отправленные предложения"} info={`Всего: ${filteredTickets.length}`} />

            <div className="page-controls">
                <div className="filter-container">
                    <input
                        type="text"
                        placeholder="Поиск по ID сотрудника..."
                        value={filterEmployeeId}
                        onChange={(e) => setFilterEmployeeId(e.target.value)}
                        className="filter-input"
                    />
                </div>

                <div className="export-buttons">
                    <button
                        className="export-button"
                        onClick={() => handleExportClick('standard')}
                        disabled={loading || filteredTickets.length === 0}
                    >
                        <FileDown size={18} />
                        Стандартный экспорт
                    </button>
                    <button
                        className="export-button export-button-detailed"
                        onClick={() => handleExportClick('detailed')}
                        disabled={loading || filteredTickets.length === 0}
                    >
                        <FileDown size={18} />
                        Детальный экспорт
                    </button>
                </div>
            </div>

            {loading ? (
                <div className="loading">Загрузка...</div>
            ) : error ? (
                <div className="error-message">{error}</div>
            ) : filteredTickets.length === 0 ? (
                <div className="empty-state">
                    <p>{type === 0 ? "Нет отправленных заявок" : "Нет отправленных предложений"}</p>
                </div>
            ) : (
                <SentTicketList
                    tickets={filteredTickets}
                    isAdmin={isAdmin}
                    onTakeToWork={handleTakeToWork}
                    onClose={handleCloseTicket}
                    onComplete={handleCompleteTicket}
                    onDelete={handleDelete}
                />
            )}

            <ExportModal
                isOpen={isExportModalOpen}
                onClose={() => setIsExportModalOpen(false)}
                onExport={handleExportConfirm}
                defaultFilename={getDefaultFilename()}
            />
        </div>
    );
};

export default SentTicketsPage;