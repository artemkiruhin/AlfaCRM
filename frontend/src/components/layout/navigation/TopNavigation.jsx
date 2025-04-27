import React, { useState, useEffect } from 'react';
import { User, Menu, X, ChevronDown, ChevronUp } from 'lucide-react';
import "./TopNavigation.css";
import { useNavigate, useLocation } from "react-router-dom";

const TopNavigation = ({ menuItems }) => {
    const navigate = useNavigate();
    const location = useLocation();
    const [activeIndex, setActiveIndex] = useState(-1);
    const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
    const [openDropdownIndex, setOpenDropdownIndex] = useState(null);

    useEffect(() => {
        const currentPath = location.pathname;

        let activeItemIndex = menuItems.findIndex(item =>
            item.link && currentPath.startsWith(item.link)
        );

        if (activeItemIndex === -1) {
            menuItems.forEach((item, index) => {
                if (item.subItems) {
                    const subItemIndex = item.subItems.findIndex(subItem =>
                        currentPath.startsWith(subItem.link)
                    );
                    if (subItemIndex !== -1) {
                        activeItemIndex = index;
                    }
                }
            });
        }

        setActiveIndex(activeItemIndex);
    }, [location.pathname, menuItems]);

    const handleItemClick = (index, path, isDropdown) => {
        if (isDropdown) {
            setOpenDropdownIndex(openDropdownIndex === index ? null : index);
        } else {
            setIsMobileMenuOpen(false);
            setOpenDropdownIndex(null);
            navigate(path);
        }
    };

    const toggleMobileMenu = () => {
        setIsMobileMenuOpen(!isMobileMenuOpen);
        if (!isMobileMenuOpen) {
            setOpenDropdownIndex(null);
        }
    };

    const isActive = (index) => {
        if (activeIndex === index) return true;

        const item = menuItems[index];
        if (item.subItems) {
            return item.subItems.some(subItem =>
                location.pathname.startsWith(subItem.link)
            );
        }

        return false;
    };

    return (
        <nav className="top-nav">
            <div className="nav-container">
                <div className="nav-left">
                    <div className="nav-logo">КорпПортал</div>
                    <button
                        className="mobile-menu-button"
                        onClick={toggleMobileMenu}
                        aria-label="Toggle menu"
                    >
                        {isMobileMenuOpen ? <X size={24} /> : <Menu size={24} />}
                    </button>
                </div>

                <div className={`nav-center ${isMobileMenuOpen ? 'open' : ''}`}>
                    <ul className="nav-list">
                        {menuItems.map((item, index) => (
                            <li key={index} className={`nav-list-item ${item.subItems ? 'has-dropdown' : ''}`}>
                                <button
                                    className={`nav-item ${isActive(index) ? 'active' : ''}`}
                                    onClick={() => handleItemClick(index, item.link, item.isDropdown)}
                                >
                                    <span className="nav-icon-wrapper">
                                        {item.icon && React.cloneElement(item.icon, {
                                            className: `nav-icon ${isActive(index) ? 'active' : ''}`
                                        })}
                                    </span>
                                    <span className="nav-label">{item.label}</span>
                                    {item.subItems && (
                                        <span className="dropdown-arrow">
                                            {openDropdownIndex === index ? <ChevronUp size={16} /> : <ChevronDown size={16} />}
                                        </span>
                                    )}
                                </button>

                                {item.subItems && openDropdownIndex === index && (
                                    <ul className="dropdown-menu">
                                        {item.subItems.map((subItem, subIndex) => (
                                            <li key={subIndex}>
                                                <button
                                                    className={`dropdown-item ${location.pathname.startsWith(subItem.link) ? 'active' : ''}`}
                                                    onClick={() => {
                                                        setIsMobileMenuOpen(false);
                                                        navigate(subItem.link);
                                                    }}
                                                >
                                                    <span className="nav-icon-wrapper">
                                                        {React.cloneElement(subItem.icon, {
                                                            className: `nav-icon ${location.pathname.startsWith(subItem.link) ? 'active' : ''}`
                                                        })}
                                                    </span>
                                                    <span className="nav-label">{subItem.label}</span>
                                                </button>
                                            </li>
                                        ))}
                                    </ul>
                                )}
                            </li>
                        ))}
                    </ul>
                </div>

                <div className="nav-right">
                    <button className="nav-profile">
                        <span className="nav-icon-wrapper">
                            <User size={20} className="nav-icon" />
                        </span>
                        <span className="nav-label">Профиль</span>
                    </button>
                </div>
            </div>
        </nav>
    );
};

export default TopNavigation;