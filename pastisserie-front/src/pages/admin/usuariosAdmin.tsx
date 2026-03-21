import { useEffect, useState } from 'react';
import api from '../../api/axios';
import {
    Search, User, Mail, Shield, Calendar,
    X, Filter, UserPlus,
    ShieldAlert
} from 'lucide-react';
import toast from 'react-hot-toast';

interface User {
    id: number;
    nombre: string;
    email: string;
    roles: string[];
    fechaRegistro: string;
    activo: boolean;
}

const UsuariosAdmin = () => {
    const [usuarios, setUsuarios] = useState<User[]>([]);
    const [loading, setLoading] = useState(true);
    const [busqueda, setBusqueda] = useState('');
    const [filtroRol, setFiltroRol] = useState('Todos');

    useEffect(() => {
        fetchUsuarios();
    }, []);

    const fetchUsuarios = async () => {
        try {
            const response = await api.get('/users');
            let data = [];
            if (Array.isArray(response.data)) data = response.data;
            else if (response.data?.data && Array.isArray(response.data.data)) data = response.data.data;

            setUsuarios(data);
        } catch (error) {
            console.error(error);
            toast.error('Error al cargar usuarios');
        } finally {
            setLoading(false);
        }
    };

    const query = busqueda.toLowerCase().trim().replace('#', '');
    const usuariosFiltrados = usuarios.filter(u => {
        // Filtro por Rol
        const rolMatch = filtroRol === 'Todos' || u.roles?.includes(filtroRol);

        // Filtro por Búsqueda
        if (!query) return rolMatch;
        const nombreMatch = u.nombre?.toLowerCase().includes(query);
        const emailMatch = u.email?.toLowerCase().includes(query);
        const idMatch = u.id.toString().includes(query);
        const searchRoleMatch = u.roles?.some(r => r.toLowerCase().includes(query));

        return rolMatch && (nombreMatch || emailMatch || idMatch || searchRoleMatch);
    });

    const toggleUserStatus = async (id: number, currentStatus: boolean) => {
        if (!window.confirm(`¿Estás seguro de que deseas ${currentStatus ? 'desactivar' : 'activar'} este usuario?`)) return;

        try {
            await api.patch(`/users/${id}/status`, !currentStatus, {
                headers: { 'Content-Type': 'application/json' }
            });
            toast.success(`Usuario ${currentStatus ? 'desactivado' : 'activado'} correctamente`);
            fetchUsuarios();
        } catch (error: any) {
            console.error(error);
            toast.error(error.response?.data?.message || 'Error al actualizar estado');
        }
    };

    // Estados para Modales
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [newUser, setNewUser] = useState({ nombre: '', email: '', password: '', confirmPassword: '', rol: 'Usuario' });

    // Estado para cambio de rol
    const [editingRoleUser, setEditingRoleUser] = useState<User | null>(null);
    const [newRole, setNewRole] = useState('');

    const handleCreateUser = async (e: React.FormEvent) => {
        e.preventDefault();

        // Validación Manual
        if (!newUser.nombre.trim() || !newUser.email.trim() || !newUser.password || !newUser.confirmPassword) {
            toast.error('Todos los campos son obligatorios para crear un usuario');
            return;
        }

        if (newUser.password !== newUser.confirmPassword) {
            toast.error('Las contraseñas no coinciden');
            return;
        }

        try {
            await api.post('/users', newUser);
            toast.success('Usuario creado exitosamente');
            setShowCreateModal(false);
            setNewUser({ nombre: '', email: '', password: '', confirmPassword: '', rol: 'Usuario' });
            fetchUsuarios();
        } catch (error: any) {
            console.error(error);
            toast.error(error.response?.data?.message || 'Error al crear usuario');
        }
    };

    const handleChangeRole = async () => {
        if (!editingRoleUser || !newRole) return;
        try {
            await api.patch(`/users/${editingRoleUser.id}/role`, { NuevoRol: newRole });
            toast.success('Rol actualizado correctamente');
            setEditingRoleUser(null);
            fetchUsuarios();
        } catch (error: any) {
            console.error(error);
            toast.error(error.response?.data?.message || 'Error al cambiar rol');
        }
    };

    return (
        <div className="animate-fade-in p-2 pb-20">
            {/* Cabecera Premium */}
            <div className="flex flex-col md:flex-row justify-between items-center mb-10 gap-6">
                <div>
                    <h1 className="text-4xl font-serif font-black text-[#5D1919] tracking-tighter mb-2">Directorio de Maestros</h1>
                    <p className="text-gray-500 font-medium">Gestión de privilegios, perfiles y trazabilidad de acceso</p>
                </div>
                <button
                    onClick={() => setShowCreateModal(true)}
                    className="bg-[#5D1919] text-white px-8 py-4 rounded-2xl flex items-center gap-3 hover:bg-[#7D2121] shadow-lg shadow-[#5D1919]/20 transition-all active:scale-95 text-[10px] font-black uppercase tracking-widest"
                >
                    <UserPlus size={18} strokeWidth={3} /> Registrar Nuevo Maestro
                </button>
            </div>

            {/* Filtros y Búsqueda Premium */}
            <div className="bg-white rounded-[2rem] p-6 mb-8 shadow-sm border border-gray-100 flex flex-col md:flex-row items-center gap-6">
                <div className="flex items-center gap-4 bg-gray-50 border border-gray-100 rounded-2xl px-6 py-4 w-full md:w-auto flex-1 group focus-within:ring-4 focus-within:ring-[#5D1919]/5 transition-all">
                    <Search className="text-gray-300 group-focus-within:text-[#5D1919] transition-colors" size={18} />
                    <input
                        type="text"
                        placeholder="Buscar por nombre, email o identificador..."
                        className="bg-transparent outline-none text-sm w-full font-bold text-gray-700 placeholder:text-gray-300"
                        value={busqueda}
                        onChange={(e) => setBusqueda(e.target.value)}
                    />
                </div>
                <div className="flex items-center gap-4 w-full md:w-auto">
                    <div className="p-3 bg-gray-50 rounded-xl text-gray-400">
                        <Filter size={16} />
                    </div>
                    <select
                        className="bg-gray-50 border border-gray-100 rounded-2xl px-6 py-4 text-[10px] font-black uppercase tracking-widest text-[#5D1919] outline-none focus:ring-4 focus:ring-[#5D1919]/5 transition-all w-full md:w-56 appearance-none cursor-pointer"
                        value={filtroRol}
                        onChange={(e) => setFiltroRol(e.target.value)}
                    >
                        <option value="Todos">Todos los Rangos</option>
                        <option value="Admin">Administradores</option>
                        <option value="Usuario">Usuarios Finales</option>
                        <option value="Repartidor">Logística / Reparto</option>
                    </select>
                </div>
            </div>

            {/* Tabla Premium */}
            <div className="bg-white rounded-[2.5rem] shadow-[0_10px_40px_rgba(0,0,0,0.03)] border border-gray-100 overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="w-full text-left text-sm text-gray-600">
                        <thead className="bg-[#fcfcfc] border-b border-gray-50">
                            <tr>
                                <th className="px-10 py-6 text-[10px] font-black uppercase tracking-widest text-gray-400">Perfil del Maestro</th>
                                <th className="px-8 py-6 text-[10px] font-black uppercase tracking-widest text-gray-400">Rango / Acceso</th>
                                <th className="px-8 py-6 text-[10px] font-black uppercase tracking-widest text-gray-400">Estado Vital</th>
                                <th className="px-8 py-6 text-[10px] font-black uppercase tracking-widest text-gray-400">Fecha de Alta</th>
                                <th className="px-8 py-6 text-[10px] font-black uppercase tracking-widest text-gray-400">Seguridad</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-100">
                            {loading ? (
                                <tr><td colSpan={5} className="text-center py-8">Cargando usuarios...</td></tr>
                            ) : usuariosFiltrados.length === 0 ? (
                                <tr><td colSpan={5} className="text-center py-8">No se encontraron usuarios.</td></tr>
                            ) : usuariosFiltrados.map((user) => (
                                <tr key={user.id} className="hover:bg-[#5D1919]/[0.02] transition-all group">
                                    <td className="px-10 py-6">
                                        <div className="flex items-center gap-4">
                                            <div className="w-12 h-12 rounded-2xl bg-[#5D1919] text-white flex items-center justify-center font-serif font-black text-lg shadow-lg shadow-[#5D1919]/20 relative overflow-hidden group-hover:scale-105 transition-transform">
                                                <div className="absolute inset-0 bg-white/10 opacity-0 group-hover:opacity-100 transition-opacity"></div>
                                                {user.nombre?.substring(0, 2).toUpperCase() || <User size={20} />}
                                            </div>
                                            <div>
                                                <p className="font-serif font-black text-gray-900 group-hover:text-[#5D1919] transition-colors">{user.nombre}</p>
                                                <div className="flex items-center gap-2 text-[10px] font-bold text-gray-400 uppercase tracking-widest mt-1">
                                                    <Mail size={10} /> {user.email}
                                                </div>
                                            </div>
                                        </div>
                                    </td>
                                    <td className="px-8 py-6">
                                        <div className="flex gap-2 flex-wrap">
                                            {user.roles && user.roles.map((rol, idx) => (
                                                <span
                                                    key={idx}
                                                    className={`px-3 py-1.5 rounded-xl text-[10px] font-black uppercase tracking-widest flex items-center gap-2 border cursor-pointer hover:shadow-md transition-all
                                                    ${rol === 'Admin' ? 'bg-[#5D1919]/5 text-[#5D1919] border-[#5D1919]/10' : 'bg-gray-50 text-gray-500 border-gray-100'}`}
                                                    onClick={() => { setEditingRoleUser(user); setNewRole(rol); }}
                                                    title="Modificar Privilegios"
                                                >
                                                    <Shield size={12} strokeWidth={3} /> {rol}
                                                </span>
                                            ))}
                                            {(!user.roles || user.roles.length === 0) && (
                                                <span
                                                    className="px-3 py-1.5 rounded-xl text-[10px] font-black uppercase tracking-widest bg-rose-50 text-rose-600 border border-rose-100 cursor-pointer"
                                                    onClick={() => { setEditingRoleUser(user); setNewRole('Usuario'); }}
                                                >
                                                    <ShieldAlert size={12} /> Sin Rango
                                                </span>
                                            )}
                                        </div>
                                    </td>
                                    <td className="px-8 py-6">
                                        <div className="flex items-center gap-2">
                                            <div className={`w-2 h-2 rounded-full ${user.activo ? 'bg-emerald-500 animate-pulse' : 'bg-rose-500'}`}></div>
                                            <span className={`text-[10px] font-black uppercase tracking-widest ${user.activo ? 'text-emerald-700' : 'text-rose-700'}`}>
                                                {user.activo ? 'Operativo' : 'Restringido'}
                                            </span>
                                        </div>
                                    </td>
                                    <td className="px-8 py-6">
                                        <div className="flex items-center gap-3 text-gray-500 font-bold text-xs">
                                            <Calendar size={14} className="text-gray-300" />
                                            {new Date(user.fechaRegistro).toLocaleDateString(undefined, { day: '2-digit', month: 'short', year: 'numeric' })}
                                        </div>
                                    </td>
                                    <td className="px-8 py-6">
                                        <div className="flex gap-2">
                                            <button
                                                onClick={() => toggleUserStatus(user.id, user.activo)}
                                                className={`p-2.5 rounded-xl transition-all ${user.activo ? 'bg-rose-50 text-rose-600 hover:bg-rose-100 hover:rotate-12' : 'bg-emerald-50 text-emerald-600 hover:bg-emerald-100'}`}
                                                title={user.activo ? "Revocar Acceso" : "Restaurar Acceso"}
                                            >
                                                <ShieldAlert size={18} strokeWidth={2.5} />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </div>

            {/* Modal Crear Usuario Premium */}
            {showCreateModal && (
                <div className="fixed inset-0 bg-[#5D1919]/20 flex items-center justify-center z-50 p-4 backdrop-blur-md animate-fade-in">
                    <div className="bg-white rounded-[2.5rem] shadow-[0_20px_60px_rgba(0,0,0,0.3)] w-full max-w-md overflow-hidden border border-[#5D1919]/10">
                        <div className="bg-[#5D1919] text-white px-10 py-8 relative overflow-hidden">
                            <div className="absolute top-0 right-0 w-32 h-32 bg-white/5 rounded-bl-[5rem] -mr-10 -mt-10"></div>
                            <h2 className="text-2xl font-serif font-black tracking-tight relative z-10">Nuevo Registro</h2>
                            <p className="text-white/60 text-[10px] font-black uppercase tracking-widest mt-1 relative z-10">Alta de personal administrativo</p>
                        </div>
                        <form onSubmit={handleCreateUser} className="p-10 space-y-6 bg-[#fcfcfc]">
                            <div className="space-y-4">
                                <div className="space-y-2">
                                    <label className="text-[9px] font-black text-gray-400 uppercase tracking-widest ml-1">Nombre Completo</label>
                                    <input
                                        type="text" placeholder="Ej: Juan Pérez"
                                        className="w-full bg-white border border-gray-100 rounded-2xl px-6 py-4 text-sm font-bold focus:ring-4 focus:ring-[#5D1919]/5 outline-none transition-all shadow-sm"
                                        value={newUser.nombre} onChange={e => setNewUser({ ...newUser, nombre: e.target.value })}
                                    />
                                </div>
                                <div className="space-y-2">
                                    <label className="text-[9px] font-black text-gray-400 uppercase tracking-widest ml-1">Email Institucional</label>
                                    <input
                                        type="email" placeholder="maestro@patisserie.com"
                                        className="w-full bg-white border border-gray-100 rounded-2xl px-6 py-4 text-sm font-bold focus:ring-4 focus:ring-[#5D1919]/5 outline-none transition-all shadow-sm"
                                        value={newUser.email} onChange={e => setNewUser({ ...newUser, email: e.target.value })}
                                    />
                                </div>
                                <div className="grid grid-cols-2 gap-4">
                                    <div className="space-y-2">
                                        <label className="text-[9px] font-black text-gray-400 uppercase tracking-widest ml-1">Credencial</label>
                                        <input
                                            type="password" placeholder="••••••••"
                                            className="w-full bg-white border border-gray-100 rounded-2xl px-6 py-4 text-sm font-bold focus:ring-4 focus:ring-[#5D1919]/5 outline-none transition-all shadow-sm"
                                            value={newUser.password} onChange={e => setNewUser({ ...newUser, password: e.target.value })}
                                        />
                                    </div>
                                    <div className="space-y-2">
                                        <label className="text-[9px] font-black text-gray-400 uppercase tracking-widest ml-1">Confirmar</label>
                                        <input
                                            type="password" placeholder="••••••••"
                                            className="w-full bg-white border border-gray-100 rounded-2xl px-6 py-4 text-sm font-bold focus:ring-4 focus:ring-[#5D1919]/5 outline-none transition-all shadow-sm"
                                            value={newUser.confirmPassword} onChange={e => setNewUser({ ...newUser, confirmPassword: e.target.value })}
                                        />
                                    </div>
                                </div>
                                <div className="space-y-2">
                                    <label className="text-[9px] font-black text-gray-400 uppercase tracking-widest ml-1">Nivel de Acceso</label>
                                    <select
                                        className="w-full bg-white border border-gray-100 rounded-2xl px-6 py-4 text-sm font-bold focus:ring-4 focus:ring-[#5D1919]/5 outline-none transition-all shadow-sm appearance-none cursor-pointer"
                                        value={newUser.rol} onChange={e => setNewUser({ ...newUser, rol: e.target.value })}
                                    >
                                        <option value="Usuario">Usuario Estándar</option>
                                        <option value="Admin">Administrador Maestro</option>
                                        <option value="Repartidor">Logística de Entrega</option>
                                    </select>
                                </div>
                            </div>

                            <div className="flex justify-end gap-4 pt-6">
                                <button type="button" onClick={() => setShowCreateModal(false)} className="px-6 py-3 text-[10px] font-black uppercase tracking-widest text-gray-400 hover:text-gray-600 transition-all">Cancelar</button>
                                <button type="submit" className="bg-[#5D1919] text-white px-10 py-4 rounded-2xl font-black text-[10px] uppercase tracking-widest hover:bg-[#7D2121] shadow-lg shadow-[#5D1919]/20 transition-all active:scale-95">Finalizar Registro</button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {/* Modal Cambiar Rol Premium */}
            {editingRoleUser && (
                <div className="fixed inset-0 bg-[#5D1919]/20 flex items-center justify-center z-50 p-4 backdrop-blur-md animate-fade-in">
                    <div className="bg-white rounded-[2.5rem] shadow-[0_20px_60px_rgba(0,0,0,0.3)] w-full max-w-sm overflow-hidden border border-[#5D1919]/10">
                        <div className="bg-[#5D1919] text-white px-8 py-6 flex justify-between items-center">
                            <h2 className="text-lg font-serif font-black tracking-tight">Privilegios</h2>
                            <button onClick={() => setEditingRoleUser(null)} className="p-1.5 hover:bg-white/10 rounded-full transition-colors"><X size={20} /></button>
                        </div>
                        <div className="p-8 space-y-6">
                            <div className="flex items-center gap-4 p-4 bg-gray-50 rounded-2xl border border-gray-100">
                                <div className="w-10 h-10 rounded-xl bg-[#5D1919] text-white flex items-center justify-center font-bold text-sm">
                                    {editingRoleUser.nombre.substring(0, 2).toUpperCase()}
                                </div>
                                <div className="overflow-hidden">
                                    <p className="font-serif font-black text-gray-900 truncate">{editingRoleUser.nombre}</p>
                                    <p className="text-[9px] text-gray-400 font-bold uppercase tracking-widest truncate">{editingRoleUser.email}</p>
                                </div>
                            </div>

                            <div className="space-y-3">
                                <label className="text-[9px] font-black text-gray-400 uppercase tracking-widest ml-1">Nuevo Escala Administrativa</label>
                                <select
                                    className="w-full bg-white border border-gray-100 rounded-2xl px-6 py-4 text-sm font-bold text-[#5D1919] outline-none focus:ring-4 focus:ring-[#5D1919]/5 transition-all shadow-sm appearance-none cursor-pointer"
                                    value={newRole} onChange={e => setNewRole(e.target.value)}
                                >
                                    <option value="Usuario">Usuario Final</option>
                                    <option value="Admin">Administrador Maestro</option>
                                    <option value="Repartidor">Logística de Despacho</option>
                                </select>
                            </div>

                            <div className="flex justify-end gap-3 pt-4">
                                <button type="button" onClick={() => setEditingRoleUser(null)} className="px-6 py-3 text-[10px] font-black uppercase tracking-widest text-gray-400 hover:text-gray-600 transition-all">Descartar</button>
                                <button type="button" onClick={handleChangeRole} className="bg-[#5D1919] text-white px-8 py-3 rounded-xl font-black text-[10px] uppercase tracking-widest hover:bg-[#7D2121] shadow-md transition-all active:scale-95">Aplicar Escala</button>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default UsuariosAdmin;
