import { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import {
    FiAlertCircle, FiCheckCircle, FiClock, FiMessageSquare, FiArrowLeft, FiSend, FiPackage
} from 'react-icons/fi';
import toast from 'react-hot-toast';

// Servicios
import { reclamacionesService, type Reclamacion } from '../services/reclamacionesService';

const Reclamaciones = () => {
    const location = useLocation();
    const navigate = useNavigate();
    const queryParams = new URLSearchParams(location.search);
    const pedidoIdFromUrl = queryParams.get('pedidoId');

    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [myClaims, setMyClaims] = useState<Reclamacion[]>([]);

    // Form state
    const [pedidoId, setPedidoId] = useState(pedidoIdFromUrl || '');
    const [motivo, setMotivo] = useState('');
    const [formErrors, setFormErrors] = useState<{ pedidoId?: string; motivo?: string }>({});

    const fetchMyClaims = async () => {
        try {
            const response = await reclamacionesService.getMisReclamaciones();
            setMyClaims(response.data || []);
            setLoading(false);
        } catch (error) {
            console.error(error);
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchMyClaims();
    }, []);

    const validateForm = () => {
        const errors: { pedidoId?: string; motivo?: string } = {};

        if (!pedidoId || pedidoId.toString().trim() === '') {
            errors.pedidoId = 'El número de pedido es obligatorio';
        } else if (isNaN(Number(pedidoId)) || Number(pedidoId) <= 0) {
            errors.pedidoId = 'Ingresa un número de pedido válido (ej: 1045)';
        }

        if (!motivo.trim()) {
            errors.motivo = 'Describe el motivo de tu reclamación';
        } else if (motivo.trim().length < 10) {
            errors.motivo = 'El motivo debe tener al menos 10 caracteres';
        }

        setFormErrors(errors);
        return Object.keys(errors).length === 0;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!validateForm()) {
            return;
        }

        try {
            setSubmitting(true);
            await reclamacionesService.createReclamacion({
                pedidoId: parseInt(pedidoId),
                motivo: motivo
            });

            toast.success('Reclamación enviada con éxito');
            setMotivo('');
            setPedidoId('');
            setFormErrors({});
            fetchMyClaims();
        } catch (error: any) {
            const errorMsg = error.response?.data?.message || 'Error al enviar reclamación. Recuerda que solo tienes 3 días después de la entrega.';
            toast.error(errorMsg);
        } finally {
            setSubmitting(false);
        }
    };

    const getStatusStyle = (estado: string) => {
        switch (estado) {
            case 'Pendiente': return 'bg-yellow-100 text-yellow-700 border-yellow-200';
            case 'EnRevision': return 'bg-blue-100 text-blue-700 border-blue-200';
            case 'Resuelta': return 'bg-green-100 text-green-700 border-green-200';
            case 'Rechazada': return 'bg-red-100 text-red-700 border-red-200';
            default: return 'bg-gray-100 text-gray-700 border-gray-200';
        }
    };

    return (
        <div className="animate-fade-in max-w-6xl mx-auto px-4 py-12">
            <button
                onClick={() => navigate('/perfil')}
                className="flex items-center gap-2 text-gray-500 hover:text-patisserie-red font-bold mb-8 transition-colors group"
            >
                <FiArrowLeft className="group-hover:-translate-x-1 transition-transform" /> Volver a mi perfil
            </button>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-12">

                {/* Lado Izquierdo: Formulario */}
                <div className="lg:col-span-1 border border-gray-100 rounded-[40px] p-8 bg-white shadow-xl shadow-gray-100/50">
                    <div className="mb-8">
                        <div className="w-14 h-14 bg-patisserie-red/10 rounded-2xl flex items-center justify-center text-patisserie-red mb-4">
                            <FiAlertCircle size={28} />
                        </div>
                        <h1 className="text-2xl font-black text-gray-900 tracking-tight">Nueva Reclamación</h1>
                        <p className="text-gray-400 text-sm font-medium mt-1">¿Algo no salió como esperabas? Cuéntanos para ayudarte.</p>
                    </div>

                    <form onSubmit={handleSubmit} className="space-y-6">
                        <div>
                            <label className="block text-[10px] font-black uppercase tracking-widest text-gray-400 mb-2 px-1">Número de Pedido</label>
                            <div className="relative">
                                <FiPackage className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-300" />
                                <input
                                    type="number"
                                    value={pedidoId}
                                    onChange={(e) => { setPedidoId(e.target.value); if (formErrors.pedidoId) setFormErrors(prev => ({ ...prev, pedidoId: undefined })); }}
                                    placeholder="Ej: 1045"
                                    className={`w-full bg-gray-50 rounded-2xl py-4 pl-12 pr-4 font-bold text-gray-700 focus:ring-2 transition-all border-2 ${formErrors.pedidoId ? 'border-red-300 focus:ring-red-200 bg-red-50' : 'border-transparent focus:ring-patisserie-red/20'}`}
                                />
                            </div>
                            {formErrors.pedidoId && (
                                <p className="flex items-center gap-1 text-red-500 text-xs font-bold mt-1.5 px-1">
                                    <FiAlertCircle size={12} /> {formErrors.pedidoId}
                                </p>
                            )}
                        </div>

                        <div>
                            <label className="block text-[10px] font-black uppercase tracking-widest text-gray-400 mb-2 px-1">Motivo de la reclamación</label>
                            <div className="relative">
                                <FiMessageSquare className="absolute left-4 top-6 text-gray-300" />
                                <textarea
                                    value={motivo}
                                    onChange={(e) => { setMotivo(e.target.value); if (formErrors.motivo) setFormErrors(prev => ({ ...prev, motivo: undefined })); }}
                                    placeholder="Describe detalladamente el problema..."
                                    rows={5}
                                    className={`w-full bg-gray-50 rounded-2xl py-4 pl-12 pr-4 font-bold text-gray-700 focus:ring-2 transition-all resize-none border-2 ${formErrors.motivo ? 'border-red-300 focus:ring-red-200 bg-red-50' : 'border-transparent focus:ring-patisserie-red/20'}`}
                                />
                            </div>
                            {formErrors.motivo && (
                                <p className="flex items-center gap-1 text-red-500 text-xs font-bold mt-1.5 px-1">
                                    <FiAlertCircle size={12} /> {formErrors.motivo}
                                </p>
                            )}
                        </div>

                        <div className="bg-patisserie-red/5 p-4 rounded-2xl border border-patisserie-red/10 flex gap-3">
                            <FiClock className="text-patisserie-red shrink-0 mt-0.5" />
                            <p className="text-[10px] font-bold text-patisserie-red leading-relaxed">
                                <span className="uppercase font-black block mb-1">Nota Importante:</span>
                                Tienes un plazo máximo de <span className="underline italic">3 días hábiles</span> después de la entrega para generar un reclamo.
                            </p>
                        </div>

                        <button
                            type="submit"
                            disabled={submitting}
                            className="w-full bg-patisserie-red text-white font-black py-4 rounded-2xl shadow-lg shadow-red-900/20 hover:bg-red-900 transition-all flex items-center justify-center gap-2 active:scale-[0.98] disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            {submitting ? 'Enviando...' : <><FiSend /> ENVIAR RECLAMACIÓN</>}
                        </button>
                    </form>
                </div>

                {/* Lado Derecho: Lista de Reclamaciones */}
                <div className="lg:col-span-2 space-y-6">
                    <div className="flex items-center justify-between mb-2">
                        <h2 className="text-xl font-black text-gray-900 uppercase tracking-tighter italic">Mis Reclamaciones Previas</h2>
                        <span className="text-[10px] font-black text-gray-400 bg-gray-50 px-3 py-1 rounded-full">{myClaims.length} registros</span>
                    </div>

                    {loading ? (
                        <div className="flex justify-center items-center py-20 bg-white rounded-[40px] border border-gray-50 shadow-sm">
                            <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-patisserie-red"></div>
                        </div>
                    ) : myClaims.length === 0 ? (
                        <div className="bg-gray-50 rounded-[40px] p-20 text-center border-2 border-dashed border-gray-100 opacity-60">
                            <div className="w-20 h-20 bg-white rounded-3xl flex items-center justify-center text-gray-200 mx-auto mb-6 shadow-sm">
                                <FiCheckCircle size={40} />
                            </div>
                            <h3 className="text-gray-400 font-black uppercase tracking-widest text-sm">No tienes reclamaciones activas</h3>
                            <p className="text-gray-300 text-xs font-bold mt-2 italic">¡Eso significa que todo va genial!</p>
                        </div>
                    ) : (
                        <div className="grid gap-4">
                            {myClaims.sort((a, b) => b.id - a.id).map((claim) => (
                                <div key={claim.id} className="bg-white rounded-[32px] p-6 border border-gray-100 shadow-sm hover:shadow-xl hover:border-patisserie-red/10 transition-all group">
                                    <div className="flex flex-col sm:flex-row justify-between items-start gap-4">
                                        <div className="flex items-center gap-4">
                                            <div className="w-12 h-12 bg-gray-50 rounded-2xl flex items-center justify-center text-patisserie-red font-black group-hover:bg-patisserie-red group-hover:text-white transition-all shadow-inner">
                                                #{claim.id}
                                            </div>
                                            <div>
                                                <div className="flex items-center gap-2 mb-1">
                                                    <span className={`text-[9px] font-black px-2 py-0.5 rounded-lg border uppercase tracking-widest ${getStatusStyle(claim.estado)}`}>
                                                        {claim.estado}
                                                    </span>
                                                    <span className="text-[10px] text-gray-400 font-bold italic">
                                                        Pedido #{claim.pedidoId}
                                                    </span>
                                                </div>
                                                <p className="text-sm font-bold text-gray-700 leading-tight pr-4">
                                                    {claim.motivo}
                                                </p>
                                            </div>
                                        </div>
                                        <div className="shrink-0 text-right mt-2 sm:mt-0">
                                            <p className="text-[10px] font-black text-gray-300 uppercase tracking-widest leading-none">Fecha de reporte</p>
                                            <p className="text-xs font-black text-gray-500 mt-1 italic">
                                                {new Date(claim.fecha).toLocaleDateString()}
                                            </p>
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default Reclamaciones;
