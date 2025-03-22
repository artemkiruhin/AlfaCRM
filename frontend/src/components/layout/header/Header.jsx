import React from 'react';
import "./Header.css"

const Header = ({ title, info }) => {
    return (
        <header className="section-header">
            <h2 className="section-title">{title}</h2>
            <div className="section-info">{info}</div>
        </header>
    );
};

export default Header;