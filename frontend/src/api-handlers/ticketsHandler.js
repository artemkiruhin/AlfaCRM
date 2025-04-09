import { API_URL } from "./baseHandler";

const getAllTickets = async (departmentId, isShort, type) => {
    try {
        let url = `${API_URL}/tickets`;
        const params = new URLSearchParams();

        if (departmentId) params.append('departmentId', departmentId);
        if (isShort) params.append('isShort', isShort);
        if (type) params.append('type', type);

        if (params.toString()) url += `?${params.toString()}`;

        const response = await fetch(url, {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();

        if (result && result.data && Array.isArray(result.data)) {
            return result.data;
        } else if (Array.isArray(result)) {
            return result;
        } else {
            throw new Error('Некорректный формат данных от сервера');
        }

    } catch (e) {
        console.error('Getting all tickets error: ', e);
        throw e;
    }
};

const getUserTickets = async (isShort, type) => {
    try {
        const url = `${API_URL}/tickets/my?isShort=${isShort}&type=${type}`;

        const response = await fetch(url, {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) {
            console.error(`Getting user tickets error: ${response.statusText} | ${response.status}`);
            console.error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        return data.data;

    } catch (e) {
        console.error('Getting user tickets error: ', e);
        throw e;
    }
};

const getTicketById = async (id) => {
    try {
        const url = `${API_URL}/tickets/id/${id}`;

        const response = await fetch(url, {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        return data.data;

    } catch (e) {
        console.error('Getting ticket error: ', e);
        throw e;
    }
};


const createTicket = async (title, text, departmentId, type) => {
    try {
        const response = await fetch(`${API_URL}/tickets/create`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                Title: title,
                Text: text,
                DepartmentId: departmentId,
                Type: type
            }),
            credentials: 'include'
        });

        if (!response.ok) {
            const errorData = await response.json();
            console.error(errorData.message || `HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        return data.data;

    } catch (e) {
        console.error('Creating ticket error:', e);
        throw e;
    }
};

const editTicket = async (id, title, text, departmentId, feedback, type) => {
    try {
        const body = {
            Id: id,
            Title: title,
            Text: text,
            DepartmentId: departmentId,
            Feedback: feedback,
            Type: type
        };

        const cleanedBody = Object.fromEntries(
            Object.entries(body).filter(([_, value]) => value !== undefined && value !== null)
        );

        const response = await fetch(`${API_URL}/tickets/edit`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(cleanedBody),
            credentials: 'include'
        });

        if (!response.ok) {
            const errorData = await response.json();
            console.error(errorData.message || `HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        return data.data;

    } catch (e) {
        console.error('Editing ticket error:', e);
        throw e;
    }
};

const changeTicketStatus = async (id, status, feedback) => {
    try {
        const body = {
            Id: id,
            Status: status,
            Feedback: feedback
        };

        const cleanedBody = Object.fromEntries(
            Object.entries(body).filter(([_, value]) => value !== undefined && value !== null)
        );

        const response = await fetch(`${API_URL}/tickets/change-status`, {
            method: 'PATCH',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(cleanedBody),
            credentials: 'include'
        });

        if (!response.ok) {
            const errorData = await response.json();
            console.error(errorData.message || `HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        return data.data;

    } catch (e) {
        console.error('Changing ticket status error:', e);
        throw e;
    }
};

const deleteTicket = async (id) => {
    try {
        const response = await fetch(`${API_URL}/tickets/delete/${id}`, {
            method: 'DELETE',
            credentials: 'include'
        });

        if (!response.ok) {
            const errorData = await response.json();
            console.error(errorData.message || `HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        return data.data;

    } catch (e) {
        console.error('Deleting ticket error:', e);
        throw e;
    }
};

export {
    getAllTickets,
    getUserTickets,
    createTicket,
    editTicket,
    changeTicketStatus,
    deleteTicket,
    getTicketById
};