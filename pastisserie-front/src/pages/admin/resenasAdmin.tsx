import { useState, useEffect } from 'react';
import {
    MessageSquare, Star, Package,
    Trash2, Clock, Check
} from 'lucide-react';
import { reviewService } from '../../services/reviewService';
import type { Review } from '../../services/reviewService';
import toast from 'react-hot-toast';

const ResenasAdmin = () => {
    const [reviews, setReviews] = useState<Review[]>([]);
    const [loading, setLoading] = useState(true);
    const [filter, setFilter] = useState<'pending' | 'approved' | 'all'>('pending');

    useEffect(() => {
        loadReviews();
    }, [filter]);

    const loadReviews = async () => {
        try {
            setLoading(true);
            const dataRaw = (filter === 'pending')
                ? await reviewService.getPending()
                : (filter === 'approved')
                    ? (await reviewService.getFeatured()).filter((r: Review) => r.aprobada)
                    : await reviewService.getFeatured();

            // Filtrar datos de prueba
            const filteredData = (Array.isArray(dataRaw) ? dataRaw : []).filter(r =>
                r.comentario &&
                !r.comentario.toLowerCase().includes('hgfh') &&
                !r.comentario.toLowerCase().includes('sadda') &&
                !r.comentario.toLowerCase().includes('deliciosoo')
            );

            setReviews(filteredData);
        } catch (error) {
            console.error("Error cargando reseñas:", error);
            toast.error('Error al sincronizar el registro de reseñas');
        } finally {
            setLoading(false);
        }
    };

    const handleApprove = async (id: number) => {
        try {
            await reviewService.approve(id);
            toast.success('Reseña aprobada exitosamente');
            loadReviews();
        } catch (error) {
            toast.error('Error en el proceso de aprobación');
        }
    };

    const handleDelete = async (id: number) => {
        if (window.confirm('¿Confirmar la eliminación definitiva de este registro?')) {
            try {
                await reviewService.delete(id);
                toast.success('Registro eliminado del sistema');
                loadReviews();
            } catch (error) {
                toast.error('Error al procesar la solicitud de eliminación');
            }
        }
    };

    return (
        <div className="animate-fade-in max-w-7xl mx-auto">
            {/* HEADER PROFESIONAL */}
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-6 mb-12">
                <div>
                    <h1 className="text-4xl font-serif font-black text-gray-900 tracking-tighter flex items-center gap-4">
                        <div className="p-3 bg-blue-600/10 text-blue-700 rounded-3xl">
                            <MessageSquare size={32} strokeWidth={2.5} />
                        </div>
                        Moderación de Reseñas
                    </h1>
                    <p className="text-gray-400 font-bold uppercase tracking-[0.2em] text-[10px] mt-2 ml-16">
                        Auditoría y control de calidad de retroalimentación pública
                    </p>
                </div>

                <div className="flex items-center gap-2 bg-white p-1.5 rounded-[2rem] shadow-sm border border-gray-100">
                    <button
                        onClick={() => setFilter('pending')}
                        className={`px-6 py-2.5 rounded-[1.5rem] text-[10px] font-black uppercase tracking-widest transition-all ${filter === 'pending' ? 'bg-amber-600 text-white shadow-lg' : 'text-gray-400 hover:text-gray-600'}`}
                    >
                        Pendientes
                    </button>
                    <button
                        onClick={() => setFilter('approved')}
                        className={`px-6 py-2.5 rounded-[1.5rem] text-[10px] font-black uppercase tracking-widest transition-all ${filter === 'approved' ? 'bg-emerald-600 text-white shadow-lg' : 'text-gray-400 hover:text-gray-600'}`}
                    >
                        Aprobadas
                    </button>
                    <button
                        onClick={() => setFilter('all')}
                        className={`px-6 py-2.5 rounded-[1.5rem] text-[10px] font-black uppercase tracking-widest transition-all ${filter === 'all' ? 'bg-gray-900 text-white shadow-lg' : 'text-gray-400 hover:text-gray-600'}`}
                    >
                        Todas
                    </button>
                </div>
            </div>

            {loading ? (
                <div className="flex flex-col items-center justify-center h-[50vh] gap-4">
                    <div className="w-12 h-12 border-4 border-blue-600/20 border-t-blue-600 rounded-full animate-spin"></div>
                    <p className="text-[10px] font-black uppercase tracking-[0.3em] text-gray-400">Sincronizando Archivos...</p>
                </div>
            ) : reviews.length === 0 ? (
                <div className="bg-white rounded-[3.5rem] border border-dashed border-gray-200 p-24 text-center">
                    <div className="w-20 h-20 bg-gray-50 rounded-full flex items-center justify-center mx-auto mb-6">
                        <Clock size={32} className="text-gray-200" />
                    </div>
                    <h3 className="text-xl font-serif font-black text-gray-600 mb-2">No se encuentran registros</h3>
                    <p className="text-gray-400 text-sm max-w-xs mx-auto">No hay reseñas que coincidan con el filtro de moderación seleccionado.</p>
                </div>
            ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-8">
                    {reviews.map((rev) => (
                        <div key={rev.id} className="bg-white rounded-[3rem] shadow-sm border border-gray-100 overflow-hidden flex flex-col group hover:shadow-2xl hover:shadow-gray-200/50 transition-all duration-500 hover:-translate-y-2">
                            <div className="p-10 flex-1">
                                <div className="flex justify-between items-start mb-8">
                                    <div className="flex items-center gap-4">
                                        <div className="w-12 h-12 bg-gray-900 text-white rounded-2xl flex items-center justify-center font-black text-xs shadow-lg group-hover:bg-blue-600 transition-colors">
                                            {rev.usuarioNombre?.substring(0, 1).toUpperCase() || 'U'}
                                        </div>
                                        <div>
                                            <p className="font-black text-gray-900 text-xs uppercase tracking-tight">{rev.usuarioNombre}</p>
                                            <div className="flex items-center gap-2 mt-1">
                                                <div className="w-1.5 h-1.5 rounded-full bg-emerald-500"></div>
                                                <p className="text-[9px] text-gray-400 font-bold uppercase tracking-widest">Verificado</p>
                                            </div>
                                        </div>
                                    </div>
                                    <div className="flex gap-0.5 text-amber-500">
                                        {Array.from({ length: 5 }).map((_, i) => (
                                            <Star key={i} size={14} className={i < rev.calificacion ? "fill-current" : "text-gray-100"} />
                                        ))}
                                    </div>
                                </div>

                                <div className="mb-6 flex items-center gap-3 bg-gray-50/80 p-3 rounded-2xl border border-gray-100">
                                    <Package size={14} className="text-blue-600" />
                                    <p className="text-[10px] font-black uppercase text-gray-600 truncate tracking-tight">{rev.productoNombre || 'Producto General'}</p>
                                </div>

                                <p className="text-[13px] text-gray-600 leading-relaxed italic border-l-4 border-blue-50 pl-5 mb-4 group-hover:border-blue-200 transition-colors">
                                    "{rev.comentario || 'Evaluación técnica sin comentarios adicionales.'}"
                                </p>

                                <p className="text-[9px] text-gray-300 font-bold uppercase tracking-widest mt-8">
                                    Registrado: {new Date(rev.fechaCreacion || '').toLocaleDateString('es-CO', { day: '2-digit', month: 'long', year: 'numeric' })}
                                </p>
                            </div>

                            <div className="p-2 flex gap-2">
                                {!rev.aprobada && (
                                    <button
                                        onClick={() => handleApprove(rev.id)}
                                        className="flex-1 bg-gray-900 text-white font-black py-4 px-6 rounded-[2rem] hover:bg-emerald-600 hover:scale-[1.02] active:scale-95 transition-all flex items-center justify-center gap-3 text-[10px] uppercase tracking-widest shadow-xl shadow-gray-900/10"
                                    >
                                        <Check size={16} strokeWidth={3} /> Validar Reseña
                                    </button>
                                )}
                                <button
                                    onClick={() => handleDelete(rev.id)}
                                    className={`${rev.aprobada ? 'flex-1' : 'w-20'} bg-red-50 text-red-600 font-black py-4 rounded-[2rem] hover:bg-red-600 hover:text-white transition-all flex items-center justify-center gap-3 text-[10px] uppercase tracking-widest group/del active:scale-90`}
                                    title="Descartar permanentemente"
                                >
                                    <Trash2 size={16} strokeWidth={2.5} />
                                    {rev.aprobada && "Eliminar del Sistema"}
                                </button>
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};

export default ResenasAdmin;
