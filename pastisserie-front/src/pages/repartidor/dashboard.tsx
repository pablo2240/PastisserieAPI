import { useEffect, useState } from 'react';
import {
    FiPackage, FiMapPin, FiCheckCircle, FiClock, FiPhone, FiXCircle, FiTrendingUp, FiActivity, FiSearch, FiArrowRight, FiUser, FiX
} from 'react-icons/fi';
import toast from 'react-hot-toast';

// Servicios
import { dashboardService } from '../../services/dashboardService';
import api from '../../api/axios';

const RepartidorDashboard = () => {
    const [data, setData] = useState<any>(null);
    const [loading, setLoading] = useState(true);
    const [activeTab, setActiveTab] = useState<'pendientes' | 'entregados' | 'noEntregados' | 'historial'>('pendientes');
    const [selectedPedido, setSelectedPedido] = useState<any>(null);


    const fetchStats = async () => {
        try {
            const stats = await dashboardService.getRepartidorStats();
            setData(stats);
            setLoading(false);
        } catch (error) {
            console.error(error);
            toast.error('Error al cargar datos del repartidor');
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchStats();
        const interval = setInterval(fetchStats, 60000); // Actualizar cada minuto
        return () => clearInterval(interval);
    }, []);

    const [failModalOpen, setFailModalOpen] = useState(false);
    const [pedidoToFail, setPedidoToFail] = useState<number | null>(null);
    const [motivoFail, setMotivoFail] = useState("");
    const [otroMotivo, setOtroMotivo] = useState("");

    const updateEstado = async (id: number, nuevoEstado: 'Entregado' | 'NoEntregado', motivoProvisto?: string) => {
        if (nuevoEstado === 'NoEntregado' && !motivoProvisto) {
            setPedidoToFail(id);
            setFailModalOpen(true);
            return;
        }

        try {
            await api.put(`/pedidos/${id}/estado`, {
                estado: nuevoEstado,
                motivoNoEntrega: motivoProvisto || ""
            });
            toast.success(`Pedido #${id} marcado como ${nuevoEstado === 'Entregado' ? 'Entregado' : 'No Entregado'}`);
            fetchStats();
            setFailModalOpen(false);
            setPedidoToFail(null);
            setMotivoFail("");
            setOtroMotivo("");
        } catch (error: any) {
            const msg = error.response?.data?.message || 'Error al actualizar estado';
            toast.error(msg);
        }
    };

    if (loading || !data) {
        return (
            <div className="flex justify-center items-center h-[70vh]">
                <div className="animate-spin rounded-full h-16 w-16 border-b-4 border-patisserie-red"></div>
            </div>
        );
    }

    const { resumen, enCamino, entregados, noEntregados, historial } = data;

    // Determinar qué lista mostrar según el tab activo
    let displayList = enCamino;
    if (activeTab === 'entregados') displayList = entregados;
    if (activeTab === 'noEntregados') displayList = noEntregados;
    if (activeTab === 'historial') displayList = historial;

    return (
        <div className="animate-fade-in space-y-8 max-w-5xl mx-auto p-4 md:p-6 pb-20">
            {/* Header y Saludo */}
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
                <div>
                    <h1 className="text-3xl font-black text-gray-900 tracking-tight">Panel de Reparto</h1>
                    <p className="text-gray-500 font-medium">Gestiona tus entregas y revisa tus ganancias</p>
                </div>
                <div className="flex items-center gap-3 bg-patisserie-red/5 px-4 py-2 rounded-2xl border border-patisserie-red/10">
                    <FiClock className="text-patisserie-red" />
                    <span className="text-sm font-black text-patisserie-red">
                        {new Date().toLocaleDateString('es-ES', { weekday: 'long', day: 'numeric', month: 'long' })}
                    </span>
                </div>
            </div>

            {/* Stats Cards */}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-3 md:gap-4">
                <div
                    onClick={() => setActiveTab('pendientes')}
                    className="bg-white p-6 rounded-[32px] border border-gray-100 shadow-sm flex items-center gap-4 group hover:shadow-xl hover:-translate-y-1 transition-all duration-300 cursor-pointer"
                >
                    <div className="p-4 bg-patisserie-red/10 text-patisserie-red rounded-2xl group-hover:bg-patisserie-red group-hover:text-white transition-colors">
                        <FiPackage size={24} />
                    </div>
                    <div>
                        <p className="text-[10px] uppercase font-black text-gray-400 tracking-widest">En Camino</p>
                        <h4 className="text-2xl font-black text-gray-900">{resumen.totalEnCamino}</h4>
                    </div>
                </div>

                <div
                    onClick={() => setActiveTab('entregados')}
                    className="bg-white p-6 rounded-[32px] border border-gray-100 shadow-sm flex items-center gap-4 group hover:shadow-xl hover:-translate-y-1 transition-all duration-300 cursor-pointer"
                >
                    <div className="p-4 bg-green-50 text-green-600 rounded-2xl group-hover:bg-green-600 group-hover:text-white transition-colors">
                        <FiCheckCircle size={24} />
                    </div>
                    <div>
                        <p className="text-[10px] uppercase font-black text-gray-400 tracking-widest">Entregados</p>
                        <h4 className="text-2xl font-black text-gray-900">{resumen.totalEntregados}</h4>
                    </div>
                </div>

                <div
                    onClick={() => setActiveTab('noEntregados')}
                    className="bg-white p-6 rounded-[32px] border border-gray-100 shadow-sm flex items-center gap-4 group hover:shadow-xl hover:-translate-y-1 transition-all duration-300 cursor-pointer"
                >
                    <div className="p-4 bg-orange-50 text-orange-600 rounded-2xl group-hover:bg-orange-600 group-hover:text-white transition-colors">
                        <FiXCircle size={24} />
                    </div>
                    <div>
                        <p className="text-[10px] uppercase font-black text-gray-400 tracking-widest">Fallidos</p>
                        <h4 className="text-2xl font-black text-gray-900">{resumen.totalNoEntregados}</h4>
                    </div>
                </div>

                <div
                    onClick={() => setActiveTab('historial')}
                    className="bg-gray-900 p-6 rounded-[32px] shadow-lg shadow-gray-900/20 flex items-center gap-4 hover:-translate-y-1 transition-all duration-300 cursor-pointer"
                >
                    <div className="p-4 bg-white/10 text-white rounded-2xl">
                        <FiTrendingUp size={24} />
                    </div>
                    <div>
                        <p className="text-[10px] uppercase font-black text-white/40 tracking-widest">Ganancias por Domicilios</p>
                        <h4 className="text-2xl font-black text-white">${resumen.gananciasTotales.toLocaleString()}</h4>
                    </div>
                </div>
            </div>

            {/* Tabs System */}
            <div className="bg-white rounded-[40px] shadow-sm border border-gray-100 p-2 flex flex-wrap gap-2">
                {[
                    { id: 'pendientes', label: 'Por Entregar', icon: <FiActivity />, count: resumen.totalEnCamino },
                    { id: 'entregados', label: 'Entregados', icon: <FiCheckCircle />, count: resumen.totalEntregados },
                    { id: 'noEntregados', label: 'No Entregados', icon: <FiXCircle />, count: resumen.totalNoEntregados },
                    { id: 'historial', label: 'Ver Todo', icon: <FiSearch />, count: historial.length }
                ].map((tab) => (
                    <button
                        key={tab.id}
                        onClick={() => setActiveTab(tab.id as any)}
                        className={`flex-1 min-w-[120px] sm:min-w-[140px] flex items-center justify-center gap-2 py-4 px-4 sm:px-6 rounded-[28px] text-[10px] sm:text-xs font-black uppercase tracking-widest transition-all duration-300 ${activeTab === tab.id
                            ? 'bg-patisserie-red text-white shadow-lg shadow-red-900/20 scale-[1.02]'
                            : 'bg-transparent text-gray-400 hover:bg-gray-50'
                            }`}
                    >
                        {tab.icon} {tab.label}
                        <span className={`ml-1 px-2 py-0.5 rounded-full text-[10px] ${activeTab === tab.id ? 'bg-white/20' : 'bg-gray-100'}`}>
                            {tab.count}
                        </span>
                    </button>
                ))}
            </div>

            {/* Content List */}
            <div className="grid gap-6">
                {displayList.length === 0 ? (
                    <div className="bg-white rounded-[40px] p-20 text-center border-2 border-dashed border-gray-100">
                        <FiPackage className="mx-auto text-gray-100 mb-6" size={80} />
                        <h3 className="text-xl font-black text-gray-300 uppercase tracking-tighter">Sin pedidos en esta lista</h3>
                    </div>
                ) : (
                    displayList.map((pedido: any) => (
                        <div
                            key={pedido.id}
                            className="bg-white rounded-[40px] shadow-sm border border-gray-100 overflow-hidden hover:shadow-2xl hover:border-patisserie-red/20 transition-all duration-500 group"
                        >
                            <div className="p-8 border-b border-gray-50 flex flex-col md:flex-row justify-between items-start md:items-center gap-4 bg-gray-50/50 group-hover:bg-white transition-colors">
                                <div className="flex gap-4 items-center">
                                    <div className="w-14 h-14 bg-white rounded-2xl shadow-sm border border-gray-100 flex items-center justify-center text-patisserie-red font-black text-xl">
                                        #{pedido.id}
                                    </div>
                                    <div>
                                        <div className="flex items-center gap-2 mb-1">
                                            <span className={`text-[9px] font-black px-2 py-0.5 rounded-lg uppercase tracking-widest shadow-sm ${pedido.estado === 'Entregado' ? 'bg-green-100 text-green-700' :
                                                pedido.estado === 'NoEntregado' ? 'bg-orange-100 text-orange-700' :
                                                    'bg-patisserie-red text-white'
                                                }`}>
                                                {pedido.estado}
                                            </span>
                                            <span className="text-[10px] font-bold text-gray-400">
                                                {new Date(pedido.fechaPedido).toLocaleDateString()}
                                            </span>
                                        </div>
                                        <h3 className="font-black text-gray-900 text-xl uppercase tracking-tighter flex items-center gap-2">
                                            {pedido.usuario?.nombre}
                                        </h3>
                                    </div>
                                </div>
                                <div className="md:text-right w-full md:w-auto p-4 md:p-0 bg-white md:bg-transparent rounded-3xl border border-gray-100 md:border-none">
                                    <p className="text-gray-400 text-[10px] uppercase font-black tracking-[0.2em] mb-1">Venta Total</p>
                                    <p className="text-3xl font-black text-patisserie-red leading-none">${pedido.total.toLocaleString()}</p>
                                </div>
                            </div>

                            <div className="p-8 space-y-6">
                                <div className="flex gap-5 bg-gray-50/50 p-6 rounded-[30px] border border-gray-100">
                                    <div className="shrink-0 w-12 h-12 bg-white rounded-2xl flex items-center justify-center text-patisserie-red shadow-sm">
                                        <FiMapPin size={24} />
                                    </div>
                                    <div className="flex-1">
                                        <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1">Dirección de Entrega</p>
                                        <p className="text-lg font-black text-gray-800 leading-snug">
                                            {pedido.direccionEnvio?.direccion || 'S/D'}
                                        </p>
                                        <p className="text-sm font-bold text-gray-400 mt-1 capitalize">
                                            {pedido.direccionEnvio?.ciudad || 'Bogotá'}
                                        </p>

                                        {pedido.direccionEnvio?.notas && (
                                            <div className="mt-4 bg-patisserie-red/5 p-4 rounded-2xl text-[11px] font-bold text-patisserie-red border border-patisserie-red/10 flex gap-3 italic">
                                                <span className="shrink-0 uppercase font-black [writing-mode:vertical-lr] border-r border-patisserie-red/20 pr-1">Nota</span>
                                                &quot;{pedido.direccionEnvio.notas}&quot;
                                            </div>
                                        )}
                                    </div>
                                </div>

                                {activeTab === 'pendientes' && (
                                    <div className="flex flex-col sm:flex-row gap-4 pt-2">
                                        <a
                                            href={`tel:${pedido.usuario?.telefono || ''}`}
                                            className="flex-1 bg-white border-2 border-gray-100 hover:border-gray-900 text-gray-900 py-4 rounded-[24px] font-black uppercase text-xs tracking-widest flex items-center justify-center gap-2 transition-all hover:shadow-xl active:scale-95 group"
                                        >
                                            <FiPhone size={18} className="group-hover:animate-bounce" /> Llamar Cliente
                                        </a>

                                        <button
                                            onClick={() => updateEstado(pedido.id, 'NoEntregado')}
                                            className="flex-1 bg-orange-50 hover:bg-orange-100 text-orange-700 py-4 rounded-[24px] font-black uppercase text-xs tracking-widest flex items-center justify-center gap-2 transition-all active:scale-95"
                                        >
                                            <FiXCircle size={18} /> Falló Entrega
                                        </button>

                                        <button
                                            onClick={() => updateEstado(pedido.id, 'Entregado')}
                                            className="flex-[1.5] bg-patisserie-red hover:bg-red-900 text-white py-4 rounded-[24px] font-black uppercase text-xs tracking-widest flex items-center justify-center gap-2 transition-all shadow-xl shadow-red-900/20 active:scale-95 group"
                                        >
                                            <FiCheckCircle size={18} className="group-hover:scale-125 transition-transform" /> Confirmar Entrega
                                        </button>
                                    </div>
                                )}

                                {activeTab !== 'pendientes' && pedido.fechaEntrega && (
                                    <div className="flex items-center gap-2 text-[10px] font-black text-gray-400 bg-gray-50 px-4 py-2 rounded-full w-fit">
                                        <FiClock /> CERRADO EL {new Date(pedido.fechaEntrega).toLocaleString()}
                                    </div>
                                )}
                            </div>

                            <div
                                onClick={() => setSelectedPedido(pedido)}
                                className="bg-gray-900 px-8 py-4 flex justify-between items-center group-hover:bg-patisserie-red cursor-pointer transition-colors duration-500"
                            >
                                <p className="text-[10px] text-white/50 font-black uppercase tracking-[0.2em] flex items-center gap-2">
                                    <FiPackage /> {pedido.itemsCount || 0} productos empacados
                                </p>
                                <span className="text-[10px] text-white font-black uppercase tracking-widest opacity-0 group-hover:opacity-100 transition-opacity">
                                    Ver Detalles <FiArrowRight className="inline ml-1" />
                                </span>
                            </div>
                        </div>
                    ))
                )}
            </div>

            {/* Modal de Detalle Pedido */}
            {selectedPedido && (
                <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-50 p-4 backdrop-blur-sm animate-fade-in">
                    <div className="bg-white rounded-2xl shadow-2xl w-full max-w-2xl overflow-hidden animate-scale-in">
                        <div className="bg-patisserie-red text-white px-8 py-5 flex justify-between items-center">
                            <div>
                                <h2 className="text-xl font-bold flex items-center gap-2"><FiPackage /> Detalle Pedido #{selectedPedido.id}</h2>
                                <p className="text-white/70 text-xs mt-1">{new Date(selectedPedido.fechaPedido).toLocaleString()}</p>
                            </div>
                            <button onClick={() => setSelectedPedido(null)} className="p-2 hover:bg-white/10 rounded-full transition-colors">
                                <FiX size={24} />
                            </button>
                        </div>

                        <div className="p-8 space-y-6 max-h-[70vh] overflow-y-auto">
                            <div className="bg-gray-50 rounded-2xl p-4 border border-gray-100 flex items-center gap-4">
                                <div className="p-3 bg-white rounded-xl shadow-sm"><FiUser className="text-patisserie-red" /></div>
                                <div>
                                    <p className="text-[10px] uppercase font-black text-gray-400">Cliente</p>
                                    <p className="font-bold text-gray-800">{selectedPedido.usuario?.nombre}</p>
                                    <p className="text-xs text-gray-500">{selectedPedido.usuario?.email}</p>
                                </div>
                            </div>

                            <div className="space-y-4">
                                <h3 className="text-[10px] font-black text-gray-400 uppercase tracking-widest flex items-center gap-2">
                                    <FiPackage className="text-patisserie-red" /> Items del Pedido
                                </h3>
                                <div className="space-y-3">
                                    {selectedPedido.items?.map((item: any, idx: number) => (
                                        <div key={idx} className="flex justify-between items-center p-3 bg-gray-50 rounded-xl border border-gray-100">
                                            <div className="flex items-center gap-3">
                                                <div className="w-10 h-10 bg-white rounded-lg flex items-center justify-center text-patisserie-red font-bold shadow-sm border border-gray-100">
                                                    {item.cantidad}x
                                                </div>
                                                <div>
                                                    <p className="text-sm font-bold text-gray-800">{item.producto?.nombre || 'Producto'}</p>
                                                    <p className="text-[10px] text-gray-400 capitalize">{item.producto?.categoria || 'General'}</p>
                                                </div>
                                            </div>
                                            <p className="font-black text-patisserie-red text-sm">${(item.precioUnitario * item.cantidad).toLocaleString()}</p>
                                        </div>
                                    ))}
                                    {(!selectedPedido.items || selectedPedido.items.length === 0) && (
                                        <p className="text-sm text-gray-600 italic">
                                            Este pedido contiene {selectedPedido.itemsCount} productos.
                                        </p>
                                    )}
                                </div>
                            </div>

                            <button
                                onClick={() => setSelectedPedido(null)}
                                className="w-full bg-gray-900 text-white py-4 rounded-2xl font-black uppercase text-xs tracking-widest hover:bg-patisserie-red transition-colors"
                            >
                                Cerrar Ventana
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Modal de Fallo de Entrega (Nuevo y Profesional) */}
            {failModalOpen && (
                <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-[60] p-4 backdrop-blur-sm animate-fade-in">
                    <div className="bg-white rounded-[40px] shadow-2xl w-full max-w-lg overflow-hidden animate-scale-in">
                        <div className="bg-orange-600 text-white px-8 py-6 flex justify-between items-center">
                            <div>
                                <h2 className="text-xl font-black flex items-center gap-2 uppercase tracking-tight">
                                    <FiXCircle size={24} /> Reportar Fallo
                                </h2>
                                <p className="text-white/70 text-[10px] font-bold uppercase tracking-widest mt-1">Pedido #{pedidoToFail}</p>
                            </div>
                            <button onClick={() => setFailModalOpen(false)} className="p-2 hover:bg-white/10 rounded-full transition-colors">
                                <FiX size={24} />
                            </button>
                        </div>

                        <div className="p-8 space-y-6">
                            <div className="space-y-4">
                                <p className="text-gray-500 font-bold text-sm">Por favor, seleccione el motivo por el cual no se pudo completar la entrega:</p>

                                <div className="grid gap-3">
                                    {[
                                        "Cliente no estaba presente",
                                        "Dirección incorrecta",
                                        "Cliente rechazó pedido",
                                        "No se pudo contactar al cliente",
                                        "Otro"
                                    ].map((motivo) => (
                                        <button
                                            key={motivo}
                                            onClick={() => setMotivoFail(motivo)}
                                            className={`w-full text-left p-4 rounded-2xl border-2 transition-all font-bold text-sm flex items-center justify-between ${motivoFail === motivo
                                                ? 'border-orange-600 bg-orange-50 text-orange-700'
                                                : 'border-gray-100 hover:border-gray-200 text-gray-600'
                                                }`}
                                        >
                                            {motivo}
                                            {motivoFail === motivo && <FiCheckCircle />}
                                        </button>
                                    ))}
                                </div>

                                {motivoFail === "Otro" && (
                                    <textarea
                                        value={otroMotivo}
                                        onChange={(e) => setOtroMotivo(e.target.value)}
                                        placeholder="Escriba el motivo detallado aquí..."
                                        className="w-full p-4 bg-gray-50 border-2 border-gray-100 rounded-2xl outline-none focus:border-orange-600 transition-all font-medium text-sm"
                                        rows={3}
                                    />
                                )}
                            </div>

                            <div className="flex gap-3 pt-2">
                                <button
                                    onClick={() => setFailModalOpen(false)}
                                    className="flex-1 bg-gray-100 text-gray-500 py-4 rounded-2xl font-black uppercase text-xs tracking-widest hover:bg-gray-200 transition-all"
                                >
                                    Cancelar
                                </button>
                                <button
                                    onClick={() => {
                                        const finalMotivo = motivoFail === "Otro" ? otroMotivo : motivoFail;
                                        if (!finalMotivo.trim()) {
                                            toast.error("Debe especificar un motivo");
                                            return;
                                        }
                                        updateEstado(pedidoToFail!, 'NoEntregado', finalMotivo);
                                    }}
                                    disabled={!motivoFail || (motivoFail === "Otro" && !otroMotivo.trim())}
                                    className="flex-2 bg-orange-600 text-white py-4 px-8 rounded-2xl font-black uppercase text-xs tracking-widest hover:bg-orange-700 transition-all shadow-lg shadow-orange-900/20 disabled:opacity-50 disabled:shadow-none"
                                >
                                    Confirmar Reporte
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default RepartidorDashboard;
