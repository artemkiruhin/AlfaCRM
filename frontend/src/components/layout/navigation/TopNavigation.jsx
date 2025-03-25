import React, { useState } from 'react';
import { Home, Users, Calendar, FileText, User, Menu } from 'lucide-react';
import "./TopNavigation.css";
import {useNavigate} from "react-router-dom";

const TopNavigation = ({ menuItems }) => {
    const navigate = useNavigate();
    const [active, setActive] = useState(-1);
    const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

    const handleItemClick = (index) => {
        setIsMobileMenuOpen(false);
        const path = menuItems[index].link;
        navigate(path)
    };

    const toggleMobileMenu = () => {
        setIsMobileMenuOpen(!isMobileMenuOpen);
    };

    return (
        <nav className="top-nav">
            <div className="nav-logo">КорпПортал</div>

            <button className="mobile-menu-icon" onClick={toggleMobileMenu}>
                <Menu size={24} />
            </button>

            <div className={`nav-center ${isMobileMenuOpen ? 'open' : ''}`}>
                <ul className="nav-list">
                    {menuItems.map((item, index) => (
                        <li key={index}>
                            <a
                                href="#"
                                className={`nav-item ${active === index ? 'active' : ''}`}
                                onClick={() => handleItemClick(index)}
                            >
                                {item.icon}
                                <span>{item.label}</span>
                            </a>
                        </li>
                    ))}
                </ul>
            </div>

            <div className="nav-right">
                <a href="#" className="nav-item">
                    <User size={20} className="nav-icon" />
                    <span>Профиль</span>
                </a>
            </div>
        </nav>
    );
};

export default TopNavigation;