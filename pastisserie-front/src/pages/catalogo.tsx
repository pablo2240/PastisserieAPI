import { useEffect, useState } from 'react';
import { FiFilter, FiSearch } from 'react-icons/fi';
import api from '../api/axios';
import ProductCard from '../components/ProductCard';
import { type Producto } from '../types';
import { ProductListSkeleton, CategorySkeleton } from '../components/common/Skeletons';
import { useTiendaStatus } from '../hooks/useTiendaStatus';

const Catalogo = () => {
    const { status: tiendaStatus } = useTiendaStatus();


    const [productos, setProductos] = useState<Producto[]>([]);
    const [loading, setLoading] = useState(true);
    const [categoriaFiltro, setCategoriaFiltro] = useState('Todos');
    const [busqueda, setBusqueda] = useState('');
    const [ordenarPor, setOrdenarPor] = useState('nombre'); // 'nombre', 'precio-asc', 'precio-desc'
    const [precioRango] = useState<[number, number]>([0, 10000000]);

    useEffect(() => {
        fetchProductos();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const fetchProductos = async () => {
        setLoading(true);
        try {
            const response = await api.get('/productos');

            const rawData = response?.data?.data ||
                response?.data?.result ||
                response?.data || [];

            const data: Producto[] = Array.isArray(rawData) ? rawData : [];
            const productosValidos = data.filter(p => p.activo !== false);
            setProductos(productosValidos);

        } catch (error: any) {
            console.error("❌ Error API:", error.response?.data || error.message);
            setProductos([]);
        } finally {
            setLoading(false);
        }
    };


    const productosFiltrados = productos
        .filter(p => {
            const catP = p.categoria?.trim().toLowerCase() || '';
            const catF = categoriaFiltro.trim().toLowerCase();
            const coincideCategoria = catF === 'todos' || catP === catF;
            const coincideBusqueda = p.nombre.toLowerCase().includes(busqueda.toLowerCase());
            const coincidePrecio = p.precio >= precioRango[0] && p.precio <= precioRango[1];
            return coincideCategoria && coincideBusqueda && coincidePrecio;
        })
        .sort((a, b) => {
            if (ordenarPor === 'precio-asc') return a.precio - b.precio;
            if (ordenarPor === 'precio-desc') return b.precio - a.precio;
            return a.nombre.localeCompare(b.nombre);
        });

    const categorias = ['Todos', ...new Set(productos.map(p => p.categoria).filter(Boolean))];

    return (
        <div className="min-h-screen bg-gray-50 pt-24 pb-20 animate-fade-in transition-all duration-500">
            {/* Header Banner */}
            <div className="bg-[#1a0505] text-white py-24 mb-16 pt-36 px-4 shadow-[0_30px_60px_rgba(0,0,0,0.1)] relative overflow-hidden transition-opacity duration-300">
                <div className="absolute inset-0 opacity-10">
                    <img src="https://images.unsplash.com/photo-1495147466023-ac5c588e2e94?q=80&w=2070&auto=format&fit=crop" alt="Bakery pattern" className="w-full h-full object-cover scale-110" />
                </div>
                <div className="absolute -bottom-24 -left-20 w-96 h-96 bg-patisserie-red/10 rounded-full blur-[100px]" />
                <div className="container mx-auto text-center relative z-10 space-y-4">
                    <span className="text-patisserie-red font-black uppercase tracking-[0.5em] text-[10px]">La Collection</span>
                    <h1 className="text-5xl md:text-6xl lg:text-8xl font-serif font-black mb-6 text-white tracking-tighter">Nuestro <span className="text-patisserie-red italic underline decoration-1 underline-offset-[12px]">Catálogo</span></h1>
                    <p className="text-gray-400 max-w-2xl mx-auto text-lg font-medium leading-relaxed">
                        Explora una selección curada de alta repostería, donde cada ingrediente cuenta una historia de sabor.
                    </p>
                </div>
            </div>


            <div className="container mx-auto px-6">

                {/* BARRA DE HERRAMIENTAS (Sticky) */}
                <div className="bg-white/80 backdrop-blur-xl p-5 rounded-[2rem] shadow-[0_10px_40px_rgba(0,0,0,0.03)] mb-16 flex flex-col lg:flex-row gap-6 justify-between items-center sticky top-28 z-[80] border border-white transition-all hover:shadow-xl opacity-100 translate-y-0">

                    {/* Categorías */}
                    <div className="flex items-center gap-3 overflow-x-auto w-full lg:w-auto pb-2 lg:pb-0 no-scrollbar">
                        <div className="w-10 h-10 bg-gray-50 rounded-xl flex items-center justify-center text-gray-400 shrink-0">
                            <FiFilter />
                        </div>
                        {loading ? (
                            <div className="flex gap-2">
                                <CategorySkeleton />
                                <CategorySkeleton />
                                <CategorySkeleton />
                            </div>
                        ) : (
                            categorias.map(cat => (
                                <button
                                    key={cat}
                                    onClick={() => setCategoriaFiltro(cat)}
                                    className={`px-6 py-2.5 rounded-xl text-[10px] font-black transition-all whitespace-nowrap uppercase tracking-[0.2em] border-2 ${categoriaFiltro === cat
                                        ? 'bg-patisserie-red border-patisserie-red text-white shadow-[0_10px_20px_rgba(125,33,33,0.2)]'
                                        : 'bg-white border-transparent text-gray-400 hover:text-patisserie-red hover:bg-red-50'
                                        }`}
                                >
                                    {cat}
                                </button>
                            ))
                        )}
                    </div>

                    {/* Ordenar y Filtros Adicionales */}
                    <div className="flex flex-col sm:flex-row items-center gap-4 w-full lg:w-auto">
                        <div className="relative w-full sm:w-auto">
                            <select
                                value={ordenarPor}
                                onChange={(e) => setOrdenarPor(e.target.value)}
                                className="w-full sm:w-auto bg-gray-50 border border-gray-100 rounded-xl px-5 py-3 text-[10px] font-black uppercase tracking-widest text-gray-500 outline-none focus:ring-4 focus:ring-patisserie-red/5 focus:border-patisserie-red transition-all cursor-pointer appearance-none pr-10"
                            >
                                <option value="nombre">Alfabetico (A-Z)</option>
                                <option value="precio-asc">Filtrar: Precio Bajo</option>
                                <option value="precio-desc">Filtrar: Precio Alto</option>
                            </select>
                            <div className="absolute right-4 top-1/2 -translate-y-1/2 pointer-events-none text-gray-400">
                                <FiFilter size={12} />
                            </div>
                        </div>

                        {/* Buscador */}
                        <div className="relative w-full sm:w-80">
                            <FiSearch className="absolute left-5 top-1/2 -translate-y-1/2 text-gray-400" />
                            <input
                                type="text"
                                placeholder="¿Qué delicia buscas hoje?..."
                                className="w-full pl-12 pr-6 py-3 bg-gray-50 border border-gray-100 rounded-xl text-xs font-bold focus:outline-none focus:ring-4 focus:ring-patisserie-red/5 focus:border-patisserie-red transition-all"
                                value={busqueda}
                                onChange={(e) => setBusqueda(e.target.value)}
                            />
                        </div>
                    </div>
                </div>

                {/* GRID DE PRODUCTOS */}
                <div className="transition-all duration-500 blur-0 scale-100">
                    {loading ? (
                        <ProductListSkeleton count={8} />
                    ) : productos.length === 0 ? (
                        <div className="text-center py-32 bg-white rounded-3xl border border-dashed border-gray-200 shadow-sm max-w-4xl mx-auto">
                            <div className="w-20 h-20 bg-gray-50 rounded-full flex items-center justify-center mx-auto mb-6">
                                <FiSearch className="text-3xl text-gray-300" />
                            </div>
                            <h3 className="text-2xl font-serif font-bold text-patisserie-dark mb-2">No pudimos encontrar delicias</h3>
                            <p className="text-gray-500 max-w-sm mx-auto mb-8">Hubo un problema al conectar con nuestra cocina o no hay productos que coincidan con tu búsqueda.</p>
                            <div className="flex flex-col sm:flex-row gap-4 justify-center">
                                <button
                                    onClick={() => { setCategoriaFiltro('Todos'); setBusqueda(''); fetchProductos(); }}
                                    className="px-8 py-3 bg-patisserie-dark text-white rounded-full font-bold hover:bg-patisserie-red hover:text-white transition-all uppercase tracking-widest text-xs"
                                >
                                    Reintentar conexión
                                </button>
                            </div>
                        </div>
                    ) : productosFiltrados.length === 0 ? (
                        <div className="text-center py-20 bg-white rounded-2xl border border-dashed border-gray-200">
                            <p className="text-xl text-gray-500 font-medium mb-4">No encontramos productos con esos criterios 😔</p>
                            <button
                                onClick={() => { setCategoriaFiltro('Todos'); setBusqueda(''); }}
                                className="text-patisserie-red font-bold hover:underline transition-colors uppercase tracking-widest text-sm"
                            >
                                Limpiar filtros y ver todo
                            </button>
                        </div>
                    ) : (
                        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4 md:gap-8">
                            {productosFiltrados.map((producto) => (
                                <ProductCard key={producto.id} product={producto} tiendaStatus={tiendaStatus} />
                            ))}
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default Catalogo;