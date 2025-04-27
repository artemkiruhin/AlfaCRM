import React from "react";
import TopNavigation from "./navigation/TopNavigation";
import {
    Newspaper,
    MessageSquare,
    Ticket,
    Send,
    ClipboardList,
    Settings,
    Shield,
    ChevronDown,
    ChevronUp
} from 'lucide-react';
import { useLocation } from 'react-router-dom';

const RootLayout = ({ page }) => {
    const location = useLocation();
    const isSpecDepartment = localStorage.getItem('spec') === "true";
    const isAdmin = localStorage.getItem('adm') === "true";

    const baseMenuItems = [
        { icon: <Newspaper size={20} className="nav-icon" />, label: 'Новости', link: "/news" },
        { icon: <MessageSquare size={20} className="nav-icon" />, label: 'Чат', link: "/chat" },
        { icon: <Ticket size={20} className="nav-icon" />, label: 'Заявки', link: "/tickets/my" },
        { icon: <ClipboardList size={20} className="nav-icon" />, label: 'Предложения', link: "/suggestions/my" },
    ];

    const sentItems = [
        { icon: <Send size={20} className="nav-icon" />, label: 'Отправленные заявки', link: "/tickets/sent" },
        { icon: <Send size={20} className="nav-icon" />, label: 'Отправленные предложения', link: "/suggestions/sent" },
    ];

    const adminItem = {
        icon: <Shield size={20} className="nav-icon" />,
        label: 'Панель администратора',
        link: "/admin"
    };

    let items = [...baseMenuItems];

    if (isSpecDepartment || isAdmin) {
        items.push({
            label: 'Отправленные',
            icon: <Send size={20} className="nav-icon" />,
            subItems: sentItems,
            isDropdown: true
        });
    }

    if (isAdmin) {
        items.push(adminItem);
    }

    return (
        <>
            <TopNavigation menuItems={items} />
            {React.cloneElement(page, { key: location.pathname })}
        </>
    );
};

export default RootLayout;