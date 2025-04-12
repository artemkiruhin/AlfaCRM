import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, Save, Trash2 } from 'lucide-react';
import "./TicketDetailsPage.css";
import Header from "../../components/layout/header/Header";
import { getTicketById, editTicket, deleteTicket } from '../../api-handlers/ticketsHandler';
import { getAllDepartmentsShort } from '../../api-handlers/departmentsHandler';

const TicketDetailsPage = ({type}) => {
    const { id } = useParams();
    const navigate = useNavigate();

    const [ticket, setTicket] = useState(null);
    const [formData, setFormData] = useState({
        title: '',
        text: '',
        departmentId: '',
        feedback: ''
    });
    const [departments, setDepartments] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchData = async () => {
            try {
                setLoading(true);
                setError(null);

                const ticketData = await getTicketById(id);
                const deps = await getAllDepartmentsShort();

                if (!ticketData) {
                    throw new Error('Заявка не найдена');
                }

                setTicket(ticketData);
                setDepartments(deps || []);

                setFormData({
                    title: ticketData.title,
                    text: ticketData.text,
                    departmentId: ticketData.department.id,
                    feedback: ticketData.feedback || ''
                });

            } catch (err) {
                console.error('Failed to fetch ticket:', err);
                setError(err.message || 'Ошибка при загрузке заявки');
                if (type === 0) navigate('/tickets/my');
                else if (type === 1) navigate('/suggestions/my');
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, [id, navigate]);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: value
        }));
    };

    const handleSave = async () => {
        try {
            setLoading(true);
            setError(null);

            const feedback = isCompletedOrRejected() ? ticket.feedback : formData.feedback;

            const updatedTicket = await editTicket(
                id,
                formData.title,
                formData.text,
                formData.departmentId,
                feedback
            );

            setTicket(updatedTicket);
        } catch (err) {
            console.error('Failed to update ticket:', err);
            setError(err.message || 'Ошибка при сохранении изменений');
        } finally {
            setLoading(false);
        }
    };

    const handleDelete = async () => {
        if (!window.confirm('Вы уверены, что хотите удалить эту заявку?')) {
            return;
        }

        try {
            setLoading(true);
            setError(null);

            await deleteTicket(id);
            if (type === 0) navigate('/tickets/my', { state: { ticketDeleted: true } });
            else if (type === 1) navigate('/suggestions/sent');
        } catch (err) {
            console.error('Failed to delete ticket:', err);
            setError(err.message || 'Ошибка при удалении заявки');
        } finally {
            setLoading(false);
        }
    };

    const formatDate = (dateString) => {
        if (!dateString) return '';
        const options = {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        };
        return new Date(dateString).toLocaleString('ru-RU', options);
    };

    const getStatusColor = (status) => {
        switch(status) {
            case 'Создано': return 'var(--info-color)';
            case 'В работе': return 'var(--warning-color)';
            case 'Выполнено': return 'var(--success-color)';
            case 'Отменено': return 'var(--error-color)';
            default: return 'var(--text-light)';
        }
    };

    const isCompletedOrRejected = () => {
        return ticket?.status === 'Выполнено' || ticket?.status === 'Отменено';
    };

    const isEditable = () => {
        return !isCompletedOrRejected();
    };

    if (loading || !ticket) {
        return <div>Загрузка...</div>;
    }

    return (
        <div className="ticket-details-page">
            <Header title={type === 0 ? `Заявка #${id}` : `Предложение #${id}`} />

            <button className="back-button" onClick={type === 0 ? () => navigate('/tickets/my') : () => navigate('/suggestions/my')}>
                <ArrowLeft size={20} /> Назад
            </button>

            <div className="ticket-details-container">
                <div className="ticket-header">
                    <input
                        name="title"
                        value={formData.title}
                        onChange={handleChange}
                        className="edit-title-input"
                        required
                        disabled={!isEditable()}
                    />

                    <span
                        className="ticket-status"
                        style={{ backgroundColor: getStatusColor(ticket.status) }}
                    >
                        {ticket.status}
                    </span>
                </div>

                <div className="ticket-section">
                    <label>Описание:</label>
                    <textarea
                        name="text"
                        value={formData.text}
                        onChange={handleChange}
                        className="edit-textarea"
                        required
                        disabled={!isEditable()}
                    />
                </div>

                <div className="ticket-section">
                    <label>Отдел:</label>
                    <select
                        name="departmentId"
                        value={formData.departmentId}
                        onChange={handleChange}
                        className="edit-select"
                        required
                        disabled={!isEditable()}
                    >
                        {departments.map(dept => (
                            <option key={dept.id} value={dept.id}>
                                {dept.name}
                            </option>
                        ))}
                    </select>
                </div>

                {(isCompletedOrRejected() || formData.feedback) && (
                    <div className="ticket-section">
                        <label>Комментарий:</label>
                        {isCompletedOrRejected() ? (
                            <p className="feedback-text">{ticket.feedback || 'Нет комментария'}</p>
                        ) : (
                            <textarea
                                name="feedback"
                                value={formData.feedback}
                                onChange={handleChange}
                                className="edit-textarea"
                                placeholder="Введите комментарий по заявке"
                            />
                        )}
                    </div>
                )}

                <div className="ticket-meta">
                    <div className="meta-group">
                        <div className="meta-item">
                            <span className="meta-label">Создано:</span>
                            <span>{formatDate(ticket.createdAt)}</span>
                        </div>
                        <div className="meta-item">
                            <span className="meta-label">Автор:</span>
                            <span>{ticket.creator.username}</span>
                        </div>
                    </div>

                    {ticket.assignee && (
                        <div className="meta-group">
                            <div className="meta-item">
                                <span className="meta-label">Исполнитель:</span>
                                <span>{ticket.assignee.username}</span>
                            </div>
                        </div>
                    )}

                    {isCompletedOrRejected() && ticket.closedAt && (
                        <div className="meta-group">
                            <div className="meta-item">
                                <span className="meta-label">Завершено:</span>
                                <span>{formatDate(ticket.closedAt)}</span>
                            </div>
                        </div>
                    )}
                </div>

                <div className="ticket-actions">
                    {isEditable() && (
                        <button
                            onClick={handleSave}
                            className="btn-save"
                            disabled={loading}
                        >
                            <Save size={18} /> {loading ? 'Сохранение...' : 'Сохранить'}
                        </button>
                    )}
                    <button
                        onClick={handleDelete}
                        className="btn-delete"
                        disabled={loading}
                    >
                        <Trash2 size={18} /> {loading ? 'Удаление...' : 'Удалить'}
                    </button>
                </div>

                {error && <div className="error-message">{error}</div>}
            </div>
        </div>
    );
};

export default TicketDetailsPage;