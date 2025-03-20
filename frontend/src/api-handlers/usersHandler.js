import {API_URL} from "./baseHandler";

const createUser = async (email, username, passwordHash, hiredAt, birthday, isMale, isAdmin, hasPublishedRights, departmentId) => {
    try {
        let body = {
            email: email,
            username: username,
            passwordHash: passwordHash,
            birthday: birthday,
            isMale: isMale,
            isAdmin: isAdmin,
            hasPublishedRights: hasPublishedRights,
            departmentId: departmentId
        }

        if (hiredAt) {
            body.hiredAt = hiredAt
        }

        const response = await fetch(`${API_URL}/users/create`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(body),
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Creating user error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()
        return data.id

    } catch (e) {
        console.error('Creating user error: ', e);
    }
}
const blockUser = async (id) => {
    try {
        const response = await fetch(`${API_URL}/users/block/${id}`, {
            method: 'POST',
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Blocking user error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()
        return data.id

    } catch (e) {
        console.error('Blocking user error: ', e);
    }
}
const resetPassword = async (userId, newPassword, mustValidate, oldPassword) => {
    try {
        let body = {
            userId: userId,
            newPassword: newPassword,
            mustValidate: mustValidate
        }

        if (mustValidate === true && oldPassword) {
            body.oldPassword = oldPassword
        }

        const response = await fetch(`${API_URL}/users/reset-password`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(body),
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Resetting password error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()
        return data.id

    } catch (e) {
        console.error('Resetting password error: ', e);
    }
}
const editUser = async (id, email, isAdmin, hasPublishedRights, departmentId) => {
    try {
        let body = {
           id: id
        }

        if (email) {
            body.email = email
        }
        if (isAdmin) {
            body.isAdmin = isAdmin
        }
        if (hasPublishedRights) {
            body.hasPublishedRights = hasPublishedRights
        }
        if (departmentId) {
            body.departmentId = departmentId
        }
        const response = await fetch(`${API_URL}/users/edit`, {
            method: 'PATCH',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(body),
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Editing user error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()
        return data.id

    } catch (e) {
        console.error('Editing user error: ', e);
    }
}
const deleteUser = async (id) => {
    try {
        const response = await fetch(`${API_URL}/users/delete/${id}`, {
            method: 'DELETE',
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Deleting user error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()
        return data.id

    } catch (e) {
        console.error('Deleting user error: ', e);
    }
}
const getAllUsers = async (isShort) => {
    try {
        let url = isShort === true ? `${API_URL}/users?isShort=true` : `${API_URL}/users`

        const response = await fetch(url, {
            method: 'GET',
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Getting all users error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()

        return data.users

    } catch (e) {
        console.error('Deleting user error: ', e);
    }
}
const getUserById = async (id) => {
    try {
        const response = await fetch(`${API_URL}/users/id/${id}`, {
            method: 'GET',
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Getting user by id error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()
        return data.user

    } catch (e) {
        console.error('Deleting user by id error: ', e);
    }
}

export {
    createUser,
    blockUser,
    resetPassword,
    editUser,
    deleteUser,
    getAllUsers,
    getUserById,
}