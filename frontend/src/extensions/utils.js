
const formatDate = (dateString) => {
    if (!(dateString instanceof Date)) {
        dateString = new Date(dateString);
    }
    const options = {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    };
    return new Date(dateString).toLocaleDateString('ru-RU', options);
};

export {
    formatDate,
}