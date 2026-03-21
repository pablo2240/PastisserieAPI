import { useEffect, useState, useRef } from 'react';
import { useLocation } from 'react-router-dom';
import api from '../../api/axios';
import {
  Edit3, Trash2, Plus, Search, X,
  Save, Image as ImageIcon,
  ChevronUp, ChevronDown, Filter, Layers, AlertCircle
} from 'lucide-react';
import toast from 'react-hot-toast';
import { categoriasService } from '../../api/categoriasService';
import type { Categoria } from '../../api/categoriasService';
import CategoriasModal from '../../components/admin/CategoriasModal';
import { formatCurrency } from '../../utils/format';

// Interfaz del Producto
interface Producto {
  id: number;
  nombre: string;
  descripcion: string;
  precio: number;
  stock: number;
  categoria: string;
  imagenUrl: string;
  activo: boolean;
}

// Estado inicial del formulario
const initialFormState = {
  nombre: '',
  descripcion: '',
  precio: 0,
  stock: 0,
  categoria: '',
  imagenUrl: '',
  activo: true
};

const ProductosAdmin = () => {
  const [productos, setProductos] = useState<Producto[]>([]);
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [loading, setLoading] = useState(true);
  const [busqueda, setBusqueda] = useState('');
  const [filtroCategoria, setFiltroCategoria] = useState('');
  const [filtroStock, setFiltroStock] = useState<'todos' | 'bajo' | 'agotado'>('todos');
  const [ordenarPor, setOrdenarPor] = useState<'nombre' | 'precio' | 'stock'>('nombre');
  const [ordenDireccion, setOrdenDireccion] = useState<'asc' | 'desc'>('asc');

  // Estados del Modal
  const [showModal, setShowModal] = useState(false);
  const [showCategoriasModal, setShowCategoriasModal] = useState(false);
  const [isEditing, setIsEditing] = useState(false);
  const [currentId, setCurrentId] = useState<number | null>(null);
  const [formData, setFormData] = useState(initialFormState);
  const [errors, setErrors] = useState<{ nombre?: string; precio?: string; stock?: string }>({});
  const fileInputRef = useRef<HTMLInputElement>(null);

  // DRAG & DROP STATES
  const [isDragging, setIsDragging] = useState(false);
  const [isUploading, setIsUploading] = useState(false);

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(true);
  };

  const handleDragLeave = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
  };

  const handleDrop = async (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);

    if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
      await uploadImage(e.dataTransfer.files[0]);
    }
  };

  const handleFileSelect = async (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      await uploadImage(e.target.files[0]);
    }
  };

  const uploadImage = async (file: File) => {
    setIsUploading(true);
    const uploadData = new FormData();
    uploadData.append('file', file);

    try {
      const response = await api.post('/upload', uploadData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      });

      if (response.data && response.data.data && response.data.data.url) {
        setFormData(prev => ({ ...prev, imagenUrl: response.data.data.url }));
        toast.success('Imagen subida correctamente');
      }
    } catch (error) {
      console.error(error);
      toast.error('Error al subir la imagen');
    } finally {
      setIsUploading(false);
    }
  };

  const location = useLocation();

  useEffect(() => {
    fetchProductos();
    fetchCategorias();

    // Capturar búsqueda desde la URL (AdminLayout)
    const params = new URLSearchParams(location.search);
    const searchParam = params.get('search');
    if (searchParam) {
      setBusqueda(searchParam);
    }
  }, [location.search]);

  const fetchCategorias = async () => {
    try {
      const response = await categoriasService.getAll();
      if (response.success && Array.isArray(response.data)) {
        setCategorias(response.data);
      }
    } catch (error) {
      console.error("Error cargando categorías");
    }
  };

  const fetchProductos = async () => {
    try {
      const response = await api.get('/productos');
      let data = [];
      if (Array.isArray(response.data)) data = response.data;
      else if (response.data?.data && Array.isArray(response.data.data)) data = response.data.data;

      setProductos(data);
    } catch (error) {
      console.error(error);
      toast.error('Error al cargar productos');
    } finally {
      setLoading(false);
    }
  };

  // --- MANEJADORES DEL FORMULARIO ---

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    const finalValue = type === 'number' ? Number(value) : value;
    setFormData(prev => ({ ...prev, [name]: finalValue }));
  };

  const openNewModal = () => {
    setFormData(initialFormState);
    setErrors({});
    setIsEditing(false);
    setShowModal(true);
    document.body.classList.add('overflow-hidden');
  };

  const openEditModal = (producto: Producto) => {
    setFormData({
      nombre: producto.nombre,
      descripcion: producto.descripcion || '',
      precio: producto.precio,
      stock: producto.stock,
      categoria: producto.categoria || '',
      imagenUrl: producto.imagenUrl || '',
      activo: producto.activo
    });
    setErrors({});
    setCurrentId(producto.id);
    setIsEditing(true);
    setShowModal(true);
    document.body.classList.add('overflow-hidden');
  };

  const validate = () => {
    const newErrors: { nombre?: string; precio?: string; stock?: string } = {};
    if (!formData.nombre.trim()) newErrors.nombre = 'El nombre es obligatorio';
    if (formData.precio <= 0) newErrors.precio = 'El precio debe ser mayor a 0';
    if (formData.stock < 0) newErrors.stock = 'El stock no puede ser negativo';

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) {
      toast.error('Por favor corrige los errores en el formulario');
      return;
    }

    try {
      if (isEditing && currentId) {
        await api.put(`/productos/${currentId}`, { ...formData, id: currentId });
        toast.success('Producto actualizado correctamente');
      } else {
        await api.post('/productos', formData);
        toast.success('Producto creado exitosamente');
      }
      setShowModal(false);
      document.body.classList.remove('overflow-hidden');
      fetchProductos();
    } catch (error) {
      console.error(error);
      toast.error('Error al guardar el producto');
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('¿Estás seguro de eliminar este producto?')) return;
    try {
      await api.delete(`/productos/${id}`);
      toast.success('Producto eliminado');
      fetchProductos();
    } catch (error) {
      toast.error('No se pudo eliminar');
    }
  };

  const listaSegura = Array.isArray(productos) ? productos : [];

  const productosFiltrados = listaSegura.filter(p => {
    const query = busqueda.toLowerCase().trim();
    const matchesBusqueda = !query ||
      (p.nombre?.toLowerCase() || '').includes(query) ||
      (p.categoria?.toLowerCase() || '').includes(query) ||
      (p.descripcion?.toLowerCase() || '').includes(query) ||
      p.id.toString() === query || `#${p.id}` === query || p.id.toString().includes(query);

    const matchesCategoria = !filtroCategoria || p.categoria === filtroCategoria;

    let matchesStock = true;
    if (filtroStock === 'bajo') matchesStock = p.stock > 0 && p.stock < 10;
    else if (filtroStock === 'agotado') matchesStock = p.stock <= 0;

    return matchesBusqueda && matchesCategoria && matchesStock;
  });

  const productosOrdenados = [...productosFiltrados].sort((a, b) => {
    let valA: any = a[ordenarPor];
    let valB: any = b[ordenarPor];

    if (typeof valA === 'string') {
      valA = valA.toLowerCase();
      valB = valB.toLowerCase();
    }

    if (valA < valB) return ordenDireccion === 'asc' ? -1 : 1;
    if (valA > valB) return ordenDireccion === 'asc' ? 1 : -1;
    return 0;
  });

  const toggleOrden = (key: 'nombre' | 'precio' | 'stock') => {
    if (ordenarPor === key) {
      setOrdenDireccion(prev => prev === 'asc' ? 'desc' : 'asc');
    } else {
      setOrdenarPor(key);
      setOrdenDireccion('asc');
    }
  };

  return (
    <div className="animate-fade-in p-2 pb-20">
      {/* Cabecera Premium */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center mb-10 gap-4">
        <div>
          <h1 className="text-3xl md:text-4xl font-serif font-black text-[#5D1919] tracking-tighter mb-2">Catálogo Maestro</h1>
          <p className="text-gray-500 font-medium text-sm">Control total de inventario, precios y visualización de productos</p>
        </div>
        <div className="flex flex-wrap gap-3">
          <button
            onClick={() => setShowCategoriasModal(true)}
            className="bg-white text-gray-700 border border-gray-100 px-4 py-2.5 rounded-2xl flex items-center gap-3 hover:bg-gray-50 transition-all shadow-sm font-black uppercase tracking-widest text-[10px]"
          >
            <Layers size={16} className="text-gray-400" /> Gestionar Categorías
          </button>
          <button
            onClick={openNewModal}
            className="bg-[#5D1919] text-white px-6 py-2.5 rounded-2xl flex items-center gap-3 hover:bg-[#7D2121] transition-all shadow-xl shadow-[#5D1919]/20 font-black uppercase tracking-widest text-[10px]"
          >
            <Plus size={18} strokeWidth={3} /> Añadir Producto
          </button>
        </div>
      </div>

      {/* Main Container - Premium Card */}
      <div className="bg-white rounded-[2.5rem] shadow-[0_10px_40px_rgba(0,0,0,0.03)] border border-gray-100 overflow-hidden">
        {/* Filter Bar Redesigned */}
        <div className="p-4 md:p-8 border-b border-gray-50 flex flex-wrap items-center gap-4 bg-[#fcfcfc]">
          <div className="flex-1 min-w-[200px] relative group">
            <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400 group-focus-within:text-[#5D1919] transition-colors" size={18} />
            <input
              type="text"
              placeholder="Buscar por nombre, ID o categoría..."
              className="w-full bg-white border border-gray-200 rounded-[1.25rem] pl-12 pr-4 py-3 text-sm font-medium outline-none focus:ring-4 focus:ring-[#5D1919]/5 focus:border-[#5D1919]/20 transition-all shadow-inner"
              value={busqueda}
              onChange={(e) => setBusqueda(e.target.value)}
            />
          </div>

          <div className="flex flex-wrap items-center gap-3">
            <div className="flex flex-col gap-1">
              <span className="text-[9px] font-black text-gray-400 uppercase tracking-widest ml-1">Colección</span>
              <div className="relative">
                <Filter className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none" size={12} />
                <select
                  value={filtroCategoria}
                  onChange={(e) => setFiltroCategoria(e.target.value)}
                  className="bg-gray-100/50 border-none rounded-xl pl-9 pr-8 py-2.5 text-xs font-bold text-gray-700 outline-none focus:ring-2 focus:ring-[#5D1919]/10 appearance-none cursor-pointer hover:bg-gray-100 transition-colors"
                >
                  <option value="">Todas las Categorías</option>
                  {categorias.map(cat => (
                    <option key={cat.id} value={cat.nombre}>{cat.nombre}</option>
                  ))}
                </select>
              </div>
            </div>

            <div className="flex flex-col gap-1">
              <span className="text-[9px] font-black text-gray-400 uppercase tracking-widest ml-1">Disponibilidad</span>
              <select
                value={filtroStock}
                onChange={(e) => setFiltroStock(e.target.value as any)}
                className="bg-gray-100/50 border-none rounded-xl px-4 py-2.5 text-xs font-bold text-gray-700 outline-none focus:ring-2 focus:ring-[#5D1919]/10 cursor-pointer hover:bg-gray-100 transition-colors"
              >
                <option value="todos">Vistazo General</option>
                <option value="bajo">Stock Crítico</option>
                <option value="agotado">Agotados</option>
              </select>
            </div>

            {(busqueda || filtroCategoria || filtroStock !== 'todos') && (
              <button
                onClick={() => { setBusqueda(''); setFiltroCategoria(''); setFiltroStock('todos'); }}
                className="mt-5 px-4 py-2 text-[10px] font-black text-rose-600 hover:bg-rose-50 rounded-xl transition-all flex items-center gap-2"
              >
                <X size={14} /> Resetear
              </button>
            )}
          </div>
        </div>

        <div className="overflow-x-auto w-full">
          <table className="w-full text-left text-sm text-gray-600">
            <thead className="bg-[#fcfcfc] border-b border-gray-50">
              <tr>
                <th className="px-8 py-6 text-[10px] font-black uppercase tracking-widest text-gray-400">Imagen</th>
                <th className="px-8 py-6 cursor-pointer group" onClick={() => toggleOrden('nombre')}>
                  <div className="flex items-center gap-2 text-[10px] font-black uppercase tracking-widest text-gray-400">
                    Producto
                    {ordenarPor === 'nombre' ? (
                      ordenDireccion === 'asc' ? <ChevronUp size={14} className="text-[#5D1919]" /> : <ChevronDown size={14} className="text-[#5D1919]" />
                    ) : <ChevronUp size={14} className="opacity-0 group-hover:opacity-100 transition-opacity" />}
                  </div>
                </th>
                <th className="px-8 py-6 cursor-pointer group" onClick={() => toggleOrden('precio')}>
                  <div className="flex items-center gap-2 text-[10px] font-black uppercase tracking-widest text-gray-400">
                    Precio
                    {ordenarPor === 'precio' ? (
                      ordenDireccion === 'asc' ? <ChevronUp size={14} className="text-[#5D1919]" /> : <ChevronDown size={14} className="text-[#5D1919]" />
                    ) : <ChevronUp size={14} className="opacity-0 group-hover:opacity-100 transition-opacity" />}
                  </div>
                </th>
                <th className="px-8 py-6 cursor-pointer group" onClick={() => toggleOrden('stock')}>
                  <div className="flex items-center gap-2 text-[10px] font-black uppercase tracking-widest text-gray-400">
                    Suministro
                    {ordenarPor === 'stock' ? (
                      ordenDireccion === 'asc' ? <ChevronUp size={14} className="text-[#5D1919]" /> : <ChevronDown size={14} className="text-[#5D1919]" />
                    ) : <ChevronUp size={14} className="opacity-0 group-hover:opacity-100 transition-opacity" />}
                  </div>
                </th>
                <th className="px-8 py-6 text-right text-[10px] font-black uppercase tracking-widest text-gray-400">Acciones</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50">
              {loading ? (
                <tr><td colSpan={5} className="text-center py-20 font-serif italic text-gray-400">Cargando catálogo...</td></tr>
              ) : productosOrdenados.map((prod) => (
                <tr key={prod.id} className="hover:bg-[#5D1919]/[0.02] transition-all group">
                  <td className="px-8 py-5">
                    <div className="w-14 h-14 rounded-2xl bg-gray-50 overflow-hidden shadow-sm border border-gray-100 group-hover:scale-105 transition-transform duration-500">
                      <img
                        src={prod.imagenUrl ? `${import.meta.env.VITE_API_URL}${prod.imagenUrl}` : 'https://via.placeholder.com/60'}
                        alt={prod.nombre}
                        className="w-full h-full object-cover"
                      />
                    </div>
                  </td>
                  <td className="px-8 py-5">
                    <div className="flex flex-col">
                      <span className="font-serif font-black text-gray-900 group-hover:text-[#5D1919] transition-colors">{prod.nombre}</span>
                      <span className="text-[10px] text-gray-400 font-bold uppercase tracking-tighter">ID: #{prod.id}</span>
                    </div>
                  </td>
                  <td className="px-8 py-5">
                    <span className="font-black text-[#5D1919] text-base">{formatCurrency(prod.precio)}</span>
                  </td>
                  <td className="px-8 py-5">
                    <div className="flex flex-col gap-1">
                      <span className={`px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest w-fit border ${prod.stock < 10 ? 'bg-rose-50 text-rose-600 border-rose-100' : 'bg-emerald-50 text-emerald-600 border-emerald-100'
                        }`}>
                        {prod.stock} unidades
                      </span>
                      {prod.stock < 10 && (
                        <span className="flex items-center gap-1 text-[9px] font-black text-rose-500 uppercase tracking-tighter ml-1">
                          <AlertCircle size={10} /> {prod.stock === 0 ? 'Agotado' : 'Stock Crítico'}
                        </span>
                      )}
                    </div>
                  </td>
                  <td className="px-8 py-5">
                    <div className="flex justify-end gap-2">
                      <button
                        onClick={() => openEditModal(prod)}
                        className="p-2.5 bg-gray-50 text-gray-600 rounded-xl hover:bg-[#5D1919] hover:text-white transition-all shadow-sm border border-gray-100"
                        title="Configurar Parámetros"
                      >
                        <Edit3 size={16} />
                      </button>
                      <button
                        onClick={() => handleDelete(prod.id)}
                        className="p-2.5 bg-gray-50 text-rose-600 rounded-xl hover:bg-rose-600 hover:text-white transition-all shadow-sm border border-gray-100"
                        title="Retirar del Catálogo"
                      >
                        <Trash2 size={16} />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* --- MODAL (FORMULARIO PRODUCTO) --- */}
      {showModal && (
        <div role="dialog" className="fixed inset-0 bg-[#5D1919]/20 flex items-center justify-center z-[1000] p-4 backdrop-blur-md animate-fade-in">
          <div className="bg-white rounded-[2.5rem] shadow-[0_20px_60px_rgba(0,0,0,0.3)] w-full max-w-2xl overflow-hidden flex flex-col max-h-[90vh] border border-[#5D1919]/10">
            <div className="bg-[#5D1919] text-white px-10 py-8 flex justify-between items-center shrink-0 relative overflow-hidden">
              <div className="absolute top-0 right-0 w-32 h-32 bg-white/5 rounded-bl-[5rem] -mr-10 -mt-10"></div>
              <div className="relative z-10">
                <h2 className="text-2xl font-serif font-black tracking-tight">{isEditing ? 'Refinar Producto' : 'Nueva Creación'}</h2>
                <p className="text-white/60 text-[10px] font-bold uppercase tracking-widest mt-1">Parámetros Maestros de Inventario</p>
              </div>
              <button onClick={() => { setShowModal(false); document.body.classList.remove('overflow-hidden'); }} className="hover:bg-white/10 p-2 rounded-full transition-all relative z-10">
                <X size={24} />
              </button>
            </div>

            <form onSubmit={handleSave} className="p-10 space-y-8 overflow-y-auto flex-1 bg-[#fcfcfc]">
              <div className="space-y-6">
                <div>
                  <label className="block text-[10px] font-black text-gray-400 uppercase tracking-[0.2em] mb-3 ml-1">Identidad del Producto</label>
                  <input
                    name="nombre"
                    value={formData.nombre}
                    onChange={handleInputChange}
                    className={`w-full bg-white border rounded-2xl px-6 py-4 text-sm font-serif font-black focus:ring-4 outline-none transition-all shadow-sm ${errors.nombre ? 'border-rose-500 focus:ring-rose-50' : 'border-gray-100 focus:ring-[#5D1919]/5 focus:border-[#5D1919]/20'}`}
                    placeholder="Nombre del manjar..."
                  />
                  {errors.nombre && <p className="text-[10px] text-rose-500 mt-2 font-bold flex items-center gap-1 ml-1"><AlertCircle size={10} /> {errors.nombre}</p>}
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-8">
                  <div className="space-y-2">
                    <label className="block text-[10px] font-black text-gray-400 uppercase tracking-[0.2em] mb-1 ml-1">Arquitectura de Precios</label>
                    <div className="relative group">
                      <div className="absolute inset-y-0 left-0 pl-6 flex items-center pointer-events-none text-[#5D1919] font-black">
                        $
                      </div>
                      <input
                        type="number"
                        name="precio"
                        placeholder="0"
                        value={formData.precio}
                        onChange={handleInputChange}
                        onFocus={(e) => e.target.select()}
                        className={`w-full bg-white border pl-12 pr-6 py-4 rounded-2xl outline-none focus:ring-4 transition-all font-black text-gray-900 ${errors.precio ? 'border-rose-500 focus:ring-rose-50' : 'border-gray-100 focus:ring-[#5D1919]/5 focus:border-[#5D1919]/20'}`}
                      />
                    </div>
                    {errors.precio && <p className="text-[10px] text-rose-500 mt-1 font-bold flex items-center gap-1 ml-1"><AlertCircle size={10} /> {errors.precio}</p>}
                  </div>

                  <div className="space-y-2">
                    <label className="block text-[10px] font-black text-gray-400 uppercase tracking-[0.2em] mb-1 ml-1">Nivel de Stock</label>
                    <div className="relative">
                      <input
                        type="number"
                        name="stock"
                        placeholder="0"
                        value={formData.stock}
                        onChange={handleInputChange}
                        onFocus={(e) => e.target.select()}
                        className={`w-full bg-white border px-6 py-4 rounded-2xl outline-none focus:ring-4 transition-all font-black text-gray-900 ${errors.stock ? 'border-rose-500 focus:ring-rose-50' : 'border-gray-100 focus:ring-[#5D1919]/5 focus:border-[#5D1919]/20'}`}
                      />
                      {formData.stock < 10 && <AlertCircle className="absolute right-6 top-1/2 -translate-y-1/2 text-rose-500" size={18} />}
                    </div>
                    {errors.stock && <p className="text-[10px] text-rose-500 mt-1 font-bold flex items-center gap-1 ml-1"><AlertCircle size={10} /> {errors.stock}</p>}
                  </div>
                  <div>
                    <label className="block text-[10px] font-black text-gray-400 uppercase tracking-[0.2em] mb-3 ml-1">Colección</label>
                    <div className="relative">
                      <select
                        name="categoria"
                        value={formData.categoria}
                        onChange={handleInputChange}
                        className="w-full bg-white border border-gray-100 px-6 py-4 rounded-2xl focus:ring-4 focus:ring-[#5D1919]/5 focus:border-[#5D1919]/20 outline-none font-bold text-gray-700 appearance-none cursor-pointer shadow-sm"
                      >
                        <option value="">Seleccione Familia...</option>
                        {categorias.map(cat => (
                          <option key={cat.id} value={cat.nombre}>{cat.nombre}</option>
                        ))}
                      </select>
                      <Filter className="absolute right-6 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none" size={14} />
                    </div>
                  </div>
                </div>
                <div>
                  <label className="block text-[10px] font-black text-gray-400 uppercase tracking-[0.2em] mb-3 ml-1">Presentación Visual</label>
                  <div
                    onClick={() => fileInputRef.current?.click()}
                    className={`h-[58px] border-2 border-dashed rounded-2xl flex items-center justify-center gap-3 cursor-pointer transition-all ${isDragging ? 'border-[#5D1919] bg-[#5D1919]/5' : 'border-gray-100 hover:border-[#5D1919]/30 hover:bg-gray-50'}`}
                    onDragOver={handleDragOver}
                    onDragLeave={handleDragLeave}
                    onDrop={handleDrop}
                  >
                    {formData.imagenUrl ? (
                      <>
                        <div className="w-8 h-8 rounded-lg overflow-hidden border border-gray-100">
                          <img src={formData.imagenUrl.startsWith('http') ? formData.imagenUrl : `${import.meta.env.VITE_API_URL}${formData.imagenUrl}`} className="w-full h-full object-cover" alt="preview" />
                        </div>
                        <span className="text-[10px] font-black text-[#5D1919] uppercase tracking-widest">Reemplazar Asset</span>
                      </>
                    ) : (
                      <>
                        <ImageIcon className="text-gray-300" size={18} />
                        <span className="text-[10px] font-black text-gray-400 uppercase tracking-widest">Cargar Media</span>
                      </>
                    )}
                    <input
                      ref={fileInputRef}
                      type="file"
                      accept="image/*"
                      className="hidden"
                      onChange={handleFileSelect}
                    />
                  </div>
                </div>
              </div>

              <div>
                <label className="block text-[10px] font-black text-gray-400 uppercase tracking-[0.2em] mb-3 ml-1">Reseña Gastronómica / Descripción</label>
                <textarea
                  name="descripcion"
                  value={formData.descripcion}
                  onChange={handleInputChange}
                  className="w-full bg-white border border-gray-100 px-6 py-4 rounded-2xl focus:ring-4 focus:ring-[#5D1919]/5 focus:border-[#5D1919]/20 outline-none h-32 resize-none text-sm font-medium text-gray-600 shadow-sm"
                  placeholder="Describe los matices, ingredientes y experiencia del producto..."
                ></textarea>
              </div>

              <div className="pt-8 border-t border-gray-100 flex justify-end gap-4 items-center">
                <button
                  type="button"
                  onClick={() => { setShowModal(false); document.body.classList.remove('overflow-hidden'); }}
                  className="px-8 py-4 text-[10px] font-black uppercase tracking-widest text-gray-400 hover:text-gray-600 transition-all"
                >
                  Descartar
                </button>
                <button
                  type="submit"
                  disabled={isUploading}
                  className="bg-[#5D1919] text-white px-10 py-4 rounded-2xl font-black uppercase tracking-widest text-[10px] hover:bg-[#7D2121] transition-all shadow-xl shadow-[#5D1919]/20 flex items-center gap-3 active:scale-95 disabled:opacity-50"
                >
                  {isUploading ? (
                    <span className="flex items-center gap-2">
                      <div className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin"></div> Procesando Media...
                    </span>
                  ) : (
                    <>
                      <Save size={18} strokeWidth={3} /> {isEditing ? 'Guardar Cambios Maestros' : 'Publicar Producto'}
                    </>
                  )}
                </button>
              </div>
            </form>
          </div>
        </div >
      )}

      {/* --- MODAL DE CATEGORÍAS --- */}
      {
        showCategoriasModal && (
          <CategoriasModal
            onClose={() => setShowCategoriasModal(false)}
            onChange={fetchCategorias}
          />
        )
      }
    </div >
  );
};

export default ProductosAdmin;