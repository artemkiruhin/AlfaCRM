
const formatDate = (dateString) => {
    if (!dateString) return 'только что';
    if (new Date() - new Date(dateString) < 5000) {
        return 'только что';
    }

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