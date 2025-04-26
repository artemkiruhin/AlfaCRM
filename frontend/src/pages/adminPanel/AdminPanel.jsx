import React, {useEffect, useState} from 'react';
import { Building, Users, Settings, Plus, ArrowRight } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import './AdminPanel.css'
import {getStats} from "../../api-handlers/adminHandler";

const AdminPanel = () => {
    const navigate = useNavigate();

    const [statsData, setStatsData] = useState({
        departmentsAmount: '',
        usersAmount: '',
        problemCasesCount: '',
        solvedProblemCasesCount: '',
        suggestionsCount: '',
        solvedSuggestionsCount: ''
    });

    useEffect(() => {
        const fetchData = async () => {
            const response = await getStats();
            setStatsData(response);
        }

        fetchData();
    }, []);

    return (
        <div className="admin-panel">
            <div className="admin-header">
                <h1 className="admin-title">Панель администратора</h1>
                <div className="admin-actions">
                    <button className="admin-btn admin-btn-primary">
                        <Plus size={18} /> Создать
                    </button>
                </div>
            </div>

            <div className="admin-grid">
                <div className="admin-card">
                    <div className="admin-card-header">
                        <h2 className="admin-card-title">Отделы</h2>
                        <p className="admin-card-subtitle">Управление организационной структурой</p>
                    </div>
                    <div className="admin-card-body">
                        <div className="admin-card-stats">
                            <div className="stat-item">
                                <div className="stat-value">{statsData.departmentsAmount}</div>
                                <div className="stat-label">Отделов</div>
                            </div>
                        </div>
                        <div className="admin-card-actions">
                            <button
                                className="admin-btn admin-btn-primary"
                                onClick={() => navigate('/departments')}
                            >
                                <Building size={18}/> Управление
                            </button>
                        </div>
                    </div>
                </div>

                <div className="admin-card">
                    <div className="admin-card-header">
                        <h2 className="admin-card-title">Сотрудники</h2>
                        <p className="admin-card-subtitle">Управление пользователями системы</p>
                    </div>
                    <div className="admin-card-body">
                        <div className="admin-card-stats">
                            <div className="stat-item">
                                <div className="stat-value">{statsData.usersAmount}</div>
                                <div className="stat-label">Сотрудников</div>
                            </div>
                        </div>
                        <div className="admin-card-actions">
                            <button
                                className="admin-btn admin-btn-primary"
                                onClick={() => navigate('/users')}
                            >
                                <Users size={18}/> Список
                            </button>
                            <button
                                className="admin-btn admin-btn-secondary"
                                onClick={() => navigate('/users/create')}
                            >
                                <Plus size={18}/> Добавить
                            </button>
                        </div>
                    </div>
                </div>

                <div className="admin-card">
                    <div className="admin-card-header">
                        <h2 className="admin-card-title">Заявки</h2>
                        <p className="admin-card-subtitle">Статистика по заявкам</p>
                    </div>
                    <div className="admin-card-body">
                        <div className="admin-card-stats">
                            <div className="stat-item">
                                <div className="stat-value">{statsData.problemCasesCount}</div>
                                <div className="stat-label">Активных</div>
                            </div>
                            <div className="stat-item">
                                <div className="stat-value">{statsData.solvedProblemCasesCount}</div>
                                <div className="stat-label">Решённых</div>
                            </div>
                        </div>
                        <div className="admin-card-actions">
                            <button
                                className="admin-btn admin-btn-primary"
                                onClick={() => navigate('/tickets/sent')}
                            >
                                <ArrowRight size={18}/> Перейти
                            </button>
                        </div>
                    </div>
                </div>

                <div className="admin-card">
                    <div className="admin-card-header">
                        <h2 className="admin-card-title">Предложения</h2>
                        <p className="admin-card-subtitle">Статистика по предложениям</p>
                    </div>
                    <div className="admin-card-body">
                        <div className="admin-card-stats">
                            <div className="stat-item">
                                <div className="stat-value">{statsData.suggestionsCount}</div>
                                <div className="stat-label">Активных</div>
                            </div>
                            <div className="stat-item">
                                <div className="stat-value">{statsData.solvedSuggestionsCount}</div>
                                <div className="stat-label">Решённых</div>
                            </div>
                        </div>
                        <div className="admin-card-actions">
                            <button
                                className="admin-btn admin-btn-primary"
                                onClick={() => navigate('/suggestions/sent')}
                            >
                                <ArrowRight size={18}/> Перейти
                            </button>
                        </div>
                    </div>
                </div>
            </div>

            <div className="departments-section">
                <div className="section-header">
                    <h2 className="section-title">Быстрые действия</h2>
                </div>
                <div className="admin-grid">
                    <div className="admin-card">
                        <div className="admin-card-body">
                            <h3 className="admin-card-title" style={{marginBottom: '16px'}}>Настройки системы</h3>
                            <button
                                className="admin-btn admin-btn-primary"
                                style={{width: '100%'}}
                                onClick={() => navigate('/admin/settings')}
                            >
                                <Settings size={18}/> Настройки
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default AdminPanel;