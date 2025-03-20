import {API_URL} from "./baseHandler";

const getAllDepartments = async (isShort) => {
    try {

        let url = isShort === true ? `${API_URL}/departments?isShort=true` : `${API_URL}/departments`

        const response = await fetch(url, {
            method: 'GET',
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Getting all deps error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()

        return data.departments

    } catch (e) {
        console.error('Getting all deps error: ', e);
    }
}
const getDepartmentById = async (id) => {
    try {
        const response = await fetch(`${API_URL}/departments/id/${id}`, {
            method: 'GET',
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Getting dep by id error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()

        return data.department

    } catch (e) {
        console.error('Getting dep by id error: ', e);
    }
}
const createDepartment = async (name) => {
    try {
        const response = await fetch(`${API_URL}/departments/create`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                name: name
            }),
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Creating dep error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()

        return data.id

    } catch (e) {
        console.error('Creating dep error: ', e);
    }
}
const editDepartment = async (id, name) => {
    try {
        const response = await fetch(`${API_URL}/departments/edit`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                departmentId: id,
                name: name
            }),
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Editing dep error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()

        return data.id

    } catch (e) {
        console.error('Editing dep error: ', e);
    }
}
const deleteDepartment = async (id) => {
    try {
        const response = await fetch(`${API_URL}/departments/delete/${id}`, {
            method: 'DELETE',
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Deleting dep error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()

        return data.id

    } catch (e) {
        console.error('Deleting dep error: ', e);
    }
}

export {
    getAllDepartments,
    getDepartmentById,
    createDepartment,
    editDepartment,
    deleteDepartment,
}