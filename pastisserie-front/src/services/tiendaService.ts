import api from '../api/axios';

export interface HorarioDia {
    diaSemana: number;
    abierto: boolean;
    horaApertura: string;
    horaCierre: string;
}

export interface TiendaEstado {
    estaAbierto: boolean;
    horaApertura: string;
    horaCierre: string;
    sistemaActivoManual: boolean;
    usarControlHorario: boolean;
    diasLaborales: string;
    horariosPorDia: HorarioDia[];
}

export const tiendaService = {
    getEstado: async () => {
        const response = await api.get('/tienda/estado');
        return response.data.data as TiendaEstado;
    }
};
