import api from '../api/axios';

export interface Reclamacion {
    id: number;
    pedidoId: number;
    fecha: string;
    motivo: string;
    estado: string;
}

export interface CreateReclamacionDto {
    pedidoId: number;
    motivo: string;
}

export const reclamacionesService = {
    // Obtener mis reclamaciones (Usuario)
    getMisReclamaciones: async () => {
        const response = await api.get('/reclamaciones/mis-reclamaciones');
        return response.data;
    },

    // Obtener todas las reclamaciones (Admin)
    getAllReclamaciones: async () => {
        const response = await api.get('/reclamaciones');
        return response.data;
    },

    // Crear una nueva reclamación (Usuario)
    createReclamacion: async (data: CreateReclamacionDto) => {
        const response = await api.post('/reclamaciones', data);
        return response.data;
    },

    // Actualizar estado de reclamación (Admin)
    updateEstado: async (id: number, estado: string) => {
        const response = await api.put(`/reclamaciones/${id}/estado`, { estado });
        return response.data;
    }
};
