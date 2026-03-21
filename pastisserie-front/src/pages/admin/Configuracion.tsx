import React, { useState, useEffect } from 'react';
import { Save, Store, Clock, Shield, Globe, MessageSquare, Phone, MapPin, DollarSign, Instagram, Facebook, MessageCircle, AlertCircle } from 'lucide-react';
import api from '../../api/axios';
import { toast } from 'react-hot-toast';

interface StoreData {
    nombreTienda: string;
    direccion: string;
    telefono: string;
    emailContacto: string;
    costoEnvio: number;
    moneda: string;
    mensajeBienvenida: string;
    sistemaActivoManual: boolean;
    usarControlHorario: boolean;
    horaApertura: string;
    horaCierre: string;
    diasLaborales: string[];
    instagramUrl?: string;
    facebookUrl?: string;
    whatsappUrl?: string;
    horariosPorDia: HorarioDia[];
}

interface HorarioDia {
    id?: number;
    diaSemana: number; // 0=Domingo, 1=Lunes, 2=Martes...
    abierto: boolean;
    horaApertura: string;
    horaCierre: string;
}

const Configuracion: React.FC = () => {
    const [loading, setLoading] = useState(false);
    const [fetching, setFetching] = useState(true);

    const [storeData, setStoreData] = useState<StoreData>({
        nombreTienda: '',
        direccion: '',
        telefono: '',
        emailContacto: '',
        costoEnvio: 0,
        moneda: 'COP',
        mensajeBienvenida: '',
        sistemaActivoManual: true,
        usarControlHorario: true,
        horaApertura: '08:00',
        horaCierre: '18:00',
        diasLaborales: ['Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes'],
        instagramUrl: '',
        facebookUrl: '',
        whatsappUrl: '',
        horariosPorDia: [
            { diaSemana: 0, abierto: false, horaApertura: '08:00', horaCierre: '18:00' },
            { diaSemana: 1, abierto: true, horaApertura: '08:00', horaCierre: '18:00' },
            { diaSemana: 2, abierto: true, horaApertura: '08:00', horaCierre: '18:00' },
            { diaSemana: 3, abierto: true, horaApertura: '08:00', horaCierre: '18:00' },
            { diaSemana: 4, abierto: true, horaApertura: '08:00', horaCierre: '18:00' },
            { diaSemana: 5, abierto: true, horaApertura: '08:00', horaCierre: '18:00' },
            { diaSemana: 6, abierto: true, horaApertura: '08:00', horaCierre: '18:00' },
        ]
    });

    useEffect(() => {
        fetchStoreConfig();
    }, []);

    const fetchStoreConfig = async () => {
        try {
            setFetching(true);
            const response = await api.get('/configuracion');
            if (response.data?.data) {
                const config = response.data.data;
                const formatTime = (t: string) => t && t.length > 5 ? t.substring(0, 5) : (t || '08:00');

                let fetchedHorarios = config.horariosPorDia || [];
                // Initialize default array if backend returned empty
                if (fetchedHorarios.length === 0) {
                    fetchedHorarios = [0, 1, 2, 3, 4, 5, 6].map(d => ({
                        diaSemana: d,
                        abierto: config.diasLaborales?.includes(d.toString()) || false,
                        horaApertura: formatTime(config.horaApertura),
                        horaCierre: formatTime(config.horaCierre)
                    }));
                } else {
                    fetchedHorarios = fetchedHorarios.map((h: any) => ({
                        ...h,
                        horaApertura: formatTime(h.horaApertura),
                        horaCierre: formatTime(h.horaCierre)
                    }));
                }
                fetchedHorarios.sort((a: any, b: any) => a.diaSemana - b.diaSemana);

                setStoreData({
                    ...config,
                    // keep backward compatibility just in case
                    diasLaborales: typeof config.diasLaborales === 'string'
                        ? config.diasLaborales.split(',').filter((d: string) => d)
                        : ['1', '2', '3', '4', '5'],
                    horaApertura: formatTime(config.horaApertura),
                    horaCierre: formatTime(config.horaCierre),
                    instagramUrl: config.instagramUrl || '',
                    facebookUrl: config.facebookUrl || '',
                    whatsappUrl: config.whatsappUrl || '',
                    horariosPorDia: fetchedHorarios
                });
            }
        } catch (error) {
            console.error('Error al cargar configuración', error);
            toast.error('Error al sincronizar configuración base');
        } finally {
            setFetching(false);
        }
    };
    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const { name, value, type } = e.target;
        const finalValue = type === 'checkbox' ? (e.target as HTMLInputElement).checked : value;

        setStoreData(prev => ({
            ...prev,
            [name]: name === 'costoEnvio' ? (parseInt(value) || 0) : finalValue
        }));
    };

    const handleSubmit = async (e?: React.FormEvent) => {
        if (e) e.preventDefault();

        const saveToast = toast.loading('Sincronizando cambios con la nube...');

        try {
            setLoading(true);

            // Payload optimization: Ensure TimeSpan format (HH:mm:ss) and proper numbers
            const payload = {
                ...storeData,
                costoEnvio: Number(storeData.costoEnvio),
                horaApertura: storeData.horaApertura.length === 5 ? `${storeData.horaApertura}:00` : storeData.horaApertura,
                horaCierre: storeData.horaCierre.length === 5 ? `${storeData.horaCierre}:00` : storeData.horaCierre,
                diasLaborales: storeData.horariosPorDia.filter(h => h.abierto).map(h => h.diaSemana.toString()).join(','),
                horariosPorDia: storeData.horariosPorDia.map(h => ({
                    ...h,
                    horaApertura: h.horaApertura.length === 5 ? `${h.horaApertura}:00` : h.horaApertura,
                    horaCierre: h.horaCierre.length === 5 ? `${h.horaCierre}:00` : h.horaCierre,
                }))
            };

            await api.put('/configuracion', payload);
            toast.success('Configuración global actualizada con éxito', { id: saveToast });
            window.scrollTo({ top: 0, behavior: 'smooth' });
        } catch (error: any) {
            const errorMsg = error.response?.data?.message || 'Error crítico al intentar guardar';
            toast.error(errorMsg, { id: saveToast });
            console.error(error);
        } finally {
            setLoading(false);
        }
    };

    if (fetching) {
        return (
            <div className="flex items-center justify-center min-h-[60vh]">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#7D2121]"></div>
            </div>
        );
    }

    return (
        <div className="max-w-6xl mx-auto px-4 py-12 pb-32 animate-fade-in space-y-12">
            {/* VANGUARD HEADER */}
            <div className="flex flex-col md:flex-row md:items-end justify-between gap-8 border-b border-gray-200 pb-12">
                <div className="animate-slide-down">
                    <h1 className="text-5xl font-serif font-black text-gray-900 tracking-tighter mb-3">
                        Configuración <span className="text-[#7D2121]">Maestra</span>
                    </h1>
                    <p className="text-gray-600 font-bold max-w-xl">
                        Ajustes de alto nivel para el motor de Patisserie Deluxe. Cambios aplicados en tiempo real sobre el portal del cliente.
                    </p>
                </div>
                <div className="flex items-center gap-4 animate-slide-left">
                    <button
                        onClick={() => handleSubmit()}
                        disabled={loading}
                        className="bg-[#111] text-white px-10 py-5 rounded-[2rem] font-black uppercase tracking-[0.25em] text-[11px] hover:bg-[#7D2121] hover:shadow-2xl hover:shadow-[#7D2121]/20 transition-all flex items-center gap-4 disabled:bg-gray-400 active:scale-95 shadow-xl shadow-black/10"
                    >
                        {loading ? <div className="w-5 h-5 border-2 border-white/20 border-t-white rounded-full animate-spin"></div> : <Save size={20} strokeWidth={2.5} />}
                        {loading ? 'Procesando...' : 'Ejecutar Cambios'}
                    </button>
                </div>
            </div>

            <form onSubmit={handleSubmit} className="space-y-10">
                {/* SECTION 1: IDENTITY */}
                <div className="bg-white rounded-[3.5rem] p-12 shadow-[0_20px_60px_rgba(0,0,0,0.05)] border border-gray-200 relative overflow-hidden group hover:shadow-[0_40px_80px_rgba(0,0,0,0.08)] transition-all duration-700">
                    <div className="absolute top-0 right-0 w-48 h-48 bg-gray-50 rounded-bl-[6rem] -mr-12 -mt-12 transition-all group-hover:bg-[#7D2121]/5"></div>

                    <div className="flex items-center gap-6 mb-12 relative z-10">
                        <div className="w-16 h-16 bg-[#7D2121]/10 rounded-[2rem] flex items-center justify-center text-[#7D2121] shadow-inner transition-transform group-hover:rotate-6">
                            <Store size={32} strokeWidth={2.5} />
                        </div>
                        <div>
                            <h2 className="text-2xl font-serif font-black text-gray-900 tracking-tight text-patisserie-red italic">Identidad de Marca</h2>
                            <p className="text-gray-600 text-[11px] font-black uppercase tracking-widest mt-1 opacity-80">Configuración global y presencia pública</p>
                        </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-10 relative z-10">
                        <div className="space-y-3">
                            <label className="text-[11px] font-black uppercase tracking-[0.25em] text-gray-600 ml-2 flex items-center gap-2 italic">
                                <Globe size={13} className="text-[#7D2121]" /> Razón Social
                            </label>
                            <input
                                type="text"
                                name="nombreTienda"
                                value={storeData.nombreTienda}
                                onChange={handleChange}
                                placeholder="Ej. Patisserie Deluxe"
                                className="w-full px-8 py-5 bg-gray-50 border-gray-200 border-2 rounded-[2rem] focus:bg-white focus:border-[#7D2121] focus:ring-0 transition-all font-black text-gray-900 placeholder:text-gray-300"
                            />
                        </div>
                        <div className="space-y-3">
                            <label className="text-[11px] font-black uppercase tracking-[0.25em] text-gray-600 ml-2 flex items-center gap-2 italic">
                                <MessageSquare size={13} className="text-[#7D2121]" /> Canal de Soporte (Email)
                            </label>
                            <input
                                type="email"
                                name="emailContacto"
                                value={storeData.emailContacto}
                                onChange={handleChange}
                                placeholder="hola@patisserie.com"
                                className="w-full px-8 py-5 bg-gray-50 border-gray-200 border-2 rounded-[2rem] focus:bg-white focus:border-[#7D2121] focus:ring-0 transition-all font-black text-gray-900 placeholder:text-gray-300"
                            />
                        </div>
                        <div className="space-y-3">
                            <label className="text-[11px] font-black uppercase tracking-[0.25em] text-gray-600 ml-2 flex items-center gap-2 italic">
                                <MapPin size={13} className="text-[#7D2121]" /> Ubicación Headquarter
                            </label>
                            <input
                                type="text"
                                name="direccion"
                                value={storeData.direccion}
                                onChange={handleChange}
                                placeholder="Calle Gourmet #123"
                                className="w-full px-8 py-5 bg-gray-50 border-gray-200 border-2 rounded-[2rem] focus:bg-white focus:border-[#7D2121] focus:ring-0 transition-all font-black text-gray-900 placeholder:text-gray-300"
                            />
                        </div>
                        <div className="space-y-3">
                            <label className="text-[11px] font-black uppercase tracking-[0.25em] text-gray-600 ml-2 flex items-center gap-2 italic">
                                <Phone size={13} className="text-[#7D2121]" /> Línea de Atención
                            </label>
                            <input
                                type="text"
                                name="telefono"
                                value={storeData.telefono}
                                onChange={handleChange}
                                placeholder="+57 300 000 0000"
                                className="w-full px-8 py-5 bg-gray-50 border-gray-200 border-2 rounded-[2rem] focus:bg-white focus:border-[#7D2121] focus:ring-0 transition-all font-black text-gray-900 placeholder:text-gray-300"
                            />
                        </div>
                        <div className="md:col-span-2 space-y-3">
                            <label className="text-[11px] font-black uppercase tracking-[0.25em] text-gray-600 ml-2 flex items-center gap-2 italic">
                                <Shield size={13} className="text-[#7D2121]" /> Proclama / Mensaje de Bienvenida
                            </label>
                            <textarea
                                name="mensajeBienvenida"
                                value={storeData.mensajeBienvenida}
                                onChange={handleChange}
                                rows={3}
                                className="w-full px-8 py-6 bg-gray-50 border-gray-200 border-2 rounded-[2.5rem] focus:bg-white focus:border-[#7D2121] focus:ring-0 transition-all font-black text-gray-900 resize-none leading-relaxed"
                            />
                        </div>
                    </div>
                </div>

                {/* SECTION 2: OPERATIONAL LOGIC */}
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-10">
                    <div className="bg-white rounded-[3.5rem] p-12 shadow-[0_20px_60px_rgba(0,0,0,0.05)] border border-gray-200 flex flex-col group transition-all duration-700 hover:shadow-[0_40px_80px_rgba(0,0,0,0.08)]">
                        <div className="flex items-center gap-6 mb-12">
                            <div className="w-16 h-16 bg-emerald-100/50 rounded-[2rem] flex items-center justify-center text-emerald-700 shadow-inner group-hover:scale-110 transition-transform">
                                <Clock size={32} strokeWidth={2.5} />
                            </div>
                            <div>
                                <h2 className="text-2xl font-serif font-black text-gray-900 tracking-tight text-emerald-800 italic">Horario Operativo</h2>
                                <p className="text-gray-600 text-[11px] font-black uppercase tracking-widest mt-1 opacity-80">Control de apertura automática</p>
                            </div>
                        </div>

                        <div className="flex items-center justify-between p-8 bg-emerald-50/50 rounded-[2.5rem] mb-10 border border-emerald-100 transition-all hover:bg-white hover:shadow-xl">
                            <div>
                                <p className="font-black text-emerald-900 text-sm uppercase tracking-tighter">Validación Inteligente</p>
                                <p className="text-[10px] text-emerald-600 font-black uppercase tracking-[0.15em] mt-1">Hora Legal: Medellín (CO)</p>
                            </div>
                            <label className="relative inline-flex items-center cursor-pointer">
                                <input
                                    type="checkbox"
                                    name="usarControlHorario"
                                    checked={storeData.usarControlHorario}
                                    onChange={(e) => {
                                        setStoreData(prev => ({ ...prev, usarControlHorario: e.target.checked }));
                                    }}
                                    className="sr-only peer"
                                />
                                <div className="w-16 h-8 bg-gray-300 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[4px] after:left-[4px] after:bg-white after:border-gray-400 after:border after:rounded-full after:h-6 after:w-6 after:transition-all peer-checked:bg-emerald-600 shadow-inner"></div>
                            </label>
                        </div>

                        {storeData.usarControlHorario && (
                            <div className="space-y-6 animate-slide-up">
                                <div className="space-y-4">
                                    <label className="text-[10px] font-black uppercase tracking-[0.2em] text-gray-700 ml-1 italic opacity-70">
                                        Horarios de Atención Semanal
                                    </label>

                                    {/* Table Header */}
                                    <div className="grid grid-cols-[1fr_48px_100px_16px_100px] gap-2 px-3 pb-1 border-b border-gray-100">
                                        <span className="text-[9px] font-black uppercase tracking-widest text-gray-400">Día</span>
                                        <span className="text-[9px] font-black uppercase tracking-widest text-gray-400 text-center">Abre</span>
                                        <span className="text-[9px] font-black uppercase tracking-widest text-gray-400 text-center">Apertura</span>
                                        <span></span>
                                        <span className="text-[9px] font-black uppercase tracking-widest text-gray-400 text-center">Cierre</span>
                                    </div>

                                    {/* Day rows */}
                                    <div className="flex flex-col gap-2">
                                        {[
                                            { nombre: 'Dom', val: 0 },
                                            { nombre: 'Lun', val: 1 },
                                            { nombre: 'Mar', val: 2 },
                                            { nombre: 'Mié', val: 3 },
                                            { nombre: 'Jue', val: 4 },
                                            { nombre: 'Vie', val: 5 },
                                            { nombre: 'Sáb', val: 6 }
                                        ].map(dia => {
                                            const horario = storeData.horariosPorDia.find(h => h.diaSemana === dia.val) || { diaSemana: dia.val, abierto: false, horaApertura: '08:00', horaCierre: '18:00' };

                                            return (
                                                <div key={dia.val} className={`grid grid-cols-[1fr_48px_100px_16px_100px] gap-2 items-center px-3 py-2 rounded-xl transition-all ${horario.abierto ? 'bg-emerald-50/60 border border-emerald-100' : 'bg-gray-50 border border-gray-100 opacity-50 hover:opacity-80'
                                                    }`}>
                                                    <span className={`text-xs font-black uppercase tracking-wider ${horario.abierto ? 'text-gray-800' : 'text-gray-400'}`}>{dia.nombre}</span>

                                                    <div className="flex justify-center">
                                                        <input
                                                            type="checkbox"
                                                            checked={horario.abierto}
                                                            onChange={(e) => {
                                                                const newArr = [...storeData.horariosPorDia];
                                                                const idx = newArr.findIndex(h => h.diaSemana === dia.val);
                                                                if (idx > -1) newArr[idx] = { ...newArr[idx], abierto: e.target.checked };
                                                                setStoreData(prev => ({ ...prev, horariosPorDia: newArr }));
                                                            }}
                                                            className="w-4 h-4 rounded border-gray-300 text-emerald-600 focus:ring-emerald-600 cursor-pointer"
                                                        />
                                                    </div>

                                                    <input
                                                        type="time"
                                                        value={horario.horaApertura}
                                                        disabled={!horario.abierto}
                                                        onChange={(e) => {
                                                            const newArr = [...storeData.horariosPorDia];
                                                            const idx = newArr.findIndex(h => h.diaSemana === dia.val);
                                                            if (idx > -1) newArr[idx] = { ...newArr[idx], horaApertura: e.target.value };
                                                            setStoreData(prev => ({ ...prev, horariosPorDia: newArr }));
                                                        }}
                                                        className="w-full px-2 py-1.5 text-xs font-black text-gray-700 bg-white rounded-xl border border-gray-200 focus:ring-0 focus:border-emerald-400 disabled:opacity-30 disabled:cursor-not-allowed"
                                                    />

                                                    <span className="text-[10px] font-black text-gray-300 text-center">→</span>

                                                    <input
                                                        type="time"
                                                        value={horario.horaCierre}
                                                        disabled={!horario.abierto}
                                                        onChange={(e) => {
                                                            const newArr = [...storeData.horariosPorDia];
                                                            const idx = newArr.findIndex(h => h.diaSemana === dia.val);
                                                            if (idx > -1) newArr[idx] = { ...newArr[idx], horaCierre: e.target.value };
                                                            setStoreData(prev => ({ ...prev, horariosPorDia: newArr }));
                                                        }}
                                                        className="w-full px-2 py-1.5 text-xs font-black text-gray-700 bg-white rounded-xl border border-gray-200 focus:ring-0 focus:border-emerald-400 disabled:opacity-30 disabled:cursor-not-allowed"
                                                    />
                                                </div>
                                            );
                                        })}
                                    </div>
                                </div>
                            </div>
                        )}
                        {!storeData.usarControlHorario && (
                            <div className="flex-1 flex items-center justify-center p-10 bg-amber-50 rounded-[2.5rem] border-2 border-dashed border-amber-300 animate-slide-up">
                                <div className="text-center">
                                    <AlertCircle size={32} className="mx-auto text-amber-600 mb-4" />
                                    <p className="text-amber-900 text-[11px] font-black text-center uppercase tracking-widest leading-relaxed max-w-[250px] mx-auto">
                                        Sistema manual activo. La tienda ignorará el reloj del servidor para ventas 24/7.
                                    </p>
                                </div>
                            </div>
                        )}
                    </div>

                    <div className="bg-[#0a0a0a] rounded-[3.5rem] p-12 shadow-[0_40px_100px_rgba(0,0,0,0.25)] flex flex-col relative overflow-hidden group transition-all duration-700">
                        <div className="absolute top-0 right-0 w-80 h-80 bg-white/10 rounded-full -mr-40 -mt-40 transition-transform group-hover:scale-110"></div>

                        <div className="flex items-center gap-6 mb-12 relative z-10">
                            <div className="w-16 h-16 bg-white/20 rounded-[2rem] flex items-center justify-center text-white shadow-xl">
                                <Shield size={32} strokeWidth={3} />
                            </div>
                            <div>
                                <h2 className="text-2xl font-serif font-black text-white tracking-tight">Infinity Switch</h2>
                                <p className="text-white/60 text-[11px] font-black uppercase tracking-widest mt-1">Cortafuegos operacional</p>
                            </div>
                        </div>

                        <div className="flex-1 flex flex-col justify-center items-center text-center space-y-10 relative z-10">
                            <div className={`p-10 rounded-[3rem] border-2 transition-all duration-500 w-full ${storeData.sistemaActivoManual ? 'bg-emerald-500/10 border-emerald-500/40 shadow-[0_0_50px_rgba(16,185,129,0.2)]' : 'bg-rose-500/10 border-rose-500/40 shadow-[0_0_50px_rgba(244,63,94,0.2)]'}`}>
                                <p className={`text-4xl font-serif font-black mb-3 italic tracking-tighter transition-colors ${storeData.sistemaActivoManual ? 'text-emerald-400' : 'text-rose-400'}`}>
                                    {storeData.sistemaActivoManual ? 'Vanguard Core Active' : 'Emergency Lockdown'}
                                </p>
                                <p className="text-white/70 text-[12px] font-black uppercase tracking-[0.4em]">Estado Vital del Sistema</p>
                            </div>

                            <label className="relative inline-flex items-center cursor-pointer scale-[1.75]">
                                <input
                                    type="checkbox"
                                    name="sistemaActivoManual"
                                    checked={storeData.sistemaActivoManual}
                                    onChange={(e) => {
                                        setStoreData(prev => ({ ...prev, sistemaActivoManual: e.target.checked }));
                                    }}
                                    className="sr-only peer"
                                />
                                <div className="w-20 h-10 bg-white/20 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[4px] after:left-[4px] after:bg-white after:rounded-full after:h-8 after:w-8 after:transition-all peer-checked:bg-emerald-500 shadow-2xl"></div>
                            </label>

                            <p className="text-white/60 text-[10px] font-black max-w-[320px] leading-relaxed uppercase tracking-[0.2em] italic">
                                {storeData.sistemaActivoManual
                                    ? 'Permisos de checkout normales según configuración previa.'
                                    : 'ADVERTENCIA: Ninguna transacción es permitida mientras este nodo esté inactivo.'}
                            </p>
                        </div>
                    </div>
                </div>

                {/* SECTION 3: LOGISTICS AND SOCIAL */}
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-10">
                    <div className="bg-white rounded-[3.5rem] p-12 shadow-[0_20px_60px_rgba(0,0,0,0.05)] border border-gray-200 group transition-all duration-700 hover:shadow-[0_40px_80px_rgba(0,0,0,0.08)]">
                        <div className="flex items-center gap-6 mb-12">
                            <div className="w-16 h-16 bg-amber-100/50 rounded-[2rem] flex items-center justify-center text-amber-700 shadow-inner group-hover:-rotate-6 transition-transform">
                                <DollarSign size={32} strokeWidth={2.5} />
                            </div>
                            <div>
                                <h2 className="text-2xl font-serif font-black text-gray-900 tracking-tight text-amber-900 italic">Logística Base</h2>
                                <p className="text-gray-600 text-[11px] font-black uppercase tracking-widest mt-1 opacity-80">Costos de operación y despacho</p>
                            </div>
                        </div>
                        <div className="space-y-10">
                            <div className="space-y-4">
                                <label className="text-[10px] font-black uppercase tracking-[0.2em] text-gray-700 ml-4 italic opacity-70">Tarifa de Despacho ({storeData.moneda})</label>
                                <div className="relative group">
                                    <div className="absolute left-6 top-1/2 -translate-y-1/2 text-amber-600 font-outfit font-black text-xl">$</div>
                                    <input
                                        type="number"
                                        name="costoEnvio"
                                        value={storeData.costoEnvio}
                                        onChange={handleChange}
                                        className="w-full pl-12 pr-6 py-4 bg-amber-50/20 border-2 border-amber-100 rounded-[1.5rem] focus:bg-white focus:border-amber-500 focus:ring-4 focus:ring-amber-500/5 transition-all font-outfit font-black text-gray-900 text-2xl shadow-inner"
                                    />
                                </div>
                            </div>
                            <div className="p-8 bg-amber-100/30 rounded-[2.5rem] border border-amber-200/50 italic">
                                <p className="text-[11px] text-amber-900 font-bold uppercase tracking-[0.2em] leading-relaxed">
                                    "La automatización logística aplicará este cargo unitario a cada transacción validada en el checkout."
                                </p>
                            </div>
                        </div>
                    </div>

                    <div className="bg-white rounded-[3.5rem] p-12 shadow-[0_20px_60px_rgba(0,0,0,0.05)] border border-gray-200 group transition-all duration-700 hover:shadow-[0_40px_80px_rgba(0,0,0,0.08)]">
                        <div className="flex items-center gap-6 mb-12">
                            <div className="w-16 h-16 bg-rose-100/50 rounded-[2rem] flex items-center justify-center text-rose-700 shadow-inner group-hover:rotate-6 transition-transform">
                                <Instagram size={32} strokeWidth={2.5} />
                            </div>
                            <div>
                                <h2 className="text-2xl font-serif font-black text-gray-900 tracking-tight text-rose-900">Nexos Sociales</h2>
                                <p className="text-gray-500 text-[11px] font-black uppercase tracking-widest mt-1">Conectores de ecosistema</p>
                            </div>
                        </div>
                        <div className="space-y-6">
                            {[
                                { name: 'instagramUrl', icon: <Instagram size={18} />, color: 'text-rose-600', bg: 'bg-rose-100', placeholder: 'Instagram Business Handle' },
                                { name: 'facebookUrl', icon: <Facebook size={18} />, color: 'text-blue-700', bg: 'bg-blue-100', placeholder: 'Facebook Official Page' },
                                { name: 'whatsappUrl', icon: <MessageCircle size={18} />, color: 'text-emerald-700', bg: 'bg-emerald-100', placeholder: 'Direct WhatsApp API Line' }
                            ].map((social, i) => (
                                <div key={i} className="relative group/input">
                                    <div className={`absolute left-6 top-1/2 -translate-y-1/2 p-2.5 ${social.bg} ${social.color} rounded-xl transition-all group-focus-within/input:scale-110 shadow-sm`}>
                                        {social.icon}
                                    </div>
                                    <input
                                        type="text"
                                        name={social.name}
                                        value={(storeData as any)[social.name] || ''}
                                        onChange={handleChange}
                                        placeholder={social.placeholder}
                                        className="w-full pl-20 pr-8 py-5 bg-gray-50 border-2 border-gray-100 rounded-[2rem] focus:bg-white focus:border-patisserie-red/20 transition-all font-black text-gray-800 text-sm"
                                    />
                                </div>
                            ))}
                        </div>
                    </div>
                </div>

                {/* FINAL DEPLOY ACTION */}
                <div className="pt-10 flex flex-col items-center animate-slide-up" style={{ animationDelay: '500ms' }}>
                    <div className="w-full bg-white rounded-[4rem] px-16 py-20 border-3 border-gray-200 text-center flex flex-col items-center gap-8 group hover:border-patisserie-red/20 transition-all duration-500 shadow-xl">
                        <div className="max-w-md">
                            <h3 className="text-4xl font-serif font-black text-gray-900 tracking-tighter mb-4 italic">Commit Config Changes</h3>
                            <p className="text-[12px] text-gray-600 font-black uppercase tracking-[0.25em] leading-relaxed">
                                Antes de pulsar, valide que los umbrales operativos y los nexos sociales sean correctos. La actualización es atómica e inmediata.
                            </p>
                        </div>
                        <button
                            type="submit"
                            disabled={loading}
                            className="bg-[#111] text-white px-16 py-7 rounded-full font-black uppercase tracking-[0.4em] text-xs hover:bg-[#7D2121] hover:shadow-[0_20px_50px_rgba(125,33,33,0.3)] transition-all active:scale-95 disabled:bg-gray-200 flex items-center gap-6 shadow-2xl"
                        >
                            {loading ? (
                                <div className="w-6 h-6 border-4 border-white/20 border-t-white rounded-full animate-spin"></div>
                            ) : (
                                <Save size={24} strokeWidth={2.5} />
                            )}
                            {loading ? 'Confirmando...' : 'Aplicar Ajustes Globales'}
                        </button>
                    </div>
                </div>
            </form>
        </div>
    );
};

export default Configuracion;
