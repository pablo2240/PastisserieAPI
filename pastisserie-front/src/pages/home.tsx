import { useEffect, useState } from 'react';
import Hero from '../components/Hero';
import ProductCard from '../components/ProductCard';
import { productService } from '../services/productService';
import { reviewService, type Review } from '../services/reviewService';
import { type Producto } from '../types';
import { FaStar, FaQuoteLeft, FaLeaf, FaShippingFast, FaAward } from 'react-icons/fa';
import LoadingScreen from '../components/LoadingScreen';
import { useTiendaStatus } from '../hooks/useTiendaStatus';
import { Link } from 'react-router-dom';

const Home = () => {
  const { status: tiendaStatus } = useTiendaStatus();
  const [featuredProducts, setFeaturedProducts] = useState<Producto[]>([]);
  const [featuredReviews, setFeaturedReviews] = useState<Review[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        // 1. Cargar Productos Destacados
        const prodResponse = await productService.getAll();

        // Robust check for data (handling both wrapped and unwrapped responses)
        const products = prodResponse?.data?.data ||
          prodResponse?.data ||
          prodResponse?.result ||
          prodResponse || [];

        if (Array.isArray(products)) {
          setFeaturedProducts(products.slice(0, 4));
        }

        // 2. Cargar Reseñas
        const reviewsData = await reviewService.getFeatured();
        if (reviewsData && reviewsData.length > 0) {
          setFeaturedReviews(reviewsData.slice(0, 3));
        }

      } catch (error) {
        console.error("Error cargando datos del home", error);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, []);

  if (loading) {
    return <LoadingScreen />;
  }

  return (
    <div className="animate-fade-in bg-gray-50">
      <Hero />

      {/* --- FEATURES STRIP --- */}
      <section className="bg-white py-12 shadow-sm mb-16 px-4">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8 text-center divide-y md:divide-y-0 md:divide-x divide-gray-100 max-w-6xl mx-auto items-start">
          <div className="pt-8 md:pt-0 md:px-8 group">
            <div className="w-16 h-16 bg-patisserie-red/5 rounded-2xl flex items-center justify-center mx-auto mb-6 transition-all duration-300">
              <FaLeaf className="text-3xl text-patisserie-red" />
            </div>
            <h3 className="font-serif font-black text-gray-900 mb-3 text-xl tracking-tight">Cosecha Natural</h3>
            <p className="text-sm text-gray-500 leading-relaxed font-medium">Ingredientes seleccionados de cultivos locales, sin conservantes artificiales.</p>
          </div>
          <div className="pt-8 md:pt-0 md:px-8 group">
            <div className="w-16 h-16 bg-patisserie-red/5 rounded-2xl flex items-center justify-center mx-auto mb-6 transition-all duration-300">
              <FaAward className="text-3xl text-patisserie-red" />
            </div>
            <h3 className="font-serif font-black text-gray-900 mb-3 text-xl tracking-tight">Maestría Artesana</h3>
            <p className="text-sm text-gray-500 leading-relaxed font-medium">Técnicas ancestrales francesas aplicadas con precisión en cada creación.</p>
          </div>
          <div className="pt-8 md:pt-0 md:px-8 group">
            <div className="w-16 h-16 bg-patisserie-red/5 rounded-2xl flex items-center justify-center mx-auto mb-6 transition-all duration-300">
              <FaShippingFast className="text-3xl text-patisserie-red" />
            </div>
            <h3 className="font-serif font-black text-gray-900 mb-3 text-xl tracking-tight">Logística Deluxe</h3>
            <p className="text-sm text-gray-500 leading-relaxed font-medium">Transporte climatizado y embalaje premium para conservar la frescura intacta.</p>
          </div>
        </div>
      </section>

      {/* --- PRODUCTOS DESTACADOS --- */}
      <section className="py-16 container mx-auto px-6">
        <div className="flex flex-col md:flex-row justify-between items-end mb-12 gap-6">
          <div className="text-left space-y-2">
            <span className="text-patisserie-red font-black uppercase tracking-[0.4em] text-[10px] bg-red-50 px-4 py-1.5 rounded-full">Edición Limitada</span>
            <h2 className="text-6xl font-serif font-black text-patisserie-dark tracking-tighter">Nuestras Joyas</h2>
          </div>
          <Link to="/productos" className="group flex items-center gap-3 text-gray-400 hover:text-patisserie-red font-black transition-all uppercase tracking-widest text-xs">
            <span>Explorar catálogo</span>
            <div className="w-8 h-px bg-gray-200 group-hover:w-12 group-hover:bg-patisserie-red transition-all" />
          </Link>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-12">
          {featuredProducts.length > 0 ? (
            featuredProducts.map((product) => (
              <ProductCard key={product.id} product={product} tiendaStatus={tiendaStatus} />
            ))
          ) : (
            [1, 2, 3, 4].map(i => (
              <div key={i} className="animate-pulse bg-gray-100 h-96 rounded-[2rem]"></div>
            ))
          )}
        </div>
      </section>

      {/* --- HISTORIA --- */}
      <section className="py-16 bg-white overflow-hidden">
        <div className="container mx-auto px-6 max-w-6xl">
          <div className="flex flex-col lg:flex-row items-center gap-16">
            <div className="lg:w-1/2 relative">
              <div className="rounded-[2.5rem] overflow-hidden shadow-2xl relative z-10 w-full">
                <img src="https://images.unsplash.com/photo-1556910103-1c02745a30bf?auto=format&fit=crop&q=80&w=800" alt="Chef decorando" className="w-full h-auto max-h-[500px] object-cover" />
              </div>
              <div className="absolute -bottom-10 -right-10 w-3/5 rounded-[2rem] overflow-hidden shadow-xl border-8 border-white hidden lg:block z-20">
                <img src="https://images.unsplash.com/photo-1612203985729-70726954388c?auto=format&fit=crop&q=80&w=400" alt="Detalle donas" />
              </div>
            </div>

            <div className="lg:w-1/2 space-y-8">
              <span className="text-patisserie-red font-black uppercase tracking-[0.4em] text-[10px]">L'Héritage</span>
              <h2 className="text-6xl font-serif font-black text-patisserie-dark leading-none tracking-tighter">Pasión por la <br /><span className="text-patisserie-red italic underline decoration-1 underline-offset-8">Alta Repostería</span></h2>
              <p className="text-gray-500 leading-loose text-lg font-medium">
                En Patisserie Deluxe, cada amanecer es una oportunidad para honrar la tradición. Seleccionamos los granos de cacao más finos y la mantequilla más pura para crear experiencias que trascienden el gusto.
              </p>
              <p className="text-gray-400 leading-loose text-base italic border-l-4 border-patisserie-red pl-6 py-2">
                "No servimos postres; creamos momentos de felicidad que perduran en el recuerdo de nuestros clientes."
              </p>
              <Link to="/contacto" className="inline-flex items-center gap-4 bg-patisserie-dark text-white py-5 px-12 rounded-full hover:bg-patisserie-red transition-all font-black shadow-2xl hover:shadow-patisserie-red/30 uppercase tracking-widest text-xs group">
                Descubre nuestra historia
                <div className="w-2 h-2 bg-white rounded-full group-hover:scale-150 transition-transform" />
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* --- TESTIMONIOS --- */}
      <section className="py-16 bg-gray-50/50">
        <div className="container mx-auto px-6">
          <div className="text-center mb-12 space-y-4">
            <span className="text-patisserie-red font-black uppercase tracking-[0.4em] text-[10px]">Experiencias Reales</span>
            <h2 className="text-6xl font-serif font-black tracking-tighter">Voces del Gourmet</h2>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {(featuredReviews.length > 0 ? featuredReviews : [
              { id: 101, comentario: "Los pasteles son simplemente increíbles. La calidad y el sabor superan todas mis expectativas.", nombre: "María González", fecha: new Date(), calificacion: 5 },
              { id: 102, comentario: "Perfectos para eventos especiales. Hemos pedido tortas para cumpleaños y siempre quedan espectaculares.", nombre: "Carlos Rodríguez", fecha: new Date(), calificacion: 5 },
              { id: 103, comentario: "Las galletas artesanales son mi debilidad. La textura es imposible de resistir.", nombre: "Ana Silva", fecha: new Date(), calificacion: 5 }
            ]).map((review: any) => (
              <div key={review.id} className="bg-white p-8 rounded-2xl shadow-sm hover:shadow-xl transition-all duration-300 transform hover:-translate-y-1 border border-gray-100 relative group">
                <FaQuoteLeft className="text-4xl text-gray-100 absolute top-6 right-6 group-hover:text-red-50 transition-colors" />

                <div className="flex text-yellow-400 mb-6">
                  {[...Array(5)].map((_, i) => (
                    <FaStar key={i} className={i < review.calificacion ? "text-[#F5B041]" : "text-gray-200"} />
                  ))}
                </div>

                <p className="text-gray-600 text-lg mb-8 italic leading-relaxed relative z-10">"{review.comentario}"</p>

                <div className="flex items-center gap-4 mt-auto border-t border-gray-50 pt-4">
                  <div className="w-12 h-12 bg-gray-100 rounded-full flex items-center justify-center text-[#7D2121] font-bold text-lg">
                    {review.nombre ? review.nombre.substring(0, 1) : (review.usuarioNombre ? review.usuarioNombre.substring(0, 1) : 'C')}
                  </div>
                  <div>
                    <p className="font-bold text-gray-900">{review.nombre || review.usuarioNombre}</p>
                    <p className="text-xs text-gray-400 font-medium uppercase tracking-wide">Cliente Verificado</p>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* --- NEWSLETTER CTA --- */}
      <section className="py-12 bg-gray-900 text-white text-center px-6">
        <div className="max-w-2xl mx-auto">
          <h2 className="text-3xl font-serif font-bold mb-4">Únete al Club Deluxe</h2>
          <p className="text-gray-400 mb-8">Recibe ofertas exclusivas y sé el primero en probar nuestras nuevas creaciones.</p>
          <form className="flex flex-col sm:flex-row gap-4 max-w-md mx-auto">
            <input type="email" placeholder="Tu correo electrónico" className="flex-1 px-6 py-4 rounded-full bg-white/5 border border-white/10 text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-patisserie-red/40 focus:border-patisserie-red transition-all" />
            <button type="button" className="px-10 py-4 bg-patisserie-red text-white font-bold rounded-full hover:bg-white hover:text-patisserie-dark transition-all shadow-lg hover:shadow-patisserie-red/20 uppercase tracking-widest text-xs">Suscribirse</button>
          </form>
        </div>
      </section>
    </div>
  );
};

export default Home;
