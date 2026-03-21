import api from '../api/axios';

export const dashboardService = {
  // Obtener estadísticas para el Admin
  getAdminStats: async () => {
    const response = await api.get('/dashboard/admin');
    return response.data.data;
  },

  // Obtener estadísticas para el Repartidor
  getRepartidorStats: async () => {
    const response = await api.get('/dashboard/repartidor');
    return response.data.data;
  },

  // Obtener historial de ganancias
  getEarningsHistory: async (startDate?: string, endDate?: string) => {
    const params = new URLSearchParams();
    if (startDate) params.append('start', startDate);
    if (endDate) params.append('end', endDate);
    const response = await api.get(`/dashboard/earnings-history?${params.toString()}`);
    return response.data.data;
  }
};