import React, {useEffect, useState} from 'react';
import { Bell, Menu, X, Home, Users, Calendar, FileText, Settings, ChevronRight, Search , Plus} from 'lucide-react';
import './NewsPage.css';
import NewsList from "../../components/news/NewsList";
import NewsSearchPanel from "../../components/news/NewsSearchPanel";
import Header from "../../components/layout/header/Header";
import {getAllPosts} from "../../api-handlers/postsHandler";
import {getAllDepartments} from "../../api-handlers/departmentsHandler";
import {useNavigate} from "react-router-dom";

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

    useEffect(() => {
        const fetchData = async () => {
            const data = await getAllPosts();
            setNewsItems(data);

            const depData = await getAllDepartments(true);
            let newDepartments = [];
            depData.forEach(item => {
                newDepartments.unshift({
                    id: item.id,
                    name: item.name
                });
            })
            setDepartments(newDepartments);
        }
        fetchData();
    }, [])

    const isAdminOrPublisher = true;

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
        //navigate('/news/add');
    };

    const filteredNews = newsItems.filter((news) => {
        const matchesSearch = news.title.toLowerCase().includes(searchQuery.toLowerCase());
        const matchesDepartment = filters.department ? news.departmentId === filters.department : true;
        const matchesImportance = filters.isImportant ? news.isImportant === true : true;

        return matchesSearch && matchesDepartment && matchesImportance;
    });

    const handleNewsClick = (news) => {
        setActiveNews(news);
        navigate(`/news/${news.id}`)
    };

    const closeNewsDetail = () => {
        setActiveNews(null);
    };

    return (
        <div className="app-container">
            <div className="content-wrapper">
                <main className="main-content">
                    <Header title={"Последние новости"} info={`Всего: ${filteredNews.length}`}/>
                    <div className="news-controls">
                        <NewsSearchPanel searchQuery={searchQuery} handleSearchChange={handleSearchChange}
                                         filters={filters} handleFilterChange={handleFilterChange} departments={departments}
                        />
                        {isAdminOrPublisher && (
                            <button className="add-news-button" onClick={handleAddNews}>
                                <Plus size={18}/> Добавить новость
                            </button>
                        )}
                    </div>
                    <NewsList newsItems={filteredNews} handleNewsClick={handleNewsClick}/>
                </main>
            </div>
        </div>
    );
};

export default NewsPage;