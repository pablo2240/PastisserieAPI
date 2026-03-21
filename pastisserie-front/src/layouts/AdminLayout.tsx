import { Outlet, Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useState } from 'react';
import {
    LayoutDashboard, Package, ShoppingBag, Tag,
    Users, Settings, LogOut, Search,
    Menu, MessageSquare, Store
} from 'lucide-react';
import { Toaster } from 'react-hot-toast';
import Notificaciones from '../components/common/Notificaciones';

const AdminLayout = () => {
    const { user, logout } = useAuth();
    const navigate = useNavigate();
    const location = useLocation();
    const [isSidebarOpen, setIsSidebarOpen] = useState(false);

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    const isActive = (path: string) => location.pathname === path
        ? "bg-white/20 text-white shadow-lg border-l-4 border-white translate-x-1"
        : "text-white/70 hover:bg-white/10 hover:text-white hover:translate-x-1";

    return (
        <div className="flex h-screen bg-gray-50 font-sans overflow-hidden">
            <Toaster position="top-right" reverseOrder={false} />
            {/* Overlay para móvil */}
            {isSidebarOpen && (
                <div
                    className="fixed inset-0 bg-black/60 backdrop-blur-sm z-20 md:hidden animate-fade-in"
                    onClick={() => setIsSidebarOpen(false)}
                />
            )}

            {/* Sidebar - Color Vino Premium */}
            <aside className={`
                fixed inset-y-0 left-0 z-30 w-72 bg-[#5D1919] text-white flex flex-col shadow-[10px_0_50px_rgba(0,0,0,0.2)] transition-all duration-500 ease-in-out md:relative md:translate-x-0
                ${isSidebarOpen ? 'translate-x-0' : '-translate-x-full'}
            `}>
                <Link to="/" className="p-8 pb-10 flex items-center justify-between hover:bg-white/5 transition-all group">
                    <div className="flex items-center gap-4">
                        <div className="bg-[#EBCfa8] p-3 rounded-[1.2rem] text-[#5D1919] group-hover:rotate-12 transition-transform shadow-xl shadow-black/20">
                            <Package size={24} strokeWidth={2.5} />
                        </div>
                        <div>
                            <h1 className="font-serif font-black text-2xl leading-none tracking-tighter">Patisserie</h1>
                            <p className="text-[10px] text-white/60 uppercase font-black tracking-[0.3em] text-left mt-1">Deluxe Admin</p>
                        </div>
                    </div>
                </Link>

                <nav className="flex-1 px-4 space-y-2 overflow-y-auto custom-scrollbar pt-4">
                    <p className="text-[10px] text-white/60 uppercase font-black tracking-[0.25em] px-5 mb-4">Gestión Principal</p>

                    <Link to="/admin" onClick={() => setIsSidebarOpen(false)} className={`flex items-center gap-4 px-5 py-4 rounded-[1.25rem] transition-all font-black text-sm uppercase tracking-wider ${isActive('/admin')}`}>
                        <LayoutDashboard size={20} strokeWidth={2.5} /> Dashboard
                    </Link>
                    <Link to="/admin/productos" onClick={() => setIsSidebarOpen(false)} className={`flex items-center gap-4 px-5 py-4 rounded-[1.25rem] transition-all font-black text-sm uppercase tracking-wider ${isActive('/admin/productos')}`}>
                        <Package size={20} strokeWidth={2.5} /> Catálogo
                    </Link>
                    <Link to="/admin/pedidos" onClick={() => setIsSidebarOpen(false)} className={`flex items-center gap-4 px-5 py-4 rounded-[1.25rem] transition-all font-black text-sm uppercase tracking-wider ${isActive('/admin/pedidos')}`}>
                        <ShoppingBag size={20} strokeWidth={2.5} /> Órdenes
                    </Link>
                    <Link to="/admin/promociones" onClick={() => setIsSidebarOpen(false)} className={`flex items-center gap-4 px-5 py-4 rounded-[1.25rem] transition-all font-black text-sm uppercase tracking-wider ${isActive('/admin/promociones')}`}>
                        <Tag size={20} strokeWidth={2.5} /> Promociones
                    </Link>
                    <Link to="/admin/usuarios" onClick={() => setIsSidebarOpen(false)} className={`flex items-center gap-4 px-5 py-4 rounded-[1.25rem] transition-all font-black text-sm uppercase tracking-wider ${isActive('/admin/usuarios')}`}>
                        <Users size={20} strokeWidth={2.5} /> Usuarios
                    </Link>
                    <Link to="/admin/resenas" onClick={() => setIsSidebarOpen(false)} className={`flex items-center gap-4 px-5 py-4 rounded-[1.25rem] transition-all font-black text-sm uppercase tracking-wider ${isActive('/admin/resenas')}`}>
                        <MessageSquare size={20} strokeWidth={2.5} /> Reseñas
                    </Link>
                </nav>

                <div className="p-6 mt-auto">
                    <div className="bg-black/20 rounded-[2rem] p-4 border border-white/5 space-y-1">
                        <Link to="/admin/configuracion" onClick={() => setIsSidebarOpen(false)} className={`flex items-center gap-4 px-4 py-3 rounded-xl transition-all text-xs font-black uppercase tracking-widest ${location.pathname === '/admin/configuracion' ? 'bg-white/25 text-white shadow-lg' : 'text-white/60 hover:text-white hover:bg-white/5'}`}>
                            <Settings size={16} strokeWidth={2.5} /> Ajustes
                        </Link>
                        <button onClick={handleLogout} className="flex items-center gap-4 px-4 py-3 text-red-300/60 hover:text-red-300 hover:bg-red-500/10 w-full rounded-xl transition-all text-left text-xs font-black uppercase tracking-widest">
                            <LogOut size={16} strokeWidth={2.5} /> Cerrar Sesión
                        </button>
                    </div>
                </div>
            </aside>

            <main className="flex-1 overflow-y-auto flex flex-col relative bg-[#fcfcfc] custom-scrollbar">
                {/* Top Header Premium - Fixed with proper z-index and spacing */}
                <header className="bg-white/70 backdrop-blur-2xl h-20 flex items-center justify-between px-6 md:px-10 sticky top-0 z-40 shrink-0 border-b border-gray-100/50 shadow-[0_1px_40px_rgba(0,0,0,0.03)]">
                    <div className="flex items-center gap-6 flex-1">
                        <button className="md:hidden text-[#5D1919] p-3 hover:bg-gray-100 rounded-2xl transition-all active:scale-90" onClick={() => setIsSidebarOpen(true)}>
                            <Menu size={24} />
                        </button>

                        {/* Search Bar Premium */}
                        <form
                            onSubmit={(e) => {
                                e.preventDefault();
                                const search = (e.currentTarget.elements.namedItem('search') as HTMLInputElement).value;
                                if (search.trim()) {
                                    navigate(`/admin/productos?search=${encodeURIComponent(search)}`);
                                }
                            }}
                            className="relative w-full max-w-md hidden lg:block group"
                        >
                            <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400 group-focus-within:text-[#5D1919] transition-colors" size={18} />
                            <input
                                name="search"
                                type="text"
                                placeholder="Búsqueda rápida de productos..."
                                className="w-full pl-12 pr-4 py-3 bg-gray-50/50 border-2 border-transparent rounded-[1.25rem] text-sm font-black outline-none focus:ring-4 focus:ring-[#5D1919]/5 focus:bg-white focus:border-[#5D1919]/10 transition-all"
                            />
                        </form>
                    </div>

                    {/* User Profile & Actions */}
                    <div className="flex items-center gap-5">
                        <Link to="/" className="hidden sm:flex items-center gap-2 px-6 py-3 text-[#5D1919] bg-[#EBCfa8]/20 hover:bg-[#EBCfa8]/40 border border-[#EBCfa8]/30 rounded-full text-[10px] font-black uppercase tracking-[0.2em] transition-all transform hover:translate-y-[-2px] active:translate-y-0" title="Ver Tienda Pública">
                            <Store size={14} /> Tienda
                        </Link>

                        <div className="h-8 w-[1px] bg-gray-100 mx-1 hidden sm:block opacity-50"></div>

                        <Notificaciones />

                        <Link to="/perfil" className="flex items-center gap-4 pl-4 group hover:bg-gray-50/80 p-1.5 pr-4 rounded-[1.25rem] transition-all border border-transparent hover:border-gray-100">
                            <div className="text-right hidden sm:block">
                                <p className="text-sm font-black text-gray-900 leading-none mb-1 group-hover:text-[#5D1919] transition-colors">{user?.nombre || 'Administrador'}</p>
                                <p className="text-[10px] text-gray-400 font-black uppercase tracking-[0.2em]">Panel Master</p>
                            </div>
                            <div className="w-10 h-10 bg-[#5D1919] text-[#EBCfa8] rounded-[1.2rem] flex items-center justify-center font-black text-base shadow-xl shadow-[#5D1919]/20 transform group-hover:scale-105 group-hover:rotate-3 transition-all">
                                {user?.nombre?.substring(0, 2).toUpperCase() || 'AD'}
                            </div>
                        </Link>
                    </div>
                </header>

                {/* Content Area with Dynamic Padding to never overlap */}
                <div className="p-8 md:p-12 lg:p-16 flex-1 animate-fade-in relative">
                    <Outlet />
                </div>
            </main>
        </div>
    );
};

export default AdminLayout;
