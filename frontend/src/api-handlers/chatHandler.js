import { API_URL } from "./baseHandler";

const getAllChats = async () => {
    try {
        const response = await fetch(`${API_URL}/chat`, {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) {
            console.error(`Getting all chats error: ${response.statusText} | ${response.status}`);
            return undefined;
        }

        const data = await response.json();
        return data.data;
    } catch (e) {
        console.error('Getting all chats error: ', e);
        return undefined;
    }
};

const getChatById = async (id, byUser = false) => {
    try {
        const response = await fetch(`${API_URL}/chat/id/${id}?byUser=${byUser.toString()}`, {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) {
            console.error(`Getting chat by id error: ${response.statusText} | ${response.status}`);
            return undefined;
        }

        const data = await response.json();
        return data.data;
    } catch (e) {
        console.error('Getting chat by id error: ', e);
        return undefined;
    }
};

const createChat = async (request) => {
    try {
        const response = await fetch(`${API_URL}/chat/create-chat`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(request),
            credentials: 'include'
        });

        if (!response.ok) {
            console.error(`Creating chat error: ${response.statusText} | ${response.status}`);
            return undefined;
        }

        const data = await response.json();
        return data.data;
    } catch (e) {
        console.error('Creating chat error: ', e);
        return undefined;
    }
};

const sendMessage = async (request) => {
    try {
        const response = await fetch(`${API_URL}/chat/send-message`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(request),
            credentials: 'include'
        });

        if (!response.ok) {
            console.error(`Sending message error: ${response.statusText} | ${response.status}`);
            return undefined;
        }

        const data = await response.json();
        return data.data;
    } catch (e) {
        console.error('Sending message error: ', e);
        return undefined;
    }
};

const getAllMessages = async (chatId) => {
    try {
        const response = await fetch(`${API_URL}/chat/messages?chatId=${chatId}`, {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) {
            console.error(`Getting all messages error: ${response.statusText} | ${response.status}`);
            return undefined;
        }

        const data = await response.json();
        return data.data;
    } catch (e) {
        console.error('Getting all messages error: ', e);
        return undefined;
    }
};


export {
    getAllChats,
    getChatById,
    createChat,
    sendMessage,
    getAllMessages
};