import React, { useEffect, useState } from 'react';
import { Building, Users, Plus, ArrowRight, Ticket, Loader2, X, RefreshCw, Check, UserPlus } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import './AdminPanel.css';
import { getStats, getBusinessStats, getUsersWorkload, assignTicketToUser, distributeTickets } from "../../api-handlers/adminHandler";

const AdminPanel = () => {
    const navigate = useNavigate();
    const [loading, setLoading] = useState({
        stats: false,
        distribution: false,
        auto: false,
        manual: false
    });
    const [showDistributionModal, setShowDistributionModal] = useState(false);
    const [usersWorkload, setUsersWorkload] = useState([]);
    const [selectedTicket, setSelectedTicket] = useState(null);
    const [ticketsForDistribution, setTicketsForDistribution] = useState([]);
    const [distributionMode, setDistributionMode] = useState('');

    const [statsData, setStatsData] = useState({
        departmentsAmount: '0',
        usersAmount: '0',
        problemCasesCount: '0',
        solvedProblemCasesCount: '0',
        suggestionsCount: '0',
        solvedSuggestionsCount: '0',
        logsCount: '0'
    });

    useEffect(() => {
        const fetchData = async () => {
            setLoading(prev => ({...prev, stats: true}));
            try {
                const stats = await getStats();
                if (stats) {
                    setStatsData({
                        departmentsAmount: stats.departmentsAmount?.toString() || '0',
                        usersAmount: stats.usersAmount?.toString() || '0',
                        problemCasesCount: stats.problemCasesCount?.toString() || '0',
                        solvedProblemCasesCount: stats.solvedProblemCasesCount?.toString() || '0',
                        suggestionsCount: stats.suggestionsCount?.toString() || '0',
                        solvedSuggestionsCount: stats.solvedSuggestionsCount?.toString() || '0',
                        logsCount: stats.logsCount?.toString() || '0'
                    });
                }
            } catch (error) {
                console.error('Ошибка загрузки данных:', error);
            } finally {
                setLoading(prev => ({...prev, stats: false}));
            }
        };

        fetchData();
    }, []);

    const loadTicketsAndUsers = async () => {
        setLoading(prev => ({...prev, distribution: true}));
        try {
            const businessStats = await getBusinessStats();
            console.log('Business stats:', businessStats);

            if (businessStats?.tickets) {
                setTicketsForDistribution(businessStats.tickets || []);
            } else {
                setTicketsForDistribution([]);
            }

            const result = await getUsersWorkload();
            console.log('Users workload:', result);

            if (Array.isArray(result)) {
                setUsersWorkload(result);
            } else {
                setUsersWorkload([]);
            }
        } catch (error) {
            console.error('Ошибка загрузки данных:', error);
        } finally {
            setLoading(prev => ({...prev, distribution: false}));
        }
    };

    const handleAutoDistribution = async () => {
        setLoading(prev => ({...prev, auto: true}));
        try {
            await distributeTickets();
            await loadTicketsAndUsers();
            setDistributionMode('auto');
            alert('Автоматическое распределение выполнено успешно!');
        } catch (error) {
            console.error('Ошибка автоматического распределения:', error);
            alert('Ошибка при автоматическом распределении!');
        } finally {
            setLoading(prev => ({...prev, auto: false}));
        }
    };

    const openManualDistribution = async () => {
        await loadTicketsAndUsers();
        setDistributionMode('manual');
        setShowDistributionModal(true);
    };

    const handleManualAssign = async (userId) => {
        if (!selectedTicket) return;

        setLoading(prev => ({...prev, manual: true}));
        try {
            await assignTicketToUser(userId, selectedTicket.id);
            setTicketsForDistribution(prev => prev.filter(t => t.id !== selectedTicket.id));
            setSelectedTicket(null);
        } catch (error) {
            console.error('Ошибка назначения заявки:', error);
        } finally {
            setLoading(prev => ({...prev, manual: false}));
        }
    };

    useEffect(() => {
        const handleKeyDown = (e) => {
            if (e.key === 'Escape') {
                setShowDistributionModal(false);
            }
        };

        document.addEventListener('keydown', handleKeyDown);
        return () => document.removeEventListener('keydown', handleKeyDown);
    }, []);

    return (
        <div className="admin-panel">
            {/* Основной интерфейс */}
            <div className="admin-header">
                <h1 className="admin-title">Панель администратора</h1>
                <div className="admin-actions">
                    <button className="admin-btn admin-btn-primary">
                        <Plus size={18} /> Создать
                    </button>
                </div>
            </div>

            <div className="admin-grid">
                {[
                    {
                        title: "Отделы",
                        subtitle: "Управление организационной структурой",
                        stats: [{value: statsData.departmentsAmount, label: "Отделов"}],
                        actions: [
                            {icon: <Building size={18}/>, text: "Управление", onClick: () => navigate('/departments')}
                        ]
                    },
                    {
                        title: "Сотрудники",
                        subtitle: "Управление пользователями системы",
                        stats: [{value: statsData.usersAmount, label: "Сотрудников"}],
                        actions: [
                            {icon: <Users size={18}/>, text: "Список", onClick: () => navigate('/users')},
                            {icon: <Plus size={18}/>, text: "Добавить", onClick: () => navigate('/users/create'), secondary: true}
                        ]
                    },
                    {
                        title: "Заявки",
                        subtitle: "Статистика по заявкам",
                        stats: [
                            {value: statsData.problemCasesCount, label: "Активных"},
                            {value: statsData.solvedProblemCasesCount, label: "Решённых"}
                        ],
                        actions: [
                            {icon: <ArrowRight size={18}/>, text: "Перейти", onClick: () => navigate('/tickets/sent')}
                        ]
                    },
                    {
                        title: "Предложения",
                        subtitle: "Статистика по предложениям",
                        stats: [
                            {value: statsData.suggestionsCount, label: "Активных"},
                            {value: statsData.solvedSuggestionsCount, label: "Решённых"}
                        ],
                        actions: [
                            {icon: <ArrowRight size={18}/>, text: "Перейти", onClick: () => navigate('/suggestions/sent')}
                        ]
                    },
                    {
                        title: "Логи",
                        subtitle: "Статистика по логам",
                        stats: [{value: statsData.logsCount, label: "Всего"}],
                        actions: [
                            {icon: <ArrowRight size={18}/>, text: "Перейти", onClick: () => navigate('/logs')}
                        ]
                    }
                ].map((card, index) => (
                    <div key={`card-${index}`} className="admin-card">
                        <div className="admin-card-header">
                            <h2 className="admin-card-title">{card.title}</h2>
                            <p className="admin-card-subtitle">{card.subtitle}</p>
                        </div>
                        <div className="admin-card-body">
                            <div className="admin-card-stats">
                                {card.stats.map((stat, statIndex) => (
                                    <div key={`stat-${index}-${statIndex}`} className="stat-item">
                                        <div className="stat-value">{stat.value}</div>
                                        <div className="stat-label">{stat.label}</div>
                                    </div>
                                ))}
                            </div>
                            <div className="admin-card-actions">
                                {card.actions.map((action, actionIndex) => (
                                    <button
                                        key={`action-${index}-${actionIndex}`}
                                        className={`admin-btn ${action.secondary ? 'admin-btn-secondary' : 'admin-btn-primary'}`}
                                        onClick={action.onClick}
                                    >
                                        {action.icon} {action.text}
                                    </button>
                                ))}
                            </div>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
};

export default AdminPanel;