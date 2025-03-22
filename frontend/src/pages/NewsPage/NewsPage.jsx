import React, { useState } from 'react';
import { Bell, Menu, X, Home, Users, Calendar, FileText, Settings, ChevronRight, Search } from 'lucide-react';
import './NewsPage.css';
import NewsList from "../../components/news/NewsList";
import { formatDate } from "../../extensions/utils";
import NewsSearchPanel from "../../components/news/NewsSearchPanel";

const NewsPage = () => {
    const [isSidebarOpen, setIsSidebarOpen] = useState(false);
    const [activeNews, setActiveNews] = useState(null);
    const [searchQuery, setSearchQuery] = useState('');
    const [filters, setFilters] = useState({
        department: '',
        isImportant: false,
    });

    const newsItems = [
        {
            id: 1,
            title: "Обновление корпоративной политики безопасности",
            createdAt: "2025-03-20T10:30:00",
            isImportant: true,
            department: "IT отдел",
            content: "Уважаемые коллеги! С 1 апреля вступают в силу обновленные правила корпоративной безопасности. Всем сотрудникам необходимо ознакомиться с новыми требованиями и пройти обязательное обучение до конца месяца."
        },
        {
            id: 2,
            title: "Корпоративное мероприятие",
            createdAt: "2025-03-19T14:15:00",
            isImportant: false,
            department: "HR отдел",
            content: "Приглашаем всех сотрудников на ежегодный корпоративный пикник, который состоится 15 апреля в 14:00 в городском парке. Регистрация участников открыта до 10 апреля через корпоративный портал."
        },
        {
            id: 3,
            title: "Финансовые результаты за Q1 2025",
            createdAt: "2025-03-18T09:45:00",
            isImportant: true,
            department: "Общая новость",
            content: "Компания успешно завершила первый квартал 2025 года, превысив плановые показатели на 15%. Детальный отчет будет представлен на общем собрании в пятницу. Поздравляем всех с отличными результатами!"
        },
        {
            id: 4,
            title: "Обновление программного обеспечения",
            createdAt: "2025-03-17T11:20:00",
            isImportant: false,
            department: "IT отдел",
            content: "В ближайшие выходные будет проведено плановое обновление внутренних систем. Возможны кратковременные перебои в работе корпоративных сервисов с 22:00 субботы до 06:00 воскресенья."
        },
        {
            id: 5,
            title: "Вакансии в отделе маркетинга",
            createdAt: "2025-03-16T16:30:00",
            isImportant: false,
            department: "HR отдел",
            content: "Открыты новые вакансии в отделе маркетинга: SMM-специалист и контент-менеджер. Если вы знаете подходящих кандидатов, пожалуйста, направьте их резюме в HR отдел до конца месяца."
        },
        {
            id: 6,
            title: "Изменение в расписании работы офиса",
            createdAt: "2025-03-15T13:10:00",
            isImportant: true,
            department: "Общая новость",
            content: "В связи с проведением технических работ в здании, 25 марта офис будет работать с 12:00 до 18:00. Просьба планировать свою работу с учетом данного изменения. Удаленная работа в этот день приветствуется."
        }
    ];

    const handleSearchChange = (e) => {
        setSearchQuery(e.target.value);
    };

    const handleFilterChange = (e) => {
        const { name, value, type, checked } = e.target;
        setFilters({
            ...filters,
            [name]: type === 'checkbox' ? checked : value,
        });
    };

    const filteredNews = newsItems.filter((news) => {
        const matchesSearch = news.title.toLowerCase().includes(searchQuery.toLowerCase());
        const matchesDepartment = filters.department ? news.department === filters.department : true;
        const matchesImportance = filters.isImportant ? news.isImportant === true : true;

        return matchesSearch && matchesDepartment && matchesImportance;
    });

    const handleNewsClick = (news) => {
        setActiveNews(news);
    };

    const closeNewsDetail = () => {
        setActiveNews(null);
    };

    return (
        <div className="app-container">
            <div className="content-wrapper">
                <main className="main-content">
                    <div className="section-header">
                        <h2 className="section-title">Последние новости</h2>
                        <div className="section-info">Всего: {filteredNews.length}</div>
                    </div>
                    <NewsSearchPanel searchQuery={searchQuery} handleSearchChange={handleSearchChange} filters={filters} handleFilterChange={handleFilterChange} />
                    <NewsList newsItems={filteredNews} handleNewsClick={handleNewsClick}/>
                </main>
            </div>
        </div>
    );
};

export default NewsPage;