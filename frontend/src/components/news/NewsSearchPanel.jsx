import {Search} from "lucide-react";
import React from "react";

const NewsSearchPanel = ({searchQuery, handleSearchChange, filters, handleFilterChange}) => {
    return (
        <div className="filters-container">
            <div className="search-bar">
                <Search size={18} className="search-icon"/>
                <input
                    type="text"
                    placeholder="Поиск по новостям..."
                    value={searchQuery}
                    onChange={handleSearchChange}
                />
            </div>

            <div className="filters">
                <select
                    name="department"
                    value={filters.department}
                    onChange={handleFilterChange}
                >
                    <option value="">Все отделы</option>
                    <option value="IT отдел">IT отдел</option>
                    <option value="HR отдел">HR отдел</option>
                    <option value="Общая новость">Общая новость</option>
                </select>

                <label className="checkbox-filter">
                    <input
                        type="checkbox"
                        name="isImportant"
                        checked={filters.isImportant}
                        onChange={handleFilterChange}
                    />
                    Только важные
                </label>
            </div>
        </div>
    )
}

export default NewsSearchPanel;