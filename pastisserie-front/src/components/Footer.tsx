import { useEffect, useState } from 'react';
import { FaFacebookF, FaInstagram, FaWhatsapp } from 'react-icons/fa';
import { configuracionService, type ConfiguracionTienda } from '../services/configuracionService';

const Footer = () => {
  const [config, setConfig] = useState<ConfiguracionTienda | null>(null);

  useEffect(() => {
    const fetchConfig = async () => {
      try {
        const response = await configuracionService.getConfig();
        if (response.success) {
          setConfig(response.data);
        }
      } catch (error) {
        console.error("Error loading footer config", error);
      }
    };
    fetchConfig();
  }, []);

  return (
    <footer className="bg-patisserie-dark text-white pt-20 pb-10 border-t border-patisserie-red/20 overflow-hidden">
      <div className="container mx-auto px-4 grid grid-cols-1 md:grid-cols-3 gap-12">

        <div className="animate-fade-in">
          <h3 className="font-serif font-bold text-2xl mb-8 text-patisserie-red tracking-tight">Información de Contacto</h3>
          <ul className="space-y-4 text-sm opacity-90">
            <li className="flex items-start gap-3 group">
              <span className="bg-patisserie-red/10 p-2 rounded-lg text-patisserie-red group-hover:bg-patisserie-red group-hover:text-white transition-colors">📍</span>
              <span className="pt-1">{config?.direccion || 'Av. Principal 123, Centro'}<br /><span className="text-gray-400 text-xs italic">Sede Principal</span></span>
            </li>
            <li className="flex items-center gap-3 group">
              <span className="bg-patisserie-red/10 p-2 rounded-lg text-patisserie-red group-hover:bg-patisserie-red group-hover:text-white transition-colors">📞</span>
              <span className="pt-1 font-bold">{config?.telefono || '+1 (555) 123-4567'}</span>
            </li>
            <li className="flex items-start gap-3 group">
              <span className="bg-patisserie-red/10 p-2 rounded-lg text-patisserie-red group-hover:bg-patisserie-red group-hover:text-white transition-colors">🕒</span>
              <span className="pt-1">
                {config?.horarioActivo ? (
                  <>
                    <span className="text-green-400 font-bold block mb-1">Horario Laboral:</span>
                    {config.horarioApertura} a {config.horarioCierre}
                  </>
                ) : (
                  <>
                    Lun - Vie: 8:00 AM - 6:00 PM<br />
                    Sáb - Dom: 9:00 AM - 2:00 PM
                  </>
                )}
              </span>
            </li>
          </ul>
        </div>

        <div className="animate-fade-in delay-100">
          <h3 className="font-serif font-bold text-2xl mb-8 text-patisserie-red tracking-tight">VIVE LA EXPERIENCIA</h3>
          <p className="text-sm opacity-90 leading-relaxed italic border-l-2 border-patisserie-red/30 pl-4 py-2 bg-white/5 rounded-r-xl">
            &quot;Desde 1995, creamos con pasión productos artesanales únicos.
            Utilizamos ingredientes premium para deleitar tus sentidos en cada bocado.&quot;
          </p>
          <div className="mt-8 flex gap-2">
            <span className="text-[10px] font-black uppercase tracking-[0.2em] text-patisserie-red bg-patisserie-red/10 px-3 py-1 rounded-full">Artesanal</span>
            <span className="text-[10px] font-black uppercase tracking-[0.2em] text-white/40 bg-white/5 px-3 py-1 rounded-full">Premium</span>
          </div>
        </div>

        <div className="animate-fade-in delay-200">
          <h3 className="font-serif font-bold text-2xl mb-8 text-patisserie-red tracking-tight">Síguenos</h3>
          <div className="flex space-x-4 mb-8">
            {config?.facebookUrl && (
              <a href={config.facebookUrl} target="_blank" rel="noopener noreferrer" className="w-12 h-12 bg-white/5 border border-white/10 flex items-center justify-center rounded-2xl hover:bg-patisserie-red hover:text-white hover:-translate-y-1 transition-all shadow-lg active:scale-90 group">
                <FaFacebookF size={20} className="group-hover:scale-110 transition-transform" />
              </a>
            )}
            {config?.instagramUrl && (
              <a href={config.instagramUrl} target="_blank" rel="noopener noreferrer" className="w-12 h-12 bg-white/5 border border-white/10 flex items-center justify-center rounded-2xl hover:bg-patisserie-red hover:text-white hover:-translate-y-1 transition-all shadow-lg active:scale-90 group">
                <FaInstagram size={20} className="group-hover:scale-110 transition-transform" />
              </a>
            )}
            {config?.whatsappUrl && (
              <a href={`https://wa.me/${config.whatsappUrl}`} target="_blank" rel="noopener noreferrer" className="w-12 h-12 bg-white/5 border border-white/10 flex items-center justify-center rounded-2xl hover:bg-green-600 hover:text-white hover:-translate-y-1 transition-all shadow-lg active:scale-90 group">
                <FaWhatsapp size={20} className="group-hover:scale-110 transition-transform" />
              </a>
            )}
          </div>
          <p className="text-xs opacity-75 font-medium tracking-tight">Comparte tus fotos usando <span className="text-patisserie-red font-bold">#PatisserieDeluxe</span></p>
          <div className="mt-6 p-4 bg-patisserie-red/5 rounded-[20px] border border-patisserie-red/10">
            <p className="text-[10px] font-black text-patisserie-red uppercase tracking-widest mb-1">Pagos Seguros</p>
            <div className="flex gap-2 opacity-30 grayscale hover:grayscale-0 transition-all">
              <span className="text-xl">💳</span>
              <span className="text-xl">🏦</span>
              <span className="text-xl">📱</span>
            </div>
          </div>
        </div>
      </div>

      <div className="container mx-auto px-4 mt-20 pt-8 border-t border-white/5 flex flex-col md:flex-row justify-between items-center gap-4 text-[10px] font-black uppercase tracking-[0.3em] opacity-40">
        <p>&copy; {new Date().getFullYear()} Patisseries Deluxe</p>
        <p className="flex gap-6">
          <a href="#" className="hover:text-patisserie-red transition-colors">Términos</a>
          <a href="#" className="hover:text-patisserie-red transition-colors">Privacidad</a>
          <a href="#" className="hover:text-patisserie-red transition-colors">Cookies</a>
        </p>
      </div>
    </footer>
  );
};

export default Footer;
