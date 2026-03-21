import { useEffect, useState } from 'react';
import api from '../../api/axios';
import {
  Clock, CheckCircle, ChevronDown, ChevronUp,
  Package, User, Search, Hash, Calendar,
  Truck, AlertCircle, Settings
} from 'lucide-react';
import toast from 'react-hot-toast';
import { formatCurrency } from '../../utils/format';

// Tipos definidos localmente para asegurar compatibilidad
interface PedidoItem {
  id: number;
  nombreProducto: string;
  cantidad: number;
  subtotal: number;
}

interface PedidoAdmin {
  id: number;
  usuarioId: number;
  nombreUsuario?: string;
  usuario?: {
    nombre: string;
    email: string;
  };
  fechaPedido: string;
  total: number;
  estado: string;
  items: PedidoItem[];
}

const PedidosAdmin = () => {
  const [pedidos, setPedidos] = useState<PedidoAdmin[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<'gestion' | 'historial'>('gestion');
  const [expandedRow, setExpandedRow] = useState<number | null>(null);
  const [repartidores, setRepartidores] = useState<{ id: number, nombre: string }[]>([]);

  useEffect(() => {
    fetchPedidos();
    fetchRepartidores();
  }, []);

  const fetchRepartidores = async () => {
    try {
      const response = await api.get('/users');
      let users = [];
      if (Array.isArray(response.data)) users = response.data;
      else if (response.data?.data) users = response.data.data;

      const reps = users
        .filter((u: any) => u.roles?.includes('Repartidor'))
        .map((u: any) => ({ id: u.id, nombre: u.nombre }));
      setRepartidores(reps);
    } catch (error) {
      console.error("Error al cargar repartidores:", error);
    }
  };

  const fetchPedidos = async () => {
    try {
      setLoading(true);
      const response = await api.get('/pedidos/todos');
      const rawData = response.data?.data || response.data?.result || response.data || [];
      const dataArray = Array.isArray(rawData) ? rawData : [];
      // Ordenar por ID descendente
      const sorted = dataArray.sort((a: PedidoAdmin, b: PedidoAdmin) => b.id - a.id);
      setPedidos(sorted);
    } catch (error) {
      console.error(error);
      toast.error('Error al cargar pedidos');
    } finally {
      setLoading(false);
    }
  };

  const cambiarEstado = async (id: number, nuevoEstado: string) => {
    try {
      await api.put(`/pedidos/${id}/estado`, { estado: nuevoEstado });
      toast.success(`Pedido #${id} actualizado`);
      fetchPedidos();
    } catch (error) {
      toast.error('Error al actualizar estado');
    }
  };

  const asignarRepartidor = async (pedidoId: number, repartidorId: number) => {
    if (!repartidorId) return;
    try {
      await api.patch(`/pedidos/${pedidoId}/asignar-repartidor`, repartidorId, {
        headers: { 'Content-Type': 'application/json' }
      });
      toast.success(`Repartidor asignado`);
      fetchPedidos();
    } catch (error) {
      toast.error('Error al asignar repartidor');
    }
  };

  const toggleRow = (id: number) => {
    setExpandedRow(expandedRow === id ? null : id);
  };

  // Helper para sacar el nombre del cliente (tu lógica segura)
  const getClientName = (pedido: PedidoAdmin) => {
    if (pedido.usuario?.nombre) return pedido.usuario.nombre;
    if (pedido.nombreUsuario) return pedido.nombreUsuario;
    if (pedido.usuario?.email) return pedido.usuario.email.split('@')[0];
    return 'Cliente Web'; // Si ves esto, es que el backend no manda datos de usuario
  };

  const getStatusColor = (estado: string) => {
    switch (estado) {
      case 'Entregado': return 'bg-green-100 text-green-700 border-green-200';
      case 'Pendiente': return 'bg-red-100 text-red-700 border-red-200';
      case 'Enviado': return 'bg-blue-100 text-blue-700 border-blue-200';
      case 'Cancelado': return 'bg-gray-100 text-gray-500 border-gray-200';
      default: return 'bg-yellow-100 text-yellow-700 border-yellow-200';
    }
  };

  // Filtrado de pestañas
  const pedidosActivos = pedidos.filter(p => ['Pendiente', 'Confirmado', 'En Preparación', 'Enviado'].includes(p.estado));
  const pedidosHistorial = pedidos.filter(p => ['Entregado', 'Cancelado'].includes(p.estado));
  const listaAMostrar = activeTab === 'gestion' ? pedidosActivos : pedidosHistorial;

  return (
    <div className="animate-fade-in p-2 pb-20">
      {/* Cabecera Premium */}
      <div className="flex flex-col md:flex-row justify-between items-center mb-10 gap-6">
        <div>
          <h1 className="text-4xl font-serif font-black text-[#5D1919] tracking-tighter mb-2">Libro de Comandas</h1>
          <p className="text-gray-500 font-medium">Gestión integral de pedidos, despachos y trazabilidad de clientes</p>
        </div>
        <div className="flex gap-4">
          {/* Podríamos añadir un botón de exportar o algo similar aquí en el futuro */}
        </div>
      </div>

      {/* PESTAÑAS REDISEÑADAS */}
      <div className="flex bg-gray-100/50 p-1.5 rounded-[1.5rem] w-fit mb-8 border border-gray-100 shadow-inner">
        <button
          onClick={() => setActiveTab('gestion')}
          className={`px-8 py-3 rounded-2xl font-black text-[10px] uppercase tracking-[0.2em] flex items-center gap-3 transition-all ${activeTab === 'gestion'
            ? 'bg-white text-[#5D1919] shadow-md scale-100'
            : 'text-gray-400 hover:text-gray-600'
            }`}
        >
          <Clock size={14} strokeWidth={3} /> En Preparación ({pedidosActivos.length})
        </button>
        <button
          onClick={() => setActiveTab('historial')}
          className={`px-8 py-3 rounded-2xl font-black text-[10px] uppercase tracking-[0.2em] flex items-center gap-3 transition-all ${activeTab === 'historial'
            ? 'bg-white text-[#5D1919] shadow-md scale-100'
            : 'text-gray-400 hover:text-gray-600'
            }`}
        >
          <CheckCircle size={14} strokeWidth={3} /> Archivo Histórico ({pedidosHistorial.length})
        </button>
      </div>

      {/* TABLA REDISEÑADA */}
      <div className="bg-white rounded-[2.5rem] shadow-[0_10px_40px_rgba(0,0,0,0.03)] border border-gray-100 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm text-gray-600">
            <thead className="bg-[#fcfcfc] border-b border-gray-50">
              <tr>
                <th className="px-8 py-6 text-[10px] font-black uppercase tracking-widest text-gray-400">ID Orden</th>
                <th className="px-8 py-6 text-[10px] font-black uppercase tracking-widest text-gray-400">Comensal / Cliente</th>
                <th className="px-8 py-6 text-[10px] font-black uppercase tracking-widest text-gray-400">Momento de Pedido</th>
                <th className="px-8 py-6 text-[10px] font-black uppercase tracking-widest text-gray-400">Inversión Total</th>
                <th className="px-8 py-6 text-[10px] font-black uppercase tracking-widest text-gray-400">Estado Vital</th>
                <th className="px-8 py-6 text-center text-[10px] font-black uppercase tracking-widest text-gray-400">Desglose</th>
                {activeTab === 'gestion' && <th className="px-8 py-6 text-[10px] font-black uppercase tracking-widest text-gray-400">Logística</th>}
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {loading ? (
                <tr><td colSpan={7} className="text-center py-10">Cargando...</td></tr>
              ) : listaAMostrar.length === 0 ? (
                <tr><td colSpan={7} className="text-center py-10 text-gray-400">No hay pedidos aquí.</td></tr>
              ) : (
                listaAMostrar.map((pedido) => (
                  <>
                    <tr key={pedido.id} className={`hover:bg-[#5D1919]/[0.02] transition-all group ${expandedRow === pedido.id ? 'bg-[#5D1919]/[0.01]' : ''}`}>
                      <td className="px-8 py-5">
                        <div className="flex items-center gap-2">
                          <Hash size={12} className="text-gray-300" />
                          <span className="font-black text-gray-900">{pedido.id}</span>
                        </div>
                      </td>
                      <td className="px-8 py-5">
                        <div className="flex items-center gap-3">
                          <div className="p-2 bg-gray-50 rounded-xl text-gray-400 group-hover:text-[#5D1919] group-hover:bg-[#5D1919]/5 transition-all">
                            <User size={14} />
                          </div>
                          <span className="font-serif font-black text-gray-900 group-hover:text-[#5D1919] transition-colors">{getClientName(pedido)}</span>
                        </div>
                      </td>
                      <td className="px-8 py-5">
                        <div className="flex items-center gap-2 text-gray-500 font-medium">
                          <Calendar size={14} className="text-gray-300" />
                          {new Date(pedido.fechaPedido).toLocaleDateString(undefined, { day: '2-digit', month: 'short', year: 'numeric' })}
                        </div>
                      </td>
                      <td className="px-8 py-5">
                        <div className="flex items-center gap-1 font-black text-[#5D1919]">
                          <span className="text-[10px] opacity-50">$</span>
                          <span>{pedido.total.toLocaleString()}</span>
                        </div>
                      </td>
                      <td className="px-8 py-5">
                        <span className={`px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest border ${getStatusColor(pedido.estado)} shadow-sm`}>
                          {pedido.estado}
                        </span>
                      </td>
                      <td className="px-8 py-5 text-center">
                        <button
                          onClick={() => toggleRow(pedido.id)}
                          className={`p-2 rounded-xl transition-all ${expandedRow === pedido.id ? 'bg-[#5D1919] text-white shadow-lg' : 'bg-gray-50 text-gray-400 hover:text-[#5D1919] hover:bg-gray-100'}`}
                        >
                          {expandedRow === pedido.id ? <ChevronUp size={18} strokeWidth={3} /> : <ChevronDown size={18} strokeWidth={3} />}
                        </button>
                      </td>
                      {activeTab === 'gestion' && (
                        <td className="px-8 py-5">
                          <div className="relative group/sel">
                            <select
                              className="w-full bg-gray-50 border border-gray-100 rounded-xl px-4 py-2 text-[10px] font-black uppercase tracking-widest text-[#5D1919] cursor-pointer outline-none focus:ring-4 focus:ring-[#5D1919]/5 transition-all appearance-none"
                              value={pedido.estado}
                              onChange={(e) => cambiarEstado(pedido.id, e.target.value)}
                            >
                              <option value="Pendiente">Pendiente</option>
                              <option value="En Preparación">Preparando</option>
                              <option value="Enviado">Enviado</option>
                              <option value="Entregado">Entregado</option>
                              <option value="Cancelado">Cancelar</option>
                            </select>
                            <Settings className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-300 pointer-events-none group-hover/sel:text-[#5D1919] transition-colors" size={10} />
                          </div>
                        </td>
                      )}
                    </tr>
                    {/* EXPANDIBLE REDISEÑADO */}
                    {expandedRow === pedido.id && (
                      <tr className="bg-white group">
                        <td colSpan={7} className="px-10 py-8 border-b border-gray-50 bg-[#fcfcfc]/80 backdrop-blur-sm">
                          <div className="grid grid-cols-1 md:grid-cols-2 gap-10 animate-slide-up">

                            {/* Lista de Productos */}
                            <div className="bg-white rounded-[2rem] p-8 shadow-sm border border-gray-100">
                              <h4 className="text-[10px] font-black text-gray-400 uppercase tracking-[0.2em] mb-6 flex items-center gap-3">
                                <Package size={14} className="text-[#5D1919]" />
                                Composición de la Orden
                              </h4>
                              <div className="space-y-4">
                                {pedido.items?.map((item, idx) => (
                                  <div key={idx} className="flex justify-between items-center group/item p-2 hover:bg-gray-50 rounded-xl transition-all">
                                    <div className="flex flex-col">
                                      <span className="text-sm font-serif font-black text-gray-900 capitalize">{item.nombreProducto}</span>
                                      <span className="text-[10px] text-gray-400 font-bold uppercase tracking-widest">{item.cantidad} Unidades</span>
                                    </div>
                                    <span className="font-black text-[#5D1919] text-sm">
                                      {formatCurrency ? formatCurrency(item.subtotal) : `$${item.subtotal.toLocaleString()}`}
                                    </span>
                                  </div>
                                ))}
                                {(!pedido.items || pedido.items.length === 0) && (
                                  <div className="text-center py-6 italic text-gray-400 flex flex-col items-center gap-2">
                                    <AlertCircle size={24} className="opacity-20" />
                                    <p className="text-xs">No se encontraron detalles del desglose.</p>
                                  </div>
                                )}
                              </div>
                            </div>

                            {/* Gestión de Logística */}
                            <div className="bg-white rounded-[2rem] p-8 shadow-sm border border-gray-100 flex flex-col justify-between">
                              <div>
                                <h4 className="text-[10px] font-black text-gray-400 uppercase tracking-[0.2em] mb-6 flex items-center gap-3">
                                  <Truck size={14} className="text-[#5D1919]" />
                                  Asignación de Logística
                                </h4>
                                <div className="space-y-6">
                                  <div className="space-y-3">
                                    <label className="text-[9px] font-black text-gray-400 uppercase tracking-widest ml-1">Personal de Reparto</label>
                                    <div className="relative">
                                      <select
                                        className="w-full bg-gray-50 border border-gray-100 rounded-2xl px-6 py-4 text-sm font-bold text-gray-700 outline-none focus:ring-4 focus:ring-[#5D1919]/5 transition-all appearance-none cursor-pointer"
                                        value={repartidores.find(r => r.id === pedido.usuarioId)?.id || ""}
                                        onChange={(e) => asignarRepartidor(pedido.id, parseInt(e.target.value))}
                                      >
                                        <option value="">Seleccionar Repartidor Maestros...</option>
                                        {repartidores.map(r => (
                                          <option key={r.id} value={r.id}>{r.nombre}</option>
                                        ))}
                                      </select>
                                      <div className="absolute right-6 top-1/2 -translate-y-1/2 text-gray-300 pointer-events-none">
                                        <ChevronDown size={14} />
                                      </div>
                                    </div>
                                  </div>

                                  <div className="p-4 bg-emerald-50 rounded-2xl border border-emerald-100 flex items-center gap-4">
                                    <div className="w-10 h-10 rounded-xl bg-emerald-500 text-white flex items-center justify-center shadow-lg shadow-emerald-500/20">
                                      <CheckCircle size={18} />
                                    </div>
                                    <div>
                                      <p className="text-[10px] font-black text-emerald-800 uppercase tracking-widest">Pago Confirmado</p>
                                      <p className="text-xs font-serif font-black text-emerald-900">Total: {formatCurrency ? formatCurrency(pedido.total) : `$${pedido.total.toLocaleString()}`}</p>
                                    </div>
                                  </div>
                                </div>
                              </div>

                              <div className="mt-8 flex justify-end">
                                <button
                                  onClick={() => toast.success(`Comprobante #${pedido.id} generado.`)}
                                  className="text-[9px] font-black text-gray-400 uppercase tracking-widest hover:text-[#5D1919] flex items-center gap-2 transition-all p-2"
                                >
                                  <Search size={12} /> Ver Auditoría completa
                                </button>
                              </div>
                            </div>

                          </div>
                        </td>
                      </tr>
                    )}
                  </>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default PedidosAdmin;