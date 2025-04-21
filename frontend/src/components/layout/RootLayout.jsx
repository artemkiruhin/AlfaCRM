import React from "react";
import TopNavigation from "./navigation/TopNavigation";
import { Home, Users, Calendar, FileText, Settings } from 'lucide-react';
import { useLocation } from 'react-router-dom';

const RootLayout = ({ page }) => {
    const location = useLocation();
    const isSpecDepartment = localStorage.getItem('spec');
    const isAdmin = localStorage.getItem('adm');

    const emplMenuItems = [
        { icon: <FileText size={20} className="nav-icon" />, label: 'Новости', link: "/news" },
        { icon: <Users size={20} className="nav-icon" />, label: 'Чат', link: "/chat" },
        { icon: <Calendar size={20} className="nav-icon" />, label: 'Заявки', link: "/tickets/my" },
        { icon: <Settings size={20} className="nav-icon" />, label: 'Предложения', link: "/suggestions/my" },
    ];

    const specMenuItems = [
        { icon: <FileText size={20} className="nav-icon" />, label: 'Новости', link: "/news" },
        { icon: <Users size={20} className="nav-icon" />, label: 'Чат', link: "/chat" },
        { icon: <Calendar size={20} className="nav-icon" />, label: 'Заявки', link: "/tickets/my" },
        { icon: <Calendar size={20} className="nav-icon" />, label: 'Отправленные заявки', link: "/tickets/sent" },
        { icon: <Settings size={20} className="nav-icon" />, label: 'Предложения', link: "/suggestions/my" },
        { icon: <Settings size={20} className="nav-icon" />, label: 'Отправленные предложения', link: "/suggestions/sent" },
    ];

    const adminMenuItems = [
        { icon: <FileText size={20} className="nav-icon" />, label: 'Новости', link: "/news" },
        { icon: <Users size={20} className="nav-icon" />, label: 'Чат', link: "/chat" },
        { icon: <Calendar size={20} className="nav-icon" />, label: 'Заявки', link: "/tickets/my" },
        { icon: <Calendar size={20} className="nav-icon" />, label: 'Отправленные заявки', link: "/tickets/sent" },
        { icon: <Settings size={20} className="nav-icon" />, label: 'Предложения', link: "/suggestions/my" },
        { icon: <Settings size={20} className="nav-icon" />, label: 'Отправленные предложения', link: "/suggestions/sent" },
        { icon: <Settings size={20} className="nav-icon" />, label: 'Панель администратора', link: "/admin" }
    ];

    let items = emplMenuItems;
    if (isSpecDepartment === "true") items = specMenuItems;
    if (isAdmin === "true") items = adminMenuItems;

    return (
        <>
            <TopNavigation menuItems={items} />
            {React.cloneElement(page, { key: location.pathname })}
        </>
    );
};

export default RootLayout;