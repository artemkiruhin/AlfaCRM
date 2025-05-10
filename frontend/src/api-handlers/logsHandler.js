import {API_URL} from "./baseHandler";

const getAllLogs = async () => {
    try {

        let url = `${API_URL}/log/`
        const response = await fetch(url, {
            method: 'GET',
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Getting all logs error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()
        return data.data

    } catch (e) {
        console.error('Getting all logs error: ', e);
    }
}

export {
    getAllLogs,
}