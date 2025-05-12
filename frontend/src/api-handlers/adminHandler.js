import { API_URL } from "./baseHandler";

const getStats = async () => {
    try {
        const response = await fetch(`${API_URL}/admin/stats`, {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) {
            console.error(`Ошибка получения статистики: ${response.statusText} | ${response.status}`);
            throw new Error(`Ошибка HTTP: ${response.status}`);
        }

        const data = await response.json();
        return data.data;
    } catch (e) {
        console.error('Ошибка при получении статистики: ', e);
        throw e;
    }
};

const getBusinessStats = async () => {
    try {
        const response = await fetch(`${API_URL}/admin/businesses`, {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) {
            console.error(`Ошибка получения статистики бизнеса: ${response.statusText} | ${response.status}`);
            throw new Error(`Ошибка HTTP: ${response.status}`);
        }

        const data = await response.json();
        return data.data;
    } catch (e) {
        console.error('Ошибка при получении статистики бизнеса: ', e);
        throw e;
    }
};

const getLogs = async () => {
    try {
        const response = await fetch(`${API_URL}/admin/logs`, {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) {
            console.error(`Ошибка получения логов: ${response.statusText} | ${response.status}`);
            throw new Error(`Ошибка HTTP: ${response.status}`);
        }

        const data = await response.json();
        return data.data;
    } catch (e) {
        console.error('Ошибка при получении логов: ', e);
        throw e;
    }
};

const getUsersWorkload = async () => {
    try {
        const response = await fetch(`${API_URL}/admin/users/workload`, {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) {
            console.error(`Ошибка получения загруженности пользователей: ${response.statusText} | ${response.status}`);
            throw new Error(`Ошибка HTTP: ${response.status}`);
        }

        const data = await response.json();
        return data.data;
    } catch (e) {
        console.error('Ошибка при получении загруженности пользователей: ', e);
        throw e;
    }
};

const assignTicketToUser = async (userId, ticketId) => {
    try {
        const response = await fetch(`${API_URL}/admin/tickets/assign`, {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                userId,
                ticketId
            })
        });

        if (!response.ok) {
            console.error(`Ошибка назначения заявки: ${response.statusText} | ${response.status}`);
            throw new Error(`Ошибка HTTP: ${response.status}`);
        }

        const data = await response.json();
        return data.success;
    } catch (e) {
        console.error('Ошибка при назначении заявки пользователю: ', e);
        throw e;
    }
};

const distributeTickets = async () => {
    try {
        const response = await fetch(`${API_URL}/admin/tickets/distribute`, {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json',
            }
        });

        if (!response.ok) {
            console.error(`Ошибка распределения заявок: ${response.statusText} | ${response.status}`);
            throw new Error(`Ошибка HTTP: ${response.status}`);
        }

        const data = await response.json();
        return data.success;
    } catch (e) {
        console.error('Ошибка при распределении заявок: ', e);
        throw e;
    }
};

export {
    getStats,
    getBusinessStats,
    getLogs,
    getUsersWorkload,
    assignTicketToUser,
    distributeTickets
};