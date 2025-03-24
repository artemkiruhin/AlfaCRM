import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './DepartmentListPage.css';
import { Plus, Pencil, Trash2, Trash } from 'lucide-react';
import Modal from '../../components/layout/modal/base/Modal';
import Header from "../../components/layout/header/Header";
import ConfirmationModal from '../../components/layout/modal/confirmation/ConfirmationModal';

const DepartmentListPage = () => {
    const [departments, setDepartments] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [isAddModalOpen, setIsAddModalOpen] = useState(false);
    const [isEditModalOpen, setIsEditModalOpen] = useState(false);
    const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
    const [currentDepartment, setCurrentDepartment] = useState(null);
    const [departmentName, setDepartmentName] = useState('');

    useEffect(() => {
        const fetchDepartments = async () => {
            try {
                const mockDepartments = [
                    { id: '1', name: 'Разработка', memberCount: 12 },
                    { id: '2', name: 'Дизайн', memberCount: 8 },
                    { id: '3', name: 'Маркетинг', memberCount: 6 },
                ];

                setDepartments(mockDepartments);
                setIsLoading(false);
            } catch (error) {
                console.error('Ошибка при загрузке отделов:', error);
                setIsLoading(false);
            }
        };

        fetchDepartments();
    }, []);

    const handleAddDepartment = () => {
        setDepartmentName('');
        setIsAddModalOpen(true);
    };

    const handleEditDepartment = (department) => {
        setCurrentDepartment(department);
        setDepartmentName(department.name);
        setIsEditModalOpen(true);
    };

    const handleDeleteClick = (department) => {
        setCurrentDepartment(department);
        setIsDeleteModalOpen(true);
    };

    const handleDeleteDepartment = async () => {
        if (!currentDepartment) return;

        try {
            console.log('Удаление отдела с ID:', currentDepartment.id);
            setDepartments(departments.filter(dept => dept.id !== currentDepartment.id));
            setIsDeleteModalOpen(false);
            alert('Отдел успешно удален');
        } catch (error) {
            console.error('Ошибка при удалении отдела:', error);
            alert('Произошла ошибка при удалении отдела');
        }
    };

    const handleSubmitAdd = async (e) => {
        e.preventDefault();
        if (!departmentName.trim()) return;

        try {
            const newDepartment = {
                id: String(Math.max(...departments.map(d => parseInt(d.id))) + 1),
                name: departmentName,
                memberCount: 0
            };

            setDepartments([...departments, newDepartment]);
            setIsAddModalOpen(false);
            alert('Отдел успешно добавлен');
        } catch (error) {
            console.error('Ошибка при добавлении отдела:', error);
            alert('Произошла ошибка при добавлении отдела');
        }
    };

    const handleSubmitEdit = async (e) => {
        e.preventDefault();
        if (!departmentName.trim() || !currentDepartment) return;

        try {
            console.log('Обновление отдела:', currentDepartment.id, departmentName);

            setDepartments(departments.map(dept =>
                dept.id === currentDepartment.id
                    ? { ...dept, name: departmentName }
                    : dept
            ));

            setIsEditModalOpen(false);
            alert('Отдел успешно обновлен');
        } catch (error) {
            console.error('Ошибка при обновлении отдела:', error);
            alert('Произошла ошибка при обновлении отдела');
        }
    };

    return (
        <div className="department-list-page">
            <Header title={"Управление отделами"}/>
            <div className="page-header">
                <button
                    className="add-button"
                    onClick={handleAddDepartment}
                >
                    <Plus size={18} />
                    Добавить отдел
                </button>
            </div>

            {isLoading ? (
                <div className="loading">Загрузка...</div>
            ) : departments.length === 0 ? (
                <div className="empty-state">
                    <p>Нет созданных отделов</p>
                </div>
            ) : (
                <div className="departments-table-container">
                    <table className="departments-table">
                        <thead>
                        <tr>
                            <th>Название отдела</th>
                            <th>Количество сотрудников</th>
                            <th>Действия</th>
                        </tr>
                        </thead>
                        <tbody>
                        {departments.map(department => (
                            <tr key={department.id}>
                                <td>{department.name}</td>
                                <td>{department.memberCount}</td>
                                <td>
                                    <div className="actions">
                                        <button
                                            className="edit-button"
                                            onClick={() => handleEditDepartment(department)}
                                            title="Редактировать"
                                        >
                                            <Pencil size={16} />
                                        </button>
                                        <button
                                            className="edit-button"
                                            onClick={() => handleDeleteClick(department)}
                                            title="Удалить"
                                        >
                                            <Trash2 size={16}/>
                                        </button>
                                    </div>
                                </td>
                            </tr>
                        ))}
                        </tbody>
                    </table>
                </div>
            )}

            <Modal
                isOpen={isAddModalOpen}
                onClose={() => setIsAddModalOpen(false)}
                title="Добавление нового отдела"
            >
                <form onSubmit={handleSubmitAdd} className="department-form">
                    <div className="form-group">
                        <label htmlFor="department-name">Название отдела*</label>
                        <input
                            type="text"
                            id="department-name"
                            value={departmentName}
                            onChange={(e) => setDepartmentName(e.target.value)}
                            required
                            placeholder="Введите название отдела"
                            autoFocus
                        />
                    </div>
                    <div className="form-actions">
                        <button
                            type="button"
                            className="cancel-button"
                            onClick={() => setIsAddModalOpen(false)}
                        >
                            Отмена
                        </button>
                        <button type="submit" className="submit-button">
                            Добавить
                        </button>
                    </div>
                </form>
            </Modal>

            <Modal
                isOpen={isEditModalOpen}
                onClose={() => setIsEditModalOpen(false)}
                title={`Редактирование отдела ${currentDepartment?.name || ''}`}
            >
                <form onSubmit={handleSubmitEdit} className="department-form">
                    <div className="form-group">
                        <label htmlFor="edit-department-name">Название отдела*</label>
                        <input
                            type="text"
                            id="edit-department-name"
                            value={departmentName}
                            onChange={(e) => setDepartmentName(e.target.value)}
                            required
                            placeholder="Введите название отдела"
                            autoFocus
                        />
                    </div>
                    <div className="form-actions">
                        <button
                            type="button"
                            className="cancel-button"
                            onClick={() => setIsEditModalOpen(false)}
                        >
                            Отмена
                        </button>
                        <button type="submit" className="submit-button">
                            Сохранить
                        </button>
                    </div>
                </form>
            </Modal>

            <ConfirmationModal
                isOpen={isDeleteModalOpen}
                onClose={() => setIsDeleteModalOpen(false)}
                onConfirm={handleDeleteDepartment}
                title="Подтверждение удаления"
                message={`Вы уверены, что хотите удалить отдел "${currentDepartment?.name}"?`}
                confirmText="Удалить"
                cancelText="Отмена"
            />
        </div>
    );
};

export default DepartmentListPage;