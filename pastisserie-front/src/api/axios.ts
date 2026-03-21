import axios from 'axios';

const api = axios.create({
    baseURL: '/api', // Usar el proxy de Vite para evitar problemas de SSL local
    headers: {
        'Content-Type': 'application/json',
    },
});

// Esto servirá para que, cuando el usuario se loguee, 
// todas las peticiones lleven el token por defecto.
api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

// Interceptor de respuesta para manejar errores globales
api.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response && error.response.status === 401) {
            // Token expirado o inválido
            console.warn('Sesión expirada o no autorizada. Redirigiendo al login...');

            const hadToken = !!localStorage.getItem('token');

            localStorage.removeItem('token');
            localStorage.removeItem('user');

            // Solo redirigir al login si estábamos logueados (teníamos token)
            // de lo contrario, una petición pública con error 401 nos expulsaría sin sentido
            if (hadToken && !window.location.pathname.includes('/login')) {
                window.location.href = '/login';
            }
        }
        return Promise.reject(error);
    }
);

export default api;