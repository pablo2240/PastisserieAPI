import { useState, useEffect } from 'react';
import api from '../api/axios';

export interface TiendaStatus {
    estaAbierto: boolean;
    horaApertura?: string;
    horaCierre?: string;
    sistemaActivoManual?: boolean;
    usarControlHorario?: boolean;
    horariosPorDia?: {
        diaSemana: number;
        abierto: boolean;
        horaApertura: string;
        horaCierre: string;
    }[];
}

export const useTiendaStatus = () => {
    const [status, setStatus] = useState<TiendaStatus | null>(null);
    const [loading, setLoading] = useState(true);

    const fetchStatus = async () => {
        try {
            const response = await api.get('/tienda/estado');
            if (response.data?.data) {
                setStatus(response.data.data);
            }
        } catch (error) {
            console.error('Error al obtener estado de la tienda', error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchStatus();
        // Opcional: Refrescar cada minuto para horario automático
        const interval = setInterval(fetchStatus, 60000);
        return () => clearInterval(interval);
    }, []);

    return { status, loading, refresh: fetchStatus };
};
