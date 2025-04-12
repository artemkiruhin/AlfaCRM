import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import SuggestionList from '../../components/ticket/SuggestionList';
import CreateSuggestionButton from '../../components/ticket/CreateSuggestionButton';
import './MySuggestionsPage.css';
import Header from "../../components/layout/header/Header";
import { getUserTickets } from '../../api-handlers/ticketsHandler';

const MySuggestionsPage = () => {
    const navigate = useNavigate();
    const [suggestions, setSuggestions] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchQuery, setSearchQuery] = useState('');
    const [showRejected, setShowRejected] = useState(false);

    const onCreatePageHandler = () => {
        navigate("/suggestions/create");
    };

    useEffect(() => {
        const fetchSuggestions = async () => {
            try {
                setLoading(true);
                // Используем type=1 для получения предложений (вместо заявок)
                const data = await getUserTickets(false, 1);
                console.log(data);
                setSuggestions(data.map(suggestion => ({
                    id: suggestion.id,
                    title: suggestion.title,
                    text: suggestion.text,
                    department: suggestion.department.name,
                    date: new Date(suggestion.createdAt).toLocaleString(),
                    status: suggestion.status,
                    feedback: suggestion.feedback,
                    assignee: suggestion.assignee ? suggestion.assignee.username : 'Не назначен',
                    closedAt: suggestion.closedAt ? new Date(suggestion.closedAt).toLocaleString() : null,
                    creator: suggestion.creator.username
                })));
            } catch (err) {
                console.error('Failed to fetch suggestions:', err);
            } finally {
                setLoading(false);
            }
        };

        fetchSuggestions();
    }, []);

    const filteredSuggestions = suggestions.filter(suggestion => {
        const matchesSearch = suggestion.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
            suggestion.text.toLowerCase().includes(searchQuery.toLowerCase());
        const matchesStatus = showRejected ? true : suggestion.status !== 'Отклонено';
        return matchesSearch && matchesStatus;
    });

    return (
        <div className="my-suggestions-page">
            <Header title={"Мои предложения"} info={`Всего: ${filteredSuggestions.length}`} />

            <div className="filter-container">
                <input
                    type="text"
                    placeholder="Поиск по предложениям..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    className="filter-input"
                />
                <label className="filter-checkbox">
                    <input
                        type="checkbox"
                        checked={showRejected}
                        onChange={() => setShowRejected(!showRejected)}
                    />
                    Показать отклоненные
                </label>
                <CreateSuggestionButton onCreatePageHandler={onCreatePageHandler} />
            </div>

            <SuggestionList tickets={filteredSuggestions} />
        </div>
    );
};

export default MySuggestionsPage;