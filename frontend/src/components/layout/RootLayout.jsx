import React from "react";
import TopNavigation from "./navigation/TopNavigation";
import { Home, Users, Calendar, FileText, Settings } from 'lucide-react';

const RootLayout = ({page}) => {
    const emplMenuItems = [
        { icon: <FileText size={20} className="nav-icon" />, label: 'Новости', link: "/news" },
        { icon: <Users size={20} className="nav-icon" />, label: 'Чат', link: "/chat" },
        { icon: <Calendar size={20} className="nav-icon" />, label: 'Заявки', link: "/tickets/my" },
        { icon: <Settings size={20} className="nav-icon" />, label: 'Предложения', link: "/suggestions/my" },
    ];

    return (
        <>
            <TopNavigation menuItems={emplMenuItems} />
            {page}
        </>
    )
}

export default RootLayout;