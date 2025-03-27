import {API_URL} from "./baseHandler";

const getAllPosts = async (departmentId) => {
    try {
        let url = departmentId ? `${API_URL}/posts?departmentId=${departmentId}` : `${API_URL}/posts`

        const response = await fetch(url, {
            method: 'GET',
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Getting all posts error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()
        return data.posts

    } catch (e) {
        console.error('Getting all posts error: ', e);
    }
}
const getPostById = async (id) => {
    try {
        const response = await fetch(`${API_URL}/posts/id/${id}`, {
            method: 'GET',
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Getting post by id error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()

        console.log(data.post)

        return data.post

    } catch (e) {
        console.error('Getting post by id  error: ', e);
    }
}
const editPost = async (postId, title, subtitle, content, isImportant, departmentId, editDepartment) => {
    try {
        const body = {
            PostId: postId,
            Title: title,
            Subtitle: subtitle,
            Content: content,
            IsImportant: isImportant,
            DepartmentId: departmentId,
            EditDepartment: editDepartment
        };

        const cleanedBody = Object.fromEntries(
            Object.entries(body).filter(([_, value]) => value !== undefined)
        );

        const response = await fetch(`${API_URL}/posts/edit`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(cleanedBody),
            credentials: 'include'
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
        }

        return await response.json();

    } catch (e) {
        console.error('Editing post error:', e);
        throw e;
    }
};
const deletePost = async (id) => {
    try {
        const response = await fetch(`${API_URL}/posts/delete/${id}`, {
            method: 'DELETE',
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Deleting post error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()
        return data.id

    } catch (e) {
        console.error('Deleting post error: ', e);
    }
}
const blockPost = async (id) => {
    try {
        const response = await fetch(`${API_URL}/posts/block/${id}`, {
            method: 'POST',
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Blocking post error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()
        return data.id

    } catch (e) {
        console.error('Blocking post error: ', e);
    }
}
const createPost = async (title, subtitle, content, isImportant, departmentId) => {
    try {
        let body = {
            title: title,
            content: content,
            isImportant: isImportant
        }

        if (subtitle) {
            body.subtitle = subtitle
        }
        if (departmentId) {
            body.departmentId = departmentId
        }

        const response = await fetch(`${API_URL}/posts/create`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(body),
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Creating post error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()
        return data.id

    } catch (e) {
        console.error('Creating post error: ', e);
    }
}

// type: 0 - LIKE | 1 - DISLIKE
const reactPost = async (postId, type) => {
    try {
        const response = await fetch(`${API_URL}/posts/react`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                postId: postId,
                type: type
            }),
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Reacting post error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()
        return data.id

    } catch (e) {
        console.error('Reacting post error: ', e);
    }
}
const deleteReact = async (id) => {
    try {
        const response = await fetch(`${API_URL}/posts/delete-react/${id}`, {
            method: 'DELETE',
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Deleting react error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()
        return data.id

    } catch (e) {
        console.error('Deleting react error: ', e);
    }
}
const deleteAllReactsByPost = async (id) => {
    try {
        const response = await fetch(`${API_URL}/posts/delete-post-all-reacts/${id}`, {
            method: 'DELETE',
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Deleting react error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()
        return data.id

    } catch (e) {
        console.error('Deleting react error: ', e);
    }
}
const createComment = async (content, postId) => {
    try {
        const response = await fetch(`${API_URL}/posts/add-comment`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                content: content,
                postId: postId
            }),
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Creating comment error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()
        return data.id

    } catch (e) {
        console.error('Creating comment error: ', e);
    }
}
const deleteComment = async (id) => {
    try {
        const response = await fetch(`${API_URL}/posts/remove-comment/${id}`, {
            method: 'DELETE',
            credentials: 'include'
        })

        if (!response.ok) {
            console.error(`Deleting comment error: ${response.statusText} | ${response.status}`)
        }

        const data = await response.json()
        return data.id

    } catch (e) {
        console.error('Deleting comment error: ', e);
    }
}

export {
    getAllPosts,
    getPostById,
    editPost,
    deletePost,
    blockPost,
    createPost,
    reactPost,
    deleteReact,
    createComment,
    deleteComment,
    deleteAllReactsByPost
}