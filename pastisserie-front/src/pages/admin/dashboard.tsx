import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { DollarSign, ShoppingCart, TrendingUp, TrendingDown, Layers, AlertCircle, Calendar, ArrowRight, XCircle } from 'lucide-react';
import { FiActivity } from 'react-icons/fi';
import {
  AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, ResponsiveContainer,
  Cell, PieChart, Pie, Legend
} from 'recharts';


// Servicios
import { dashboardService } from '../../services/dashboardService';
import { productService } from '../../services/productService';
import { reclamacionesService } from '../../services/reclamacionesService';
import type { Reclamacion } from '../../services/reclamacionesService';

// Componentes
import ShopStatusWidget from '../../components/admin/ShopStatusWidget';


const Dashboard = () => {
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  // Estado para los datos reales del dashboard
  const [data, setData] = useState<any>(null);
  const [earningsHistory, setEarningsHistory] = useState<any[]>([]);
  const [dateRange, setDateRange] = useState({
    start: new Date(new Date().setDate(new Date().getDate() - 7)).toISOString().split('T')[0],
    end: new Date().toISOString().split('T')[0]
  });
  const [isHistorialLoading, setIsHistorialLoading] = useState(false);
  const [reclamaciones, setReclamaciones] = useState<Reclamacion[]>([]);

  const COLORS = ['#7D2121', '#EBCfa8', '#111111', '#5D1919', '#9ca3af'];

  const categoryData = data?.pedidosPorCategoria || [
    { name: 'Pastelería', value: 400 },
    { name: 'Panadería', value: 300 },
    { name: 'Bebidas', value: 200 },
    { name: 'Otros', value: 100 }
  ];

  const loadData = async () => {
    try {
      const [stats, productsData] = await Promise.all([
        dashboardService.getAdminStats(),
        productService.getAll()
      ]);

      setData(stats);

      // Stock bajo
      const productsList = Array.isArray(productsData) ? productsData : productsData?.data || [];
      if (Array.isArray(productsList)) {
        // setLowStock(productsList.filter((p: any) => p.stock < 5).sort((a, b) => a.stock - b.stock)); // Removed lowStock usage
      }

      // Reclamaciones con validación de ID positivo
      try {
        const reclamResponse = await reclamacionesService.getAllReclamaciones();
        const reclamArr = reclamResponse?.data || reclamResponse || [];
        if (Array.isArray(reclamArr)) {
          const validReclamArr = reclamArr.filter((r: any) => r.id > 0 && r.pedidoId > 0);
          setReclamaciones(validReclamArr);
        }
      } catch {
        // Ignorar error si no hay registros
      }


      setLoading(false);
    } catch (error) {
      console.error("Error cargando el panel administrativo:", error);
      setLoading(false);
    }
  };

  const fetchHistory = async (start?: string, end?: string) => {
    setIsHistorialLoading(true);
    try {
      const resp = await dashboardService.getEarningsHistory(start || dateRange.start, end || dateRange.end);
      if (resp?.dailyData) {
        setEarningsHistory(resp.dailyData);
      } else {
        setEarningsHistory(Array.isArray(resp) ? resp : []);
      }
    } catch (error) {
      console.error("Error cargando historial:", error);
    } finally {
      setIsHistorialLoading(false);
    }
  };

  const setQuickFilter = (type: 'hoy' | 'semana' | 'mes') => {
    const end = new Date();
    let start = new Date();

    if (type === 'semana') start.setDate(end.getDate() - 7);
    else if (type === 'mes') start.setMonth(end.getMonth() - 1);

    const startStr = start.toISOString().split('T')[0];
    const endStr = end.toISOString().split('T')[0];

    setDateRange({ start: startStr, end: endStr });
    fetchHistory(startStr, endStr);
  };



  useEffect(() => {
    loadData();
    const interval = setInterval(loadData, 30000); // Polling cada 30s
    return () => clearInterval(interval);
  }, []);

  if (loading || !data) return (
    <div className="flex items-center justify-center h-96">
      <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#7D2121]"></div>
    </div>
  );

  return (
    <div className="animate-fade-in space-y-8 pb-10">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-end gap-4 border-b border-gray-100 pb-8">
        <div className="animate-slide-down">
          <h1 className="text-4xl font-serif font-black text-gray-900 tracking-tighter mb-2 italic">
            Panel de <span className="text-[#7D2121]">Gestión Administrativa</span>
          </h1>
          <p className="text-gray-600 font-black max-w-2xl text-xs uppercase tracking-tight opacity-70">
            Resumen financiero y operativo del negocio. Monitoreo de inventario, flujos de caja y análisis de ventas para Patisserie Deluxe.
          </p>
        </div>
        <div className="flex-1 max-w-sm animate-slide-left">
          <ShopStatusWidget />
        </div>
      </div>


      {/* KPI CARDS - Premium Grid with Upgraded Typography */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4 xl:gap-8">
        {[
          {
            label: 'Ventas Diarias',
            val: `$${data.ventasHoy.toLocaleString()}`,
            tag: 'Hoy',
            icon: <DollarSign size={20} strokeWidth={2.5} />,
            color: 'emerald',
            route: '/admin/pedidos?filter=hoy'
          },
          {
            label: 'Ingresos Mensuales',
            val: `$${data.ventasMes.toLocaleString()}`,
            tag: 'Mes',
            icon: <TrendingUp size={20} strokeWidth={2.5} />,
            color: 'sky',
            route: '/admin/pedidos?filter=mes'
          },
          {
            label: 'Pedidos en Cola',
            val: data.pedidosPendientes,
            tag: 'Pendientes',
            icon: <ShoppingCart size={20} strokeWidth={2.5} />,
            color: 'amber',
            route: '/admin/pedidos?filter=pendiente',
            alert: data.pedidosPendientes > 0
          },
          {
            label: (data.alertasCriticas?.conteoRetrasados || 0) > 0
              ? 'Alertas de Operación'
              : ((data.pedidosPorEstado?.NoEntregado || 0) > 0 ? 'Auditoría Crítica' : 'Salud Operacional'),
            val: (data.alertasCriticas?.conteoRetrasados || 0) + (data.pedidosPorEstado?.NoEntregado || 0) > 0
              ? (data.alertasCriticas?.conteoRetrasados || 0) + (data.pedidosPorEstado?.NoEntregado || 0)
              : "Estable",
            tag: 'Indicadores Críticos',
            icon: <AlertCircle size={20} strokeWidth={2.5} />,
            color: (data.alertasCriticas?.conteoRetrasados || 0) > 0 || (data.pedidosPorEstado?.NoEntregado || 0) > 0 ? 'rose' : 'emerald',
            route: '/admin/pedidos?filter=fallido',
            info: (data.alertasCriticas?.conteoRetrasados || 0) > 0
              ? `${data.alertasCriticas.conteoRetrasados} pedidos con retraso`
              : ((data.pedidosPorEstado?.NoEntregado || 0) > 0 ? "Revisar entregas fallidas" : "Flujo de trabajo optimizado")
          },
          {
            label: 'Valor Neto Master',
            val: `$${data.gananciasTotales.toLocaleString()}`,
            tag: 'Valor Total Neto',
            icon: <Layers size={20} strokeWidth={2.5} />,
            color: 'master',
            route: '/admin/pedidos'
          }
        ].map((kpi, idx) => (
          <div
            key={idx}
            onClick={() => navigate(kpi.route)}
            style={{ animationDelay: `${idx * 100}ms` }}
            className={`animate-slide-up bg-white p-8 rounded-[3rem] shadow-sm border flex flex-col justify-between hover:shadow-2xl hover:-translate-y-2 transition-all duration-500 group cursor-pointer relative overflow-hidden ${kpi.color === 'master' ? 'border-2 border-gray-900 !text-gray-900 bg-gray-50' :
              (kpi.color === 'rose') ? 'bg-rose-50 border-rose-100 !text-rose-900 shadow-rose-100' : 'border-gray-100'
              }`}
          >
            {kpi.color !== 'master' && (
              <div className={`absolute top-0 right-0 w-24 h-24 bg-${kpi.color.replace('-plain', '')}-100/30 rounded-bl-[3rem] -mr-8 -mt-8 opacity-40 group-hover:bg-${kpi.color.replace('-plain', '')}-100 transition-colors`}></div>
            )}
            <div className="flex justify-between items-start relative z-10">
              <div className={`p-4 rounded-2xl transition-all shadow-sm ${kpi.color === 'master' ? 'bg-gray-900 text-white group-hover:bg-gray-800' :
                `bg-${kpi.color.replace('-plain', '')}-100 text-${kpi.color.replace('-plain', '')}-600 group-hover:bg-${kpi.color.replace('-plain', '')}-600 group-hover:text-white`
                }`}>
                {kpi.icon}
              </div>
              <div className="flex flex-col items-end gap-1.5">
                <span className={`text-[11px] font-black px-4 py-1.5 rounded-full uppercase tracking-widest shadow-sm ${kpi.color === 'master' ? 'bg-gray-900 text-white' :
                  `bg-${kpi.color.replace('-plain', '')}-100 text-${kpi.color.replace('-plain', '')}-700`
                  }`}>{kpi.tag}</span>
                {kpi.alert && <span className="w-2.5 h-2.5 bg-rose-500 rounded-full animate-ping shadow-[0_0_10px_rgba(244,63,94,0.8)]"></span>}
              </div>
            </div>
            <div className="mt-10 relative z-10">
              <p className={`text-[10px] font-black uppercase tracking-[0.3em] mb-1 ${kpi.color === 'master' ? 'text-gray-900' : 'text-gray-600'
                }`}>{kpi.label}</p>
              {kpi.info && (
                <p className={`text-[8px] font-bold uppercase tracking-widest mb-3 text-gray-400`}>
                  {kpi.info}
                </p>
              )}
              <h3 className={`text-4xl font-sans font-black tracking-tight text-gray-900`}>
                {kpi.val}
              </h3>
            </div>
          </div>
        ))}
      </div>

      <div className="bg-gray-900 rounded-[3.5rem] p-12 shadow-2xl relative overflow-hidden animate-slide-up">
        {/* Background Accent */}
        <div className="absolute top-0 right-0 w-64 h-64 bg-[#7D2121]/10 rounded-full blur-3xl -mr-32 -mt-32"></div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-12 relative z-10">
          {/* Section 1: Global Critical Indicators */}
          <div className="">
            <div className="flex items-center gap-3 mb-8">
              <div className="p-3 bg-amber-900/40 rounded-2xl text-amber-400">
                <AlertCircle size={20} />
              </div>
              <div>
                <h4 className="text-white text-sm font-black uppercase tracking-widest">Indicadores Críticos</h4>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="bg-white/5 p-5 rounded-3xl border border-white/10 text-center hover:bg-white/10 transition-all">
                <p className="text-[10px] font-black text-white/30 uppercase mb-2">Stock Bajo</p>
                <div className="text-3xl font-serif font-black text-amber-500">{data.alertasCriticas?.stockBajo || 0}</div>
              </div>
              <div className="bg-white/5 p-5 rounded-3xl border border-white/10 text-center hover:bg-white/10 transition-all">
                <p className="text-[10px] font-black text-white/30 uppercase mb-2">Reclamos</p>
                <div className="text-3xl font-serif font-black text-emerald-500">{data.alertasCriticas?.reclamacionesPendientes || 0}</div>
              </div>
            </div>
            <button
              onClick={() => navigate('/admin/configuracion')}
              className="w-full mt-6 py-4 bg-[#EBCfa8] text-gray-900 font-black uppercase tracking-[0.2em] text-[10px] rounded-2xl hover:scale-[1.02] transition-all active:scale-95 shadow-lg shadow-[#EBCfa8]/10"
            >
              Configuración de Umbrales
            </button>
          </div>

          {/* Section 2: Audit Summary */}
          <div className="md:border-l md:border-white/10 md:pl-12">
            <div className="flex items-center gap-3 mb-8">
              <div className="p-3 bg-gray-800 rounded-2xl text-gray-400">
                <XCircle size={20} />
              </div>
              <div>
                <h4 className="text-white text-sm font-black uppercase tracking-widest">Resumen de Incidencias</h4>
              </div>
            </div>

            {(data.pedidosPorEstado?.NoEntregado || 0) > 0 ? (
              <div className="flex flex-col items-center">
                <div className="text-6xl font-serif font-black text-orange-500 mb-2">{data.pedidosPorEstado.NoEntregado}</div>
                <p className="text-[10px] font-black text-orange-500/60 uppercase tracking-widest mb-6">Incidencias Críticas</p>
              </div>
            ) : (
              <div className="py-12 border border-white/5 bg-white/5 rounded-[2.5rem] flex flex-col items-center justify-center text-center opacity-60">
                <FiActivity size={32} className="text-white/20 mb-4" />
                <p className="text-[10px] text-white/40 font-black uppercase tracking-[0.3em]">Operación Limpia</p>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* MID SECTION: CHARTS */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Historical Analysis Wrap */}
        <div className="lg:col-span-2 bg-white p-10 rounded-[3rem] shadow-[0_20px_60px_rgba(0,0,0,0.02)] border border-gray-100 animate-slide-up" style={{ animationDelay: '500ms' }}>
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-12 gap-6">
            <div>
              <h3 className="text-2xl font-serif font-black text-gray-900 tracking-tight">Resumen Financiero del Periodo</h3>
              <p className="text-gray-400 text-xs font-bold uppercase tracking-widest mt-2 flex items-center gap-2">
                <Calendar size={14} className="text-[#7D2121]" /> Flujo de caja del periodo seleccionado
              </p>
            </div>
            <div className="flex flex-wrap gap-2 bg-gray-50 p-1.5 rounded-2xl border border-gray-100">
              {['hoy', 'semana', 'mes'].map((f) => {
                const isSelected = (f === 'hoy' && dateRange.start === new Date().toISOString().split('T')[0]) ||
                  (f === 'semana' && dateRange.start === new Date(new Date().setDate(new Date().getDate() - 7)).toISOString().split('T')[0]) ||
                  (f === 'mes' && dateRange.start === new Date(new Date().setMonth(new Date().getMonth() - 1)).toISOString().split('T')[0]);
                return (
                  <button
                    key={f}
                    onClick={() => setQuickFilter(f as any)}
                    className={`px-4 py-2 text-[10px] font-black uppercase rounded-xl transition-all ${isSelected ? 'bg-[#7D2121] text-white shadow-lg' : 'text-gray-400 hover:text-gray-900'
                      }`}
                  >
                    {f === 'hoy' ? 'Día' : f === 'semana' ? 'Semana' : 'Mes'}
                  </button>
                );
              })}
            </div>
          </div>

          <div className="h-[350px] w-full relative">
            {isHistorialLoading && (
              <div className="absolute inset-0 bg-white/50 backdrop-blur-[2px] z-10 flex items-center justify-center rounded-3xl">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-[#7D2121]"></div>
              </div>
            )}
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={earningsHistory.length > 0 ? earningsHistory : data.ventasPorDia}>
                <defs>
                  <linearGradient id="colorSales" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#7D2121" stopOpacity={0.2} />
                    <stop offset="95%" stopColor="#7D2121" stopOpacity={0} />
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="12 12" vertical={false} stroke="#f1f5f9" />
                <XAxis dataKey="nombre" axisLine={false} tickLine={false} tick={{ fill: '#94a3b8', fontSize: 10, fontWeight: 800 }} dy={20} />
                <YAxis axisLine={false} tickLine={false} tick={{ fill: '#94a3b8', fontSize: 10, fontWeight: 800 }} dx={-10} />
                <RechartsTooltip
                  contentStyle={{ borderRadius: '2rem', border: 'none', boxShadow: '0 25px 50px -12px rgba(0,0,0,0.15)', padding: '24px', background: '#fff' }}
                  itemStyle={{ color: '#7D2121', fontWeight: 900, fontSize: '18px' }}
                  labelStyle={{ fontWeight: 900, fontSize: '12px', color: '#94a3b8', textTransform: 'uppercase', marginBottom: '8px' }}
                  formatter={(val: number) => [`$${val.toLocaleString()}`, 'Ganancia']}
                />
                <Area type="monotone" dataKey="ventas" stroke="#7D2121" strokeWidth={5} fill="url(#colorSales)" animationDuration={2000} />
              </AreaChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Side Panel: Distribution & Low Stock */}
        <div className="flex flex-col gap-8">
          {/* CATEGORY MIX - COMPACT INSIDE ANALYTICS ROW */}
          <div className="bg-white p-10 rounded-[3rem] shadow-[0_20px_60px_rgba(0,0,0,0.02)] border border-gray-100 flex-1 flex flex-col animate-slide-up" style={{ animationDelay: '700ms' }}>
            <div className="mb-6 text-center">
              <h3 className="text-xl font-serif font-black text-gray-900 tracking-tight">Distribución de Ventas por Categoría</h3>
              <p className="text-gray-400 text-[9px] font-bold uppercase tracking-widest mt-1">Análisis por línea de producto</p>
            </div>
            <div className="flex-1 min-h-[220px]">
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={categoryData}
                    innerRadius={50}
                    outerRadius={80}
                    paddingAngle={8}
                    dataKey="value"
                    animationDuration={1500}
                    stroke="none"
                  >
                    {categoryData.map((_: any, index: number) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <RechartsTooltip contentStyle={{ borderRadius: '1.5rem', border: 'none', boxShadow: '0 10px 15px -3px rgba(0,0,0,0.1)' }} />
                  <Legend verticalAlign="bottom" align="center" iconType="circle" formatter={(v) => <span className="text-[9px] font-black uppercase text-gray-500 ml-1">{v}</span>} />
                </PieChart>
              </ResponsiveContainer>
            </div>
          </div>
        </div>
      </div>

      {/* PRODUCT PERFORMANCE: SIDE-BY-SIDE COMPACT GRID */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 mt-16">
        {/* Top 5 Bestsellers */}
        <div className="bg-white p-8 rounded-[2.5rem] shadow-sm border border-gray-100 animate-slide-up">
          <div className="flex justify-between items-center mb-6">
            <h3 className="text-xl font-serif font-black text-gray-900 tracking-tight">Top 5 Productos Más Vendidos</h3>
            <div className="p-2.5 bg-emerald-50 text-emerald-600 rounded-xl shadow-inner">
              <TrendingUp size={20} />
            </div>
          </div>
          <div className="space-y-3">
            {!data.topMasVendidos || data.topMasVendidos.length === 0 ? (
              <p className="text-[11px] text-gray-400 italic py-4">Sin datos suficientes.</p>
            ) : (
              data.topMasVendidos.slice(0, 5).map((prod: any, i: number) => (
                <div key={i} className="flex items-center justify-between p-3.5 bg-gray-50 rounded-2xl hover:bg-white border border-transparent hover:border-emerald-100 transition-all group">
                  <div className="flex items-center gap-4">
                    <span className="text-xs font-serif font-black text-emerald-600 w-4">{i + 1}</span>
                    <p className="font-black text-gray-800 text-[11px] uppercase tracking-tight truncate">{prod.nombre}</p>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="text-sm font-black text-gray-900">{prod.totalCantidad}</span>
                    <span className="text-[8px] text-emerald-600 font-black uppercase">Ventas</span>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>

        {/* Bottom 5 Underperformers */}
        <div className="bg-white p-8 rounded-[2.5rem] shadow-sm border border-gray-100 animate-slide-up">
          <div className="flex justify-between items-center mb-6">
            <h3 className="text-xl font-serif font-black text-gray-900 tracking-tight">Baja Rotación</h3>
            <div className="p-2.5 bg-rose-50 text-rose-600 rounded-xl shadow-inner">
              <TrendingDown size={20} />
            </div>
          </div>
          <div className="space-y-3">
            {!data.topMenosVendidos || data.topMenosVendidos.length === 0 ? (
              <p className="text-[11px] text-gray-400 italic py-4">Inventario en movimiento saludable.</p>
            ) : (
              data.topMenosVendidos.slice(0, 5).map((prod: any, i: number) => (
                <div key={i} className="flex items-center justify-between p-3.5 bg-gray-50 rounded-2xl hover:bg-white border border-transparent hover:border-rose-100 transition-all group">
                  <div className="flex items-center gap-4">
                    <span className="text-xs font-serif font-black text-rose-600 w-4">{i + 1}</span>
                    <p className="font-black text-gray-800 text-[11px] uppercase tracking-tight truncate">{prod.nombre}</p>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className={`text-sm font-black ${prod.totalCantidad === 0 ? 'text-rose-600' : 'text-gray-900'}`}>{prod.totalCantidad}</span>
                    <span className="text-[8px] text-gray-400 font-black uppercase">Ventas</span>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      </div>



      {/* CLAIMS MANAGEMENT SECTION */}
      <div className="bg-white p-10 rounded-[3rem] shadow-sm border border-gray-100 animate-slide-up mt-16 max-w-4xl">
        <div className="flex justify-between items-center mb-8">
          <h3 className="text-2xl font-serif font-black text-gray-900 tracking-tight">Gestión de Reclamaciones</h3>
          <div className="p-3 bg-amber-50 text-amber-600 rounded-2xl shadow-inner">
            <AlertCircle size={20} />
          </div>
        </div>
        <div className="space-y-4">
          {reclamaciones.length === 0 ? (
            <p className="text-xs text-gray-400 font-bold uppercase tracking-widest text-center py-12 bg-gray-50/50 rounded-3xl border border-dashed border-gray-200">
              No hay registros disponibles en el periodo seleccionado.
            </p>
          ) : (
            reclamaciones.slice(0, 5).map((rec, idx) => (
              <div key={idx} className="flex items-center justify-between p-5 bg-gray-50 rounded-3xl hover:bg-white border border-transparent hover:border-amber-100 transition-all group shadow-sm hover:shadow-md">
                <div className="flex items-center gap-5">
                  <span className="text-[11px] font-black text-amber-600 font-serif">#{rec.pedidoId}</span>
                  <div className="flex flex-col">
                    <p className="font-black text-gray-900 text-[11px] uppercase tracking-tight truncate max-w-[140px]">{rec.motivo}</p>
                    <p className="text-[9px] text-gray-400 font-bold uppercase tracking-widest mt-0.5">Reporte: {new Date(rec.fecha).toLocaleDateString()}</p>
                  </div>
                </div>
                <span className={`px-4 py-2 rounded-full text-[9px] font-black uppercase tracking-widest border-2 transition-all ${rec.estado === 'Pendiente' ? 'bg-amber-50 text-amber-700 border-amber-200' :
                  rec.estado === 'EnRevision' ? 'bg-blue-50 text-blue-700 border-blue-200' :
                    'bg-emerald-50 text-emerald-700 border-emerald-200'
                  }`}>
                  {rec.estado}
                </span>
              </div>
            ))
          )}
        </div>
        {reclamaciones.length > 0 && (
          <button
            onClick={() => navigate('/admin/pedidos')}
            className="w-full mt-8 py-4 text-[10px] font-black uppercase tracking-[0.2em] text-gray-400 hover:text-amber-600 transition-colors flex items-center justify-center gap-2"
          >
            Acceder al Registro Completo <ArrowRight size={14} />
          </button>
        )}
      </div>
    </div>
  );
};

export default Dashboard;
