import React, {useEffect, useState} from 'react';
import { Plus, FileDown } from 'lucide-react';
import './NewsPage.css';
import NewsList from "../../components/news/NewsList";
import NewsSearchPanel from "../../components/news/NewsSearchPanel";
import Header from "../../components/layout/header/Header";
import {getAllPosts} from "../../api-handlers/postsHandler";
import {getAllDepartments} from "../../api-handlers/departmentsHandler";
import {useNavigate} from "react-router-dom";
import ExportModal from "../../components/layout/modal/export/ExportModal";
import {exportToExcel} from "../../api-handlers/reportsHandler";

const NewsPage = () => {
    const navigate = useNavigate();

    const [isSidebarOpen, setIsSidebarOpen] = useState(false);
    const [activeNews, setActiveNews] = useState(null);
    const [searchQuery, setSearchQuery] = useState('');
    const [filters, setFilters] = useState({
        department: '',
        isImportant: false,
    });

    const [departments, setDepartments] = useState([{
        id: "",
        name: ""
    }]);

    const [newsItems, setNewsItems] = useState([{
        id: "",
        title: "",
        createdAt: "",
        isImportant: false,
        department: "",
        departmentId: ""
    }]);

    const [isLoading, setIsLoading] = useState(true);
    const [isExportModalOpen, setIsExportModalOpen] = useState(false);

    useEffect(() => {
        const fetchData = async () => {
            setIsLoading(true);
            try {
                const data = await getAllPosts();
                setNewsItems(data);

                const depData = await getAllDepartments(true);
                let newDepartments = [];
                depData.forEach(item => {
                    newDepartments.unshift({
                        id: item.id,
                        name: item.name
                    });
                });
                setDepartments(newDepartments);
            } catch (error) {
                console.error('Ошибка при загрузке данных:', error);
            } finally {
                setIsLoading(false);
            }
        };
        fetchData();
    }, []);

    const isAdminOrPublisher = localStorage.getItem('adm') === true || localStorage.getItem('spec') === true;

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

    const handleAddNews = () => {
        navigate('/news/add');
    };

    const filteredNews = newsItems.filter((news) => {
        const matchesSearch = news.title.toLowerCase().includes(searchQuery.toLowerCase());
        const matchesDepartment = filters.department ? news.departmentId === filters.department : true;
        const matchesImportance = filters.isImportant ? news.isImportant === true : true;

        return matchesSearch && matchesDepartment && matchesImportance;
    });

    const handleNewsClick = (news) => {
        setActiveNews(news);
        navigate(`/news/${news.id}`);
    };

    const closeNewsDetail = () => {
        setActiveNews(null);
    };

    const handleExportClick = () => {
        setIsExportModalOpen(true);
    };

    const handleExportConfirm = async (filename, description) => {
        try {
            await exportToExcel(2, filename || "Отчет_по_новостям", description || "");
            setIsExportModalOpen(false);
        } catch (error) {
            console.error('Ошибка при экспорте:', error);
            alert('Произошла ошибка при экспорте данных');
        }
    };

    return (
        <div className="app-container">
            <div className="content-wrapper">
                <main className="main-content">
                    <Header title={"Последние новости"} info={`Всего: ${filteredNews.length}`}/>
                    <div className="news-controls">
                        <NewsSearchPanel
                            searchQuery={searchQuery}
                            handleSearchChange={handleSearchChange}
                            filters={filters}
                            handleFilterChange={handleFilterChange}
                            departments={departments}
                        />
                        <div className="news-action-buttons">
                            {isAdminOrPublisher && (
                                <>
                                    <button
                                        className="add-news-button"
                                        onClick={handleAddNews}
                                    >
                                        <Plus size={18}/> Добавить новость
                                    </button>
                                    <button
                                        className="export-button"
                                        onClick={handleExportClick}
                                        disabled={isLoading || filteredNews.length === 0}
                                    >
                                        <FileDown size={18}/>
                                        Экспорт в Excel
                                    </button>
                                </>
                            )}
                        </div>
                    </div>

                    {isLoading ? (
                        <div className="loading">Загрузка...</div>
                    ) : filteredNews.length === 0 ? (
                        <div className="empty-state">
                            <p>Нет доступных новостей</p>
                        </div>
                    ) : (
                        <NewsList newsItems={filteredNews} handleNewsClick={handleNewsClick}/>
                    )}
                </main>
            </div>

            <ExportModal
                isOpen={isExportModalOpen}
                onClose={() => setIsExportModalOpen(false)}
                onExport={handleExportConfirm}
                defaultFilename="Отчет_по_новостям"
            />
        </div>
    );
};

export default NewsPage;