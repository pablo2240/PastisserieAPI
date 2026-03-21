import React, { useState, useEffect } from 'react';
import { X, Plus, Trash2, Edit2, Check, Layers } from 'lucide-react';
import toast from 'react-hot-toast';
import { categoriasService } from '../../api/categoriasService';
import type { Categoria } from '../../api/categoriasService';

interface CategoriasModalProps {
    onClose: () => void;
    onChange: () => void;
}

const CategoriasModal: React.FC<CategoriasModalProps> = ({ onClose, onChange }) => {
    const [categorias, setCategorias] = useState<Categoria[]>([]);
    const [loading, setLoading] = useState(true);
    const [editingId, setEditingId] = useState<number | null>(null);
    const [editNombre, setEditNombre] = useState('');
    const [newNombre, setNewNombre] = useState('');

    useEffect(() => {
        loadCategorias();
        // Bloquear scroll del fondo
        document.body.classList.add('overflow-hidden');
        return () => {
            document.body.classList.remove('overflow-hidden');
        };
    }, []);

    const loadCategorias = async () => {
        try {
            const response = await categoriasService.getAll();
            if (response.success && Array.isArray(response.data)) {
                setCategorias(response.data);
            }
        } catch (error) {
            toast.error('Error al cargar categorías');
        } finally {
            setLoading(false);
        }
    };

    const handleCreate = async () => {
        if (!newNombre.trim()) return;
        try {
            await categoriasService.create({ nombre: newNombre, activa: true });
            toast.success('Categoría creada');
            setNewNombre('');
            loadCategorias();
            onChange();
        } catch (error) {
            toast.error('Error al crear categoría');
        }
    };

    const handleUpdate = async (id: number) => {
        if (!editNombre.trim()) return;
        try {
            await categoriasService.update(id, { id, nombre: editNombre, activa: true });
            toast.success('Categoría actualizada');
            setEditingId(null);
            loadCategorias();
            onChange();
        } catch (error) {
            toast.error('Error al actualizar');
        }
    };

    const handleDelete = async (id: number) => {
        if (!window.confirm('¿Eliminar categoría? Esto podría afectar a los productos asociados.')) return;
        try {
            await categoriasService.delete(id);
            toast.success('Categoría eliminada');
            loadCategorias();
            onChange();
        } catch (error) {
            toast.error('Error al eliminar');
        }
    };

    return (
        <div role="dialog" className="fixed inset-0 bg-[#5D1919]/20 flex items-center justify-center z-[1000] p-4 backdrop-blur-md animate-fade-in">
            <div className="bg-white rounded-[2.5rem] shadow-[0_20px_60px_rgba(0,0,0,0.3)] w-full max-w-lg overflow-hidden flex flex-col max-h-[80vh] border border-[#5D1919]/10">

                <div className="bg-[#5D1919] text-white px-10 py-8 flex justify-between items-center shrink-0 relative overflow-hidden">
                    <div className="absolute top-0 right-0 w-32 h-32 bg-white/5 rounded-bl-[5rem] -mr-10 -mt-10"></div>
                    <div className="relative z-10">
                        <h2 className="text-2xl font-serif font-black tracking-tight">Colecciones</h2>
                        <p className="text-white/60 text-[10px] font-bold uppercase tracking-widest mt-1">Gestión de taxonomía maestra</p>
                    </div>
                    <button onClick={onClose} className="hover:bg-white/10 p-2 rounded-full transition-all relative z-10">
                        <X size={24} />
                    </button>
                </div>

                <div className="p-10 overflow-y-auto flex-1 bg-[#fcfcfc]">
                    {/* Input Crear */}
                    <div className="mb-10">
                        <label className="block text-[10px] font-black text-gray-400 uppercase tracking-[0.2em] mb-3 ml-1">Nueva Clasificación</label>
                        <div className="flex gap-3">
                            <input
                                value={newNombre}
                                onChange={(e) => setNewNombre(e.target.value)}
                                placeholder="Ej: Bebidas, Postres, Panadería..."
                                className="flex-1 bg-white border border-gray-100 rounded-2xl px-6 py-4 text-sm font-bold focus:ring-4 focus:ring-[#5D1919]/5 outline-none transition-all shadow-sm"
                            />
                            <button
                                onClick={handleCreate}
                                disabled={!newNombre.trim()}
                                className="bg-[#5D1919] text-white px-8 rounded-2xl hover:bg-[#7D2121] disabled:opacity-50 shadow-lg shadow-[#5D1919]/20 transition-all active:scale-95 flex items-center justify-center"
                            >
                                <Plus size={24} strokeWidth={3} />
                            </button>
                        </div>
                    </div>

                    <label className="block text-[10px] font-black text-gray-400 uppercase tracking-[0.2em] mb-4 ml-1">Familias Registradas</label>
                    <div className="space-y-3">
                        {loading ? (
                            <div className="flex flex-col items-center py-10 bg-white rounded-3xl border border-gray-50">
                                <div className="w-10 h-10 border-4 border-[#5D1919]/10 border-t-[#5D1919] rounded-full animate-spin"></div>
                                <p className="mt-4 text-[10px] font-black uppercase tracking-widest text-gray-400">Consultando Base de Datos...</p>
                            </div>
                        ) :
                            categorias.map(cat => (
                                <div key={cat.id} className="group animate-fade-in">
                                    {editingId === cat.id ? (
                                        <div className="flex items-center gap-3 p-2 bg-white border-2 border-[#5D1919]/20 rounded-2xl shadow-sm">
                                            <input
                                                autoFocus
                                                value={editNombre}
                                                onChange={(e) => setEditNombre(e.target.value)}
                                                className="flex-1 bg-transparent px-4 py-2 text-sm font-black text-[#5D1919] outline-none"
                                            />
                                            <div className="flex gap-2 pr-2">
                                                <button onClick={() => handleUpdate(cat.id)} className="bg-[#5D1919] text-white p-2.5 rounded-xl hover:bg-[#7D2121] shadow-md transition-all"><Check size={16} strokeWidth={3} /></button>
                                                <button onClick={() => setEditingId(null)} className="bg-gray-100 text-gray-400 p-2.5 rounded-xl hover:bg-gray-200 transition-all"><X size={16} /></button>
                                            </div>
                                        </div>
                                    ) : (
                                        <div className="flex justify-between items-center p-5 bg-white border border-gray-50 hover:border-[#5D1919]/20 rounded-[1.5rem] transition-all group-hover:shadow-md">
                                            <div className="flex items-center gap-4">
                                                <div className="w-2 h-2 rounded-full bg-[#5D1919]/20 group-hover:bg-[#5D1919] transition-colors"></div>
                                                <span className="font-serif font-black text-gray-900 text-base">{cat.nombre}</span>
                                            </div>
                                            <div className="flex gap-2 opacity-0 group-hover:opacity-100 transition-all translate-x-4 group-hover:translate-x-0">
                                                <button
                                                    onClick={() => { setEditingId(cat.id); setEditNombre(cat.nombre); }}
                                                    className="p-2.5 text-gray-400 hover:text-[#5D1919] hover:bg-[#5D1919]/5 rounded-xl transition-all"
                                                    title="Refinar"
                                                >
                                                    <Edit2 size={18} />
                                                </button>
                                                <button
                                                    onClick={() => handleDelete(cat.id)}
                                                    className="p-2.5 text-gray-400 hover:text-rose-600 hover:bg-rose-50 rounded-xl transition-all"
                                                    title="Eliminar"
                                                >
                                                    <Trash2 size={18} />
                                                </button>
                                            </div>
                                        </div>
                                    )}
                                </div>
                            ))
                        }
                        {!loading && categorias.length === 0 && (
                            <div className="text-center py-16 bg-white rounded-[2rem] border-2 border-dashed border-gray-100 flex flex-col items-center">
                                <Layers className="text-gray-200 mb-4" size={48} />
                                <p className="text-gray-400 text-xs font-black uppercase tracking-widest">Sin Colecciones Registradas</p>
                            </div>
                        )}
                    </div>
                </div>

                <div className="px-10 py-6 bg-gray-50 border-t border-gray-100 flex justify-center">
                    <button
                        onClick={onClose}
                        className="px-8 py-3 rounded-xl text-[10px] font-black uppercase tracking-widest text-gray-400 hover:text-gray-600 transition-all"
                    >
                        Salir de Gestión
                    </button>
                </div>
            </div>
        </div>
    );
};

export default CategoriasModal;
