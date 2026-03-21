import React from 'react';
import { createPortal } from 'react-dom';
import { Link } from 'react-router-dom';
import { type Producto } from '../types';
import { useCart } from '../context/CartContext';
import { FiPlus, FiClock, FiX } from 'react-icons/fi';
import { formatCurrency } from '../utils/format';
import { type TiendaStatus } from '../hooks/useTiendaStatus';

interface ProductCardProps {
  product: Producto;
  tiendaStatus?: TiendaStatus | null;
}

const ProductCard: React.FC<ProductCardProps> = ({ product, tiendaStatus }) => {
  const { addToCart } = useCart();
  const [showHoursModal, setShowHoursModal] = React.useState(false);

  const status = tiendaStatus;

  const isClosed = status && !status.estaAbierto;

  const diasNombres = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'];

  return (
    <>
      <div className="bg-white rounded-2xl shadow-[0_4px_20px_rgba(0,0,0,0.03)] hover:shadow-[0_10px_40px_rgba(0,0,0,0.08)] transition-all duration-500 overflow-hidden flex flex-col h-full border border-gray-100 group hover:-translate-y-1">

        {/* Imagen (Ahora con Link) */}
        <Link to={`/productos/${product.id}`} className="h-48 overflow-hidden relative block">
          <img
            src={product.imagenUrl || 'https://via.placeholder.com/300x200?text=Sin+Imagen'}
            alt={product.nombre}
            loading="lazy"
            className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-110 hover-expand"
          />
          {/* Badges */}
          <div className="absolute top-4 left-4 flex flex-col gap-2 z-10">
            <span className="bg-white/90 backdrop-blur-md px-3 py-1 text-[10px] font-bold rounded-full uppercase tracking-[0.2em] text-patisserie-red shadow-sm border border-patisserie-red/20">
              {product.categoria}
            </span>
            {product.stock === 0 && (
              <span className="bg-red-600 text-white px-3 py-1 text-[10px] font-bold rounded-full uppercase tracking-[0.2em] shadow-lg animate-pulse">
                Sin Stock
              </span>
            )}
          </div>
        </Link>

        {/* Contenido */}
        <div className="p-4 flex flex-col flex-grow">
          {/* Título (Ahora con Link) */}
          <Link to={`/productos/${product.id}`}>
            <h3 className="font-serif font-bold text-xl mb-1 line-clamp-1 text-patisserie-dark group-hover:text-patisserie-red transition-colors">
              {product.nombre}
            </h3>
          </Link>

          {/* Descripción corta (Opcional, si el diseño lo permite) */}
          {product.descripcion && (
            <p className="text-sm text-gray-500 mb-3 line-clamp-2">{product.descripcion}</p>
          )}

          <div className="mt-auto pt-3">
            <p className="text-patisserie-red font-bold text-2xl mb-4">
              {formatCurrency(product.precio)}
            </p>

            <button
              onClick={(e) => {
                e.preventDefault();
                if (isClosed) {
                  setShowHoursModal(true);
                  return;
                }
                if (product.stock === 0) return;

                addToCart(product.id);
                // Add a quick success pop effect
                const btn = e.currentTarget;
                btn.classList.add('success-pop');
                setTimeout(() => btn.classList.remove('success-pop'), 400);
              }}
              disabled={product.stock === 0 && !isClosed}
              className={`w-full py-3.5 rounded-xl font-bold text-sm flex items-center justify-center gap-2 transition-all duration-300 btn-premium uppercase tracking-widest ${isClosed ? 'bg-patisserie-red text-white hover:bg-patisserie-dark cursor-pointer' : 'bg-patisserie-dark text-white hover:bg-patisserie-red disabled:bg-gray-200 disabled:text-gray-400 disabled:cursor-not-allowed'}`}
            >
              {product.stock === 0 && !isClosed ? 'Agotado' : isClosed ? <><FiClock className="text-lg" /> Ver Horarios</> : <><FiPlus className="text-lg" /> Agregar</>}
            </button>
          </div>
        </div>
      </div>

      {showHoursModal && createPortal(
        <div className="fixed inset-0 z-[2000] flex items-center justify-center bg-[#5D1919]/20 backdrop-blur-md animate-fade-in p-4" onClick={() => setShowHoursModal(false)} role="dialog">
          <div className="bg-white rounded-[2.5rem] shadow-2xl w-full max-w-md overflow-hidden animate-scale-in border border-gray-100" onClick={e => e.stopPropagation()}>
            <div className="bg-[#7D2121] text-white p-8 relative">
              <button
                onClick={() => setShowHoursModal(false)}
                className="absolute top-6 right-6 text-white/60 hover:text-white transition-colors"
              >
                <FiX size={24} />
              </button>
              <div className="flex items-center gap-3 mb-2">
                <FiClock size={28} />
                <h3 className="font-serif font-black text-2xl tracking-tight">Horarios de Operación</h3>
              </div>
              <p className="text-[10px] font-black text-white/70 uppercase tracking-widest">La tienda se encuentra fuera de servicio</p>
            </div>

            <div className="p-8">
              {!status?.usarControlHorario && !status?.sistemaActivoManual ? (
                <div className="text-center py-10 px-4 bg-rose-50 rounded-3xl border border-rose-100">
                  <p className="text-rose-600 font-black uppercase tracking-widest text-xs">Mantenimiento Temporal</p>
                  <p className="text-[10px] text-rose-400 mt-2 font-bold uppercase tracking-widest">El sistema ha sido desactivado manualmente por administración.</p>
                </div>
              ) : (status?.horariosPorDia && status.horariosPorDia.length > 0) ? (
                <div className="space-y-2 max-h-[60vh] overflow-y-auto pr-2 custom-scrollbar">
                  {status.horariosPorDia.map(h => {
                    const isHoy = new Date().getDay() === h.diaSemana;
                    return (
                      <div key={h.diaSemana} className={`flex justify-between items-center p-4 rounded-2xl transition-all ${isHoy ? 'bg-[#7D2121] text-white shadow-xl shadow-[#7D2121]/10' : 'bg-gray-50/50 border border-transparent hover:border-gray-100'}`}>
                        <div className="flex items-center gap-3">
                          <span className="text-[10px] font-black uppercase">{diasNombres[h.diaSemana]}</span>
                          {isHoy && <div className="px-2 py-0.5 bg-white/20 rounded-full text-[8px] font-black uppercase tracking-tighter">Hoy</div>}
                        </div>
                        <span className={`text-[11px] font-black tabular-nums ${isHoy ? 'text-white' : (h.abierto ? 'text-gray-800' : 'text-rose-400')}`}>
                          {h.abierto ? `${h.horaApertura} a ${h.horaCierre}` : 'No Laborable'}
                        </span>
                      </div>
                    )
                  })}
                </div>
              ) : (
                <div className="text-center py-12 border border-dashed border-gray-200 rounded-[2.5rem]">
                  <FiClock size={32} className="mx-auto text-gray-200 mb-4" />
                  <p className="text-[10px] text-gray-400 font-black uppercase tracking-widest">Cerrado temporalmente</p>
                </div>
              )}
            </div>

            <button
              onClick={() => setShowHoursModal(false)}
              className="w-[calc(100%-4rem)] mx-8 mb-8 py-4 bg-gray-900 text-white font-black uppercase tracking-[0.2em] text-[10px] rounded-2xl hover:bg-[#7D2121] transition-all"
            >
              Comprendido
            </button>
          </div>
        </div>,
        document.body
      )}
    </>
  );
};

export default ProductCard;