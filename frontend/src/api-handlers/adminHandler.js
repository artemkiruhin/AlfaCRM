import {API_URL} from "./baseHandler";

const getStats = async () => {
    try {
        const response = await fetch(`${API_URL}/admin/stats`, {
            method: 'GET',
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Getting stats error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json();
        return data.data;
    } catch (e) {
        console.error('Getting stats error: ', e);
    }
}

export {
    getStats,
}