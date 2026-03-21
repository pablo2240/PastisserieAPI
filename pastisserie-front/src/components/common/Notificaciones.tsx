import React, { useState, useEffect, useRef } from 'react';
import { Bell, Check, Info, AlertTriangle, CheckCircle, XCircle } from 'lucide-react';
import { notificacionService } from '../../api/notificacionService';
import type { Notificacion } from '../../api/notificacionService';
import { useAuth } from '../../context/AuthContext';
import { useNavigate } from 'react-router-dom';

interface NotificacionesProps {
    className?: string;
}

const Notificaciones: React.FC<NotificacionesProps> = ({ className }) => {
    const [notificaciones, setNotificaciones] = useState<Notificacion[]>([]);
    const [isOpen, setIsOpen] = useState(false);
    const [loading, setLoading] = useState(false);
    const { user } = useAuth();
    const dropdownRef = useRef<HTMLDivElement>(null);
    const navigate = useNavigate();

    const cargarNotificaciones = async () => {
        // ... (omitting intermediate logic if possible, but replace_file_content needs contiguous block)
        // I will just replace the whole component definition part including the render start.
        try {
            const response = await notificacionService.getMisNotificaciones();
            // Resiliencia ante formatos: { success, data } o array directo
            let rawData = [];
            if (response && response.success && Array.isArray(response.data)) {
                rawData = response.data;
            } else if (Array.isArray(response)) {
                rawData = response;
            } else if (response && Array.isArray(response.data)) {
                rawData = response.data;
            }

            // Filtrado por Rol para Relevancia Máxima
            let processedData = [...rawData];
            if (user?.rol) {
                const roles = Array.isArray(user.rol) ? user.rol : [user.rol];
                if (roles.includes('Repartidor')) {
                    processedData = processedData.sort((a: Notificacion, b: Notificacion) => {
                        const aPrio = a.titulo.toLowerCase().includes('asignad') ? 1 : 0;
                        const bPrio = b.titulo.toLowerCase().includes('asignad') ? 1 : 0;
                        return bPrio - aPrio;
                    });
                }
            }

            setNotificaciones(processedData);
        } catch (error) {
            console.error("Error al cargar notificaciones:", error);
        }
    };

    useEffect(() => {
        // Cargar al inicio y cada 12 segundos (Alta Frecuencia para flujo profesional)
        cargarNotificaciones();
        const interval = setInterval(cargarNotificaciones, 12000);
        return () => clearInterval(interval);
    }, []);

    // Cerrar dropdown al hacer click fuera
    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
                setIsOpen(false);
            }
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    const handleMarcarLeida = async (id: number) => {
        try {
            await notificacionService.marcarLeida(id);
            setNotificaciones(prev => prev.map(n => n.id === id ? { ...n, leida: true } : n));
        } catch (error) {
            console.error("Error al marcar como leída", error);
        }
    };

    const handleMarcarTodas = async () => {
        try {
            setLoading(true);
            await notificacionService.marcarTodasLeidas();
            setNotificaciones(prev => prev.map(n => ({ ...n, leida: true })));
            setLoading(false);
        } catch (error) {
            console.error("Error al marcar todas", error);
            setLoading(false);
        }
    };

    const handleNotificacionClick = (notificacion: Notificacion) => {
        if (!notificacion.leida) {
            handleMarcarLeida(notificacion.id);
        }

        if (notificacion.enlace) {
            navigate(notificacion.enlace);
            setIsOpen(false);
        }
    };

    const getIcon = (tipo: string) => {
        switch (tipo) {
            case 'Success': return <CheckCircle size={16} className="text-green-500" />;
            case 'Warning': return <AlertTriangle size={16} className="text-orange-500" />;
            case 'Error': return <XCircle size={16} className="text-red-500" />;
            default: return <Info size={16} className="text-blue-500" />;
        }
    };

    const noLeidas = notificaciones.filter(n => !n.leida).length;

    return (
        <div className="relative" ref={dropdownRef}>
            <button
                onClick={() => setIsOpen(!isOpen)}
                className={`relative p-2 transition-all transform active:scale-95 focus:outline-none ${className || 'text-gray-600 hover:text-patisserie-red'}`}
            >
                <Bell size={24} />
                {noLeidas > 0 && (
                    <span className="absolute top-1 right-1 inline-flex items-center justify-center w-5 h-5 text-[10px] font-black leading-none text-white transform translate-x-1/4 -translate-y-1/4 bg-patisserie-red rounded-full shadow-lg shadow-red-900/20 border-2 border-white animate-pulse">
                        {noLeidas > 9 ? '9+' : noLeidas}
                    </span>
                )}
            </button>

            {isOpen && (
                <div className="absolute right-0 mt-3 w-80 bg-white rounded-3xl shadow-2xl overflow-hidden z-[110] border border-gray-100 animate-scale-in origin-top-right">
                    <div className="p-5 bg-gray-50/50 border-b border-gray-100 flex justify-between items-center">
                        <div>
                            <h3 className="text-sm font-black text-gray-900 tracking-tight">Notificaciones</h3>
                            <p className="text-[10px] text-gray-400 font-bold uppercase tracking-widest">{noLeidas} nuevas hoy</p>
                        </div>
                        {noLeidas > 0 && (
                            <button
                                onClick={handleMarcarTodas}
                                disabled={loading}
                                className="text-xs text-patisserie-red hover:text-red-700 font-black flex items-center gap-1 transition-colors"
                            >
                                <Check size={14} />
                                Leer todas
                            </button>
                        )}
                    </div>

                    <div className="max-h-[400px] overflow-y-auto custom-scrollbar">
                        {notificaciones.length === 0 ? (
                            <div className="p-10 text-center space-y-3">
                                <div className="w-12 h-12 bg-gray-50 rounded-2xl flex items-center justify-center mx-auto text-gray-300">
                                    <Bell size={24} />
                                </div>
                                <p className="text-xs text-gray-400 font-bold uppercase tracking-widest">Sin notificaciones</p>
                                <p className="text-[10px] text-gray-300">Te avisaremos cuando pase algo importante.</p>
                            </div>
                        ) : (
                            <ul className="divide-y divide-gray-50">
                                {notificaciones.map(notificacion => (
                                    <li
                                        key={notificacion.id}
                                        onClick={() => handleNotificacionClick(notificacion)}
                                        className={`p-4 hover:bg-gray-50 transition-all ${notificacion.enlace ? 'cursor-pointer hover:bg-gray-100' : 'cursor-default'} ${!notificacion.leida ? 'bg-blue-50/20' : ''}`}
                                    >
                                        <div className="flex gap-3">
                                            <div className="mt-1 flex-shrink-0">
                                                {getIcon(notificacion.tipo)}
                                            </div>
                                            <div className="flex-1 min-w-0">
                                                <div className="flex justify-between items-start gap-2">
                                                    <p className={`text-xs font-black leading-tight ${!notificacion.leida ? 'text-gray-900' : 'text-gray-500'}`}>
                                                        {notificacion.titulo}
                                                    </p>
                                                    {!notificacion.leida && (
                                                        <button
                                                            onClick={(e) => {
                                                                e.stopPropagation();
                                                                handleMarcarLeida(notificacion.id);
                                                            }}
                                                            className="w-2 h-2 rounded-full bg-patisserie-red flex-shrink-0 mt-1"
                                                            title="Marcar leída"
                                                        />
                                                    )}
                                                </div>
                                                <p className="text-[11px] text-gray-500 mt-1 leading-relaxed line-clamp-2">
                                                    {notificacion.mensaje}
                                                </p>
                                                <p className="text-[9px] text-gray-400 font-bold uppercase mt-2">
                                                    Hace {Math.floor((new Date().getTime() - new Date(notificacion.fechaCreacion).getTime()) / 60000)} min
                                                </p>
                                            </div>
                                        </div>
                                    </li>
                                ))}
                            </ul>
                        )}
                    </div>
                </div>
            )}
        </div>
    );
};

export default Notificaciones;
