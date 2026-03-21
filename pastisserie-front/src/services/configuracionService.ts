import api from '../api/axios';

export interface ConfiguracionTienda {
    nombreTienda: string;
    direccion: string;
    telefono: string;
    emailContacto: string;
    costoEnvio: number;
    moneda: string;
    mensajeBienvenida: string;
    horarioApertura?: string;
    horarioCierre?: string;
    horarioActivo?: boolean;
    instagramUrl?: string;
    facebookUrl?: string;
    whatsappUrl?: string;
}

export const configuracionService = {
    getConfig: async () => {
        const response = await api.get('/configuracion');
        return response.data;
    },

    updateConfig: async (data: ConfiguracionTienda) => {
        const response = await api.put('/configuracion', data);
        return response.data;
    }
};
