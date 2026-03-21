import { useState, useEffect } from 'react';
import { createPortal } from 'react-dom';
import { FiClock, FiChevronDown, FiActivity } from 'react-icons/fi';
import { tiendaService, type TiendaEstado } from '../../services/tiendaService';

const ShopStatusWidget = () => {
    const [estado, setEstado] = useState<TiendaEstado | null>(null);
    const [loading, setLoading] = useState(true);
    const [showFullSchedule, setShowFullSchedule] = useState(false);

    const fetchEstado = async () => {
        try {
            const data = await tiendaService.getEstado();
            setEstado(data);
        } catch (error) {
            console.error("Error fetching shop status:", error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchEstado();
        const interval = setInterval(fetchEstado, 60000);
        return () => clearInterval(interval);
    }, []);

    const getDiaNombre = (dia: number) => {
        const dias = ['Dom', 'Lun', 'Mar', 'Mie', 'Jue', 'Vie', 'Sab'];
        return dias[dia];
    };

    if (loading) return null;

    const medellinTime = new Intl.DateTimeFormat('es-CO', {
        timeZone: 'America/Bogota',
        hour: '2-digit',
        minute: '2-digit',
        hour12: false
    }).format(new Date());

    return (
        <>
            <div className="relative">
                <div className="flex items-center gap-4 bg-white/80 backdrop-blur-md px-5 py-2.5 rounded-2xl border border-gray-100 shadow-sm transition-all hover:shadow-md">
                    <div className="flex items-center gap-2.5 pr-4 border-r border-gray-100">
                        <div className={`w-2.5 h-2.5 rounded-full ${estado?.estaAbierto ? 'bg-emerald-500 shadow-[0_0_8px_rgba(16,185,129,0.4)]' : 'bg-rose-500 animate-pulse shadow-[0_0_8px_rgba(244,63,94,0.4)]'}`}></div>
                        <span className={`text-[10px] font-black uppercase tracking-widest ${estado?.estaAbierto ? 'text-emerald-600' : 'text-rose-600'}`}>
                            {estado?.estaAbierto ? 'Tienda Abierta' : 'Tienda Cerrada'}
                        </span>
                    </div>

                    <div className="flex items-center gap-4">
                        <div className="flex flex-col">
                            <span className="text-[9px] font-black text-gray-400 uppercase tracking-tighter leading-none mb-1">Medellín (CO)</span>
                            <div className="flex items-center gap-1.5">
                                <span className="text-xs font-black text-gray-800 tabular-nums">{medellinTime}</span>
                            </div>
                        </div>

                        <button
                            onClick={() => {
                                setShowFullSchedule(true);
                                document.body.classList.add('overflow-hidden');
                            }}
                            className={`p-2 transition-all rounded-xl ${showFullSchedule ? 'bg-[#7D2121] text-white shadow-lg' : 'bg-gray-50 text-gray-400 hover:text-gray-900 hover:bg-gray-100'}`}
                            title="Ver Horarios 24h"
                        >
                            <FiClock size={16} />
                        </button>
                    </div>
                </div>
            </div>

            {showFullSchedule && createPortal(
                <div className="fixed inset-0 bg-[#5D1919]/20 flex items-center justify-center z-[2000] p-4 backdrop-blur-md animate-fade-in" role="dialog">
                    <div className="bg-white rounded-[2.5rem] shadow-2xl border border-gray-100 p-8 w-full max-w-md animate-scale-in">
                        <div className="flex justify-between items-center mb-8">
                            <div>
                                <h4 className="text-[10px] font-black text-gray-400 uppercase tracking-widest">Horarios de Operación</h4>
                                <p className="text-[9px] font-bold text-gray-300 uppercase mt-1">Formato 24 Horas</p>
                            </div>
                            <button
                                onClick={() => {
                                    setShowFullSchedule(false);
                                    document.body.classList.remove('overflow-hidden');
                                }}
                                className="w-8 h-8 flex items-center justify-center bg-gray-50 text-gray-400 hover:text-gray-900 rounded-full transition-all"
                            >
                                <FiChevronDown className="rotate-180" />
                            </button>
                        </div>

                        <div className="space-y-2 max-h-[60vh] overflow-y-auto pr-2 custom-scrollbar">
                            {estado?.horariosPorDia?.map((h) => (
                                <div key={h.diaSemana} className={`flex justify-between items-center p-4 rounded-2xl transition-all ${h.diaSemana === new Date().getDay() ? 'bg-[#7D2121] text-white shadow-xl shadow-[#7D2121]/10' : 'bg-gray-50/50 border border-transparent hover:border-gray-100'}`}>
                                    <div className="flex items-center gap-3">
                                        <span className="text-[10px] font-black uppercase">{getDiaNombre(h.diaSemana)}</span>
                                        {h.diaSemana === new Date().getDay() && (
                                            <div className="px-2 py-0.5 bg-white/20 rounded-full text-[8px] font-black uppercase tracking-tighter">Hoy</div>
                                        )}
                                    </div>
                                    <span className={`text-[11px] font-black tabular-nums ${h.diaSemana === new Date().getDay() ? 'text-white' : (h.abierto ? 'text-gray-800' : 'text-rose-400')}`}>
                                        {h.abierto ? `${h.horaApertura} a ${h.horaCierre}` : 'No Laborable'}
                                    </span>
                                </div>
                            ))}
                        </div>

                        <div className="mt-8 pt-6 border-t border-gray-50 flex items-center justify-between">
                            <div className="flex items-center gap-3">
                                <div className={`w-2 h-2 rounded-full ${estado?.usarControlHorario ? 'bg-blue-400' : 'bg-amber-400'}`}></div>
                                <p className="text-[9px] font-black text-gray-400 uppercase tracking-widest">
                                    Control: {estado?.usarControlHorario ? 'Automático' : 'Manual / Siempre Abierto'}
                                </p>
                            </div>
                            <FiActivity className="text-gray-200" size={14} />
                        </div>

                        <button
                            onClick={() => {
                                setShowFullSchedule(false);
                                document.body.classList.remove('overflow-hidden');
                            }}
                            className="w-full mt-6 py-4 bg-gray-900 text-white font-black uppercase tracking-[0.2em] text-[10px] rounded-2xl hover:bg-[#7D2121] transition-all"
                        >
                            Cerrar Vista
                        </button>
                    </div>
                </div>,
                document.body
            )}
        </>
    );
};

export default ShopStatusWidget;
