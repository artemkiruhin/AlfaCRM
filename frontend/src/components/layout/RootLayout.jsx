import React from "react";
import TopNavigation from "./navigation/TopNavigation";
import { Home, Users, Calendar, FileText, Settings } from 'lucide-react';

const RootLayout = ({page}) => {
    const menuItems = [
        { icon: <Home size={20} className="nav-icon" />, label: 'Главная' },
        { icon: <FileText size={20} className="nav-icon" />, label: 'Новости' },
        { icon: <Users size={20} className="nav-icon" />, label: 'Сотрудники' },
        { icon: <Calendar size={20} className="nav-icon" />, label: 'Календарь' },
        { icon: <Settings size={20} className="nav-icon" />, label: 'Настройки' },
    ];

    return (
        <>
            <TopNavigation menuItems={menuItems} />
            {page}
        </>
    )
}

export default RootLayout;