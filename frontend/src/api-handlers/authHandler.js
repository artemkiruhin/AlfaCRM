import {API_URL} from "./baseHandler";

const login = async (username, password) => {
    try {
        const response = await fetch(`${API_URL}/auth/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                username: username,
                passwordHash: password
            }),
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Login error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()

        return {
            id: data.id,
            token: data.token,
            department: data.department,
            username: data.username
        }

    } catch (e) {
        console.error('Login error: ', e);
    }
}

const logout = async () => {
    try {
        const response = await fetch(`${API_URL}/auth/logout`, {
            method: 'POST',
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Logout error: ${response.statusText} | ${response.status}`)
            return false
        }

        return true
    } catch (e) {
        console.error('Logout error: ', e);
        return false
    }
}

const validate = async () => {
    try {
        const response = await fetch(`${API_URL}/auth/validate`, {
            method: 'GET',
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Validation error: ${response.statusText} | ${response.status}`)
            return false
        }

        return true
    } catch (e) {
        console.error('Validation error: ', e);
        return false
    }
}

const validateAdminOrPublisher = async () => {
    try {
        const response = await fetch(`${API_URL}/auth/validate-admin-or-author`, {
            method: 'GET',
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Validation error: ${response.statusText} | ${response.status}`)
            return false
        }

        return true
    } catch (e) {
        console.error('Validation error: ', e);
        return false
    }
}


export {
    login,
    logout,
    validate,
    validateAdminOrPublisher,
}