import React, { useState, useEffect } from 'react';
import { ArrowLeft, Save } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import './TicketCreatePage.css';
import Header from "../../components/layout/header/Header";
import { createTicket } from '../../api-handlers/ticketsHandler';
import { getAllDepartmentsShort } from '../../api-handlers/departmentsHandler';


const TicketCreatePage = () => {
    const navigate = useNavigate();
    const [formData, setFormData] = useState({
        title: '',
        text: '',
        departmentId: ''
    });
    const [departments, setDepartments] = useState([]);
    const [loading, setLoading] = useState(false);
    const [initialLoading, setInitialLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchDepartments = async () => {
            try {
                const deps = await getAllDepartmentsShort();
                setDepartments(deps || []);
                setInitialLoading(false);
            } catch (err) {
                console.error('Failed to fetch departments:', err);
                setError('Не удалось загрузить список отделов');
                setInitialLoading(false);
            }
        };

        fetchDepartments();
    }, []);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: value
        }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!formData.title.trim()) {
            setError('Пожалуйста, укажите заголовок заявки');
            return;
        }

        if (!formData.text.trim()) {
            setError('Пожалуйста, опишите вашу заявку');
            return;
        }

        if (!formData.departmentId) {
            setError('Пожалуйста, выберите отдел');
            return;
        }

        try {
            setLoading(true);
            setError(null);

            await createTicket(
                formData.title,
                formData.text,
                formData.departmentId,
                0
            );

            navigate('/tickets/my', { state: { ticketCreated: true } });
        } catch (err) {
            console.error('Ticket creation failed:', err);
            setError(err.message || 'Не удалось создать заявку. Пожалуйста, попробуйте позже.');
        } finally {
            setLoading(false);
        }
    };

    const handleCancel = () => {
        navigate('/tickets/my');
    };

    // if (initialLoading) {
    //     return <LoadingSpinner />;
    // }

    if (error && !departments.length) {
        return (
            <div className="ticket-create-page">
                <Header title={"Создание новой заявки"} />
                {/*<ErrorMessage message={error} />*/}
                <button className="back-button" onClick={handleCancel}>
                    <ArrowLeft size={20} /> Назад
                </button>
            </div>
        );
    }

    return (

        <div className="ticket-create-page">
            <Header title={"Создание новой заявки"} />
            <button className="back-button" onClick={handleCancel}>
                <ArrowLeft size={20} /> Назад
            </button>

            <form onSubmit={handleSubmit} className="ticket-create-form">
                {/*{error && <ErrorMessage message={error} />}*/}

                <div className="form-field">
                    <label htmlFor="title">Заголовок*:</label>
                    <input
                        id="title"
                        name="title"
                        type="text"
                        value={formData.title}
                        onChange={handleChange}
                        maxLength={100}
                        placeholder="Кратко опишите проблему"
                        required
                    />
                </div>

                <div className="form-field">
                    <label htmlFor="text">Описание проблемы*:</label>
                    <textarea
                        id="text"
                        name="text"
                        value={formData.text}
                        onChange={handleChange}
                        rows={6}
                        placeholder="Подробно опишите вашу заявку (что случилось, когда, важные детали)"
                        required
                    />
                </div>

                <div className="form-field">
                    <label htmlFor="departmentId">Отдел*:</label>
                    <select
                        id="departmentId"
                        name="departmentId"
                        value={formData.departmentId}
                        onChange={handleChange}
                        required
                    >
                        <option value="">Выберите отдел</option>
                        {departments.map(dept => (
                            <option key={dept.id} value={dept.id}>
                                {dept.name}
                            </option>
                        ))}
                    </select>
                </div>

                <div className="form-actions">
                    <button
                        type="submit"
                        className="submit-button"
                        disabled={loading}
                    >
                        {/*{loading ? (*/}
                        {/*    <LoadingSpinner small />*/}
                        {/*) : (*/}
                            <>
                                <Save size={18} /> Создать заявку
                            </>
                        {/*)}*/}
                    </button>
                    <button
                        type="button"
                        className="cancel-button"
                        onClick={handleCancel}
                        disabled={loading}
                    >
                        Отмена
                    </button>
                </div>
            </form>
        </div>
    );
};

export default TicketCreatePage;