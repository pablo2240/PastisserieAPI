import api from '../api/axios';

// Interface para Review
export interface Review {
  id: number;
  productoId: number;
  productoNombre?: string;
  usuarioId: number;
  usuarioNombre: string;
  calificacion: number;
  comentario: string;
  fechaCreacion: string;
  aprobada?: boolean;
}

export const reviewService = {
  // Obtener reseñas de un producto
  getByProduct: async (productoId: number) => {
    const response = await api.get(`/Reviews/producto/${productoId}`);
    return response.data.data || response.data;
  },

  // Crear una nueva reseña
  create: async (productoId: number, calificacion: number, comentario: string) => {
    const payload = {
      ProductoId: productoId,
      Calificacion: calificacion,
      Comentario: comentario
    };
    const response = await api.post('/Reviews', payload);
    return response.data;
  },

  // Editar reseña existente
  update: async (id: number, calificacion: number, comentario: string) => {
    const payload = {
      Calificacion: calificacion,
      Comentario: comentario
    };
    const response = await api.put(`/Reviews/${id}`, payload);
    return response.data;
  },

  // Obtener reseñas pendientes de moderación (Admin)
  getPending: async () => {
    const response = await api.get('/Reviews/pendientes');
    return response.data.data || response.data;
  },

  // Aprobar una reseña (Admin)
  approve: async (id: number) => {
    const response = await api.put(`/Reviews/${id}/aprobar`);
    return response.data;
  },

  // Eliminar una reseña (Admin)
  delete: async (id: number) => {
    const response = await api.delete(`/Reviews/${id}`);
    return response.data;
  },

  // Obtener reseñas destacadas (Home)
  getFeatured: async () => {
    try {
      const response = await api.get('/Reviews');
      const data = response.data.data || response.data;
      return Array.isArray(data) ? data : [];
    } catch {
      return [];
    }
  }
};
