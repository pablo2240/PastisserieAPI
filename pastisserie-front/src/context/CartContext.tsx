import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import { cartService, type AddToCartRequest } from '../services/cartService';
import type { Carrito } from '../types';
import toast from 'react-hot-toast';
import { useAuth } from './AuthContext';
import api from '../api/axios';

interface CartContextType {
  carrito: Carrito | null;
  totalItems: number;
  cartTotal: number;
  isLoading: boolean;
  addToCart: (productoId: number, cantidad?: number) => Promise<void>;
  updateCartItem: (itemId: number, cantidad: number) => Promise<void>;
  removeFromCart: (itemId: number) => Promise<void>;
  clearCart: () => Promise<void>;
}

const CartContext = createContext<CartContextType | undefined>(undefined);

export const CartProvider = ({ children }: { children: ReactNode }) => {
  const [carrito, setCarrito] = useState<Carrito | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const { isAuthenticated } = useAuth(); // Usamos tu hook de autenticación

  // Cargar carrito al iniciar o al loguearse
  useEffect(() => {
    if (isAuthenticated) {
      loadCart();
    } else {
      setCarrito(null);
    }
  }, [isAuthenticated]);

  const loadCart = async () => {
    try {
      const response = await cartService.getCart();
      // Verificamos si la respuesta fue exitosa (tu backend usa 'succeeded' o 'success')
      if (response.success) {
        setCarrito(response.data);
      }
    } catch (error) {
      console.error('Error cargando carrito:', error);
    }
  };

  // Calculamos totales
  const totalItems = carrito?.items?.reduce((acc, item) => acc + item.cantidad, 0) || 0;
  const cartTotal = carrito?.total || 0;

  // 1. Agregar al carrito
  const addToCart = async (productoId: number, cantidad: number = 1) => {
    if (!isAuthenticated) {
      toast.error('Debes iniciar sesión para comprar');
      return;
    }

    // Verificar si la tienda está abierta antes de agregar
    try {
      const statusResp = await api.get('/tienda/estado');
      const storeStatus = statusResp.data?.data;
      if (storeStatus && !storeStatus.estaAbierto) {
        const format12h = (t: string) => {
          if (!t) return '';
          const [h, m] = t.split(':');
          const hi = parseInt(h, 10);
          return `${hi % 12 || 12}:${m} ${hi >= 12 ? 'PM' : 'AM'}`;
        };
        const apertura = storeStatus.horaApertura ? format12h(storeStatus.horaApertura) : '';
        const cierre = storeStatus.horaCierre ? format12h(storeStatus.horaCierre) : '';
        const horarioStr = apertura && cierre ? `${apertura} – ${cierre}` : 'nuestro horario laboral';

        toast.error(
          () => (
            <div className="flex flex-col gap-1">
              <span className="font-black text-rose-600 uppercase tracking-widest text-[10px]">Tienda Cerrada</span>
              <span className="text-xs font-medium text-gray-600">
                Lo sentimos, solo aceptamos pedidos en {horarioStr}.
              </span>
            </div>
          ),
          { duration: 5000, icon: '🕑' }
        );
        return;
      }
    } catch {
      // Si no podemos verificar el estado, permitimos continuar
    }

    setIsLoading(true);
    try {
      const request: AddToCartRequest = { productoId, cantidad };
      const response = await cartService.addToCart(request);

      if (response.success) {
        toast.success('Agregado al carrito 🛒');
        setCarrito(response.data);
      } else {
        toast.error(response.message || 'No se pudo agregar');
      }
    } catch (error) {
      console.error(error);
      toast.error('Error al agregar producto');
    } finally {
      setIsLoading(false);
    }
  };

  // 2. Actualizar cantidad
  const updateCartItem = async (itemId: number, cantidad: number) => {
    if (cantidad <= 0) {
      await removeFromCart(itemId);
      return;
    }

    setIsLoading(true);
    try {
      const response = await cartService.updateCartItem(itemId, { cantidad });
      if (response.success) {
        setCarrito(response.data); // Actualizamos con el carrito nuevo
      }
    } catch (error) {
      toast.error('Error al actualizar cantidad');
    } finally {
      setIsLoading(false);
    }
  };

  // 3. Eliminar ítem
  const removeFromCart = async (itemId: number) => {
    setIsLoading(true);
    try {
      const response = await cartService.removeFromCart(itemId);
      if (response.success) {
        toast.success('Producto eliminado');
        await loadCart(); // Recargamos el carrito completo
      }
    } catch (error) {
      toast.error('Error al eliminar producto');
    } finally {
      setIsLoading(false);
    }
  };

  // 4. Vaciar carrito completo
  const clearCart = async () => {
    setIsLoading(true);
    try {
      const response = await cartService.clearCart();
      if (response.success) {
        setCarrito(null); // Limpiamos estado local
        toast.success('Carrito vaciado');
      }
    } catch (error) {
      toast.error('Error al vaciar el carrito');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <CartContext.Provider value={{
      carrito,
      totalItems,
      cartTotal,
      isLoading,
      addToCart,
      updateCartItem,
      removeFromCart,
      clearCart
    }}>
      {children}
    </CartContext.Provider>
  );
};

export const useCart = () => {
  const context = useContext(CartContext);
  if (context === undefined) {
    throw new Error('useCart debe usarse dentro de un CartProvider');
  }
  return context;
};