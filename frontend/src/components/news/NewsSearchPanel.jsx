import {Search} from "lucide-react";
import React from "react";

const NewsSearchPanel = ({searchQuery, handleSearchChange, filters, handleFilterChange, departments}) => {
    return (
        <div className="filters-container">
            <div className="search-bar">
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
                    {departments.map(department => (
                        <option key={department.id} value={department.id}>{department.name}</option>
                    ))}
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