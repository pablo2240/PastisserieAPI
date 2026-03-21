using Microsoft.EntityFrameworkCore;
using PastisserieAPI.Core.Entities;
using PastisserieAPI.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace PastisserieAPI.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context, ILogger logger)
        {
            try
            {
                logger.LogInformation("Verificando base de datos...");
                
                // Asegurar que la base de datos existe y tiene las migraciones aplicadas
                context.Database.Migrate();

                logger.LogInformation("✅ Base de datos verificada y migrada.");

                // Orden de ejecución de seeds (manteniendo integridad referencial)
                SeedRoles(context, logger);
                SeedCategorias(context, logger);
                SeedTiposMetodoPago(context, logger);
                SeedIngredientes(context, logger);
                SeedAdmin(context, logger);
                SeedProductos(context, logger);
                SeedConfiguracionTienda(context, logger);

                logger.LogInformation("✨ Inicialización de base de datos completada.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ocurrió un error al inicializar la base de datos.");
                throw;
            }
        }

        private static void SeedRoles(ApplicationDbContext context, ILogger logger)
        {
            if (context.Roles.Any()) return;

            var roles = new List<Rol>
            {
                new() { Id = 1, Nombre = "Usuario", Activo = true },
                new() { Id = 2, Nombre = "Admin", Activo = true },
                new() { Id = 3, Nombre = "Repartidor", Activo = true },
                new() { Id = 4, Nombre = "Gerente", Activo = true }
            };

            var strategy = context.Database.CreateExecutionStrategy();
            strategy.Execute(() =>
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Roles ON");
                    context.Roles.AddRange(roles);
                    context.SaveChanges();
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Roles OFF");
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            });
            
            logger.LogInformation("🔑 Roles iniciales agregados.");
        }

        private static void SeedTiposMetodoPago(ApplicationDbContext context, ILogger logger)
        {
            if (context.TiposMetodoPago.Any()) return;

            var metodos = new List<TipoMetodoPago>
            {
                new() { Id = 1, Nombre = "Efectivo", Descripcion = "Pago en efectivo contra entrega", Activo = true },
                new() { Id = 2, Nombre = "Tarjeta de Crédito", Descripcion = "Pago con tarjeta de crédito", Activo = true },
                new() { Id = 3, Nombre = "Tarjeta de Débito", Descripcion = "Pago con tarjeta de débito", Activo = true },
                new() { Id = 4, Nombre = "Transferencia", Descripcion = "Transferencia bancaria", Activo = true },
                new() { Id = 5, Nombre = "PSE", Descripcion = "Pago electrónico PSE", Activo = true }
            };

            var strategy = context.Database.CreateExecutionStrategy();
            strategy.Execute(() =>
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT TiposMetodoPago ON");
                    context.TiposMetodoPago.AddRange(metodos);
                    context.SaveChanges();
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT TiposMetodoPago OFF");
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            });

            logger.LogInformation("💳 Métodos de pago agregados.");
        }

        private static void SeedCategorias(ApplicationDbContext context, ILogger logger)
        {
            var categoriasEsperadas = new List<CategoriaProducto>
            {
                new() { Id = 1, Nombre = "Tortas", Descripcion = "Tortas y pasteles gourmet", Activa = true },
                new() { Id = 2, Nombre = "Panes", Descripcion = "Variedad de panes artesanales de masa madre", Activa = true },
                new() { Id = 3, Nombre = "Postres", Descripcion = "Postres finos y dulces de autor", Activa = true },
                new() { Id = 4, Nombre = "Galletas", Descripcion = "Galletas caseras y artesanales", Activa = true },
                new() { Id = 5, Nombre = "Bebidas", Descripcion = "Cafés de especialidad y bebidas naturales", Activa = true },
                new() { Id = 6, Nombre = "Salados", Descripcion = "Opciones saladas y quiches", Activa = true },
                new() { Id = 7, Nombre = "Promociones", Descripcion = "Combos y ofertas especiales", Activa = true }
            };

            var strategy = context.Database.CreateExecutionStrategy();
            strategy.Execute(() =>
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    bool changed = false;
                    foreach (var cat in categoriasEsperadas)
                    {
                        var existing = context.CategoriasProducto.FirstOrDefault(c => c.Id == cat.Id);
                        if (existing == null)
                        {
                            context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT CategoriasProducto ON");
                            context.CategoriasProducto.Add(cat);
                            context.SaveChanges();
                            context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT CategoriasProducto OFF");
                            changed = true;
                        }
                        else if (existing.Nombre != cat.Nombre)
                        {
                            existing.Nombre = cat.Nombre;
                            existing.Descripcion = cat.Descripcion;
                            changed = true;
                        }
                    }

                    if (changed)
                    {
                        context.SaveChanges();
                        transaction.Commit();
                        logger.LogInformation("📦 Categorías actualizadas/creadas.");
                    }
                    else
                    {
                        transaction.Rollback();
                        logger.LogInformation("✅ Categorías ya están al día.");
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    logger.LogError(ex, "Error seeding categories");
                    throw;
                }
            });
        }

        private static void SeedIngredientes(ApplicationDbContext context, ILogger logger)
        {
            if (context.Ingredientes.Any()) return;

            var ingredientes = new List<Ingrediente>
            {
                new() { Id = 1, Nombre = "Arequipe", Descripcion = "Relleno de arequipe", PrecioAdicional = 5000, Activo = true },
                new() { Id = 2, Nombre = "Crema de chocolate", Descripcion = "Crema de chocolate belga", PrecioAdicional = 7000, Activo = true },
                new() { Id = 3, Nombre = "Fresas frescas", Descripcion = "Fresas naturales", PrecioAdicional = 8000, Activo = true },
                new() { Id = 4, Nombre = "Frutas mixtas", Descripcion = "Variedad de frutas", PrecioAdicional = 10000, Activo = true }
            };

            var strategy = context.Database.CreateExecutionStrategy();
            strategy.Execute(() =>
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Ingredientes ON");
                    context.Ingredientes.AddRange(ingredientes);
                    context.SaveChanges();
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Ingredientes OFF");
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    logger.LogError(ex, "Error seeding ingredients");
                    throw;
                }
            });

            logger.LogInformation("🍓 Ingredientes iniciales agregados.");
        }

        private static void SeedAdmin(ApplicationDbContext context, ILogger logger)
        {
            var adminEmail = "administrador123@gmail.com";
            var existingAdmin = context.Users.FirstOrDefault(u => u.Email == adminEmail);
            var staticDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            
            int adminId;

            if (existingAdmin == null)
            {
                var admin = new User
                {
                    Id = 1,
                    Nombre = "Admin Deluxe",
                    Email = adminEmail,
                    PasswordHash = "$2a$11$uNPojHS7OzaaXFFXlSHQKOMdvw4QGeOWp9kxviN91XTgReEaZQlbG", 
                    EmailVerificado = true,
                    Activo = true,
                    FechaRegistro = staticDate,
                    FechaCreacion = staticDate
                };

                var strategy = context.Database.CreateExecutionStrategy();
                strategy.Execute(() =>
                {
                    using var transaction = context.Database.BeginTransaction();
                    try
                    {
                        context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Users ON");
                        context.Users.Add(admin);
                        context.SaveChanges();
                        context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Users OFF");

                        // Asignar Rol Admin (Id=2)
                        var userRol = new UserRol
                        {
                            UsuarioId = admin.Id,
                            RolId = 2, // Admin
                            FechaAsignacion = staticDate
                        };
                        context.UserRoles.Add(userRol);
                        context.SaveChanges();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        logger.LogError(ex, "Error seeding admin");
                        throw;
                    }
                });
                adminId = 1;
                logger.LogInformation("👤 Usuario Administrador creado.");
            }
            else
            {
                adminId = existingAdmin.Id;
                // Asegurar que tenga el rol de Admin
                if (!context.UserRoles.Any(ur => ur.UsuarioId == adminId && ur.RolId == 2))
                {
                    context.UserRoles.Add(new UserRol { UsuarioId = adminId, RolId = 2, FechaAsignacion = staticDate });
                    context.SaveChanges();
                    logger.LogInformation("🔑 Rol Admin asignado a usuario existente.");
                }
                logger.LogInformation("✅ Usuario Administrador ya existe.");
            }
        }

        private static void SeedProductos(ApplicationDbContext context, ILogger logger)
        {
            if (context.Productos.Any()) return;

            var staticDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var productos = new List<Producto>
            {
                // --- TORTAS (Cat 1) ---
                new() { Id = 1, Nombre = "Tarta Ópera Real", Descripcion = "Capas finas de bizcocho de almendra, crema de café y ganache de chocolate 70%.", Precio = 45000, Stock = 8, ImagenUrl = "https://images.unsplash.com/photo-1571115177098-24ec42ed204d?q=80&w=800", Categoria = "Tortas", EsPersonalizable = true, Activo = true, FechaCreacion = staticDate },
                new() { Id = 2, Nombre = "Cheesecake de Frutos del Bosque", Descripcion = "Base de galleta crujiente con crema de queso suave y coulis artesanal de moras.", Precio = 38000, Stock = 12, ImagenUrl = "https://images.unsplash.com/photo-1533134242443-d4fd215305ad?q=80&w=800", Categoria = "Tortas", EsPersonalizable = false, Activo = true, FechaCreacion = staticDate },
                new() { Id = 3, Nombre = "Mousse de Limón y Merengue", Descripcion = "Tarta refrescante con crema de limón siciliano y merengue suizo flameado.", Precio = 35000, Stock = 10, ImagenUrl = "https://images.unsplash.com/photo-1519915028121-7d3463d20b13?q=80&w=800", Categoria = "Tortas", EsPersonalizable = false, Activo = true, FechaCreacion = staticDate },
                new() { Id = 4, Nombre = "Pastel Red Velvet", Descripcion = "Clásico terciopelo rojo con capas de crema de queso frosting y un toque de vainilla.", Precio = 42000, Stock = 6, ImagenUrl = "https://images.unsplash.com/photo-1586788680434-30d324b2d46f?q=80&w=800", Categoria = "Tortas", EsPersonalizable = true, Activo = true, FechaCreacion = staticDate },
                new() { Id = 5, Nombre = "Tarta de Frutillas con Crema", Descripcion = "Bizcochos esponjosos rellenos de fresas frescas y crema Chantilly.", Precio = 34000, Stock = 15, ImagenUrl = "https://images.unsplash.com/photo-1565958011703-44f9829ba187?q=80&w=800", Categoria = "Tortas", EsPersonalizable = false, Activo = true, FechaCreacion = staticDate },

                // --- PANES (Cat 2) ---
                new() { Id = 6, Nombre = "Pan de Masa Madre (Sourdough)", Descripcion = "Pan artesanal de fermentación lenta (48h) con corteza rústica y miga aireada.", Precio = 6500, Stock = 20, ImagenUrl = "https://images.unsplash.com/photo-1585478259715-876a6a81fc08?q=80&w=800", Categoria = "Panes", EsPersonalizable = false, Activo = true, FechaCreacion = staticDate },
                new() { Id = 7, Nombre = "Brioche de Canela y Nuez", Descripcion = "Pan dulce francés enriquecido con mantequilla y espirales de canela premium.", Precio = 8000, Stock = 15, ImagenUrl = "https://images.unsplash.com/photo-1541119638723-c51cbe2262aa?q=80&w=800", Categoria = "Panes", EsPersonalizable = false, Activo = true, FechaCreacion = staticDate },
                new() { Id = 8, Nombre = "Croissants Clásicos (Pack x4)", Descripcion = "Hojaldre mantequilloso con múltiples capas, crujiente y dorado.", Precio = 12000, Stock = 30, ImagenUrl = "https://images.unsplash.com/photo-1509440159596-0249088772ff?q=80&w=800", Categoria = "Panes", EsPersonalizable = false, Activo = true, FechaCreacion = staticDate },
                new() { Id = 9, Nombre = "Baguette Tradicional", Descripcion = "La clásica baguette de corteza crujiente y aroma inigualable.", Precio = 3500, Stock = 40, ImagenUrl = "https://images.unsplash.com/photo-1530610476181-d83430b64dcd?q=80&w=800", Categoria = "Panes", EsPersonalizable = false, Activo = true, FechaCreacion = staticDate },
                new() { Id = 10, Nombre = "Focaccia de Romero y Sal", Descripcion = "Pan italiano esponjoso hidratado con aceite de oliva extra virgen.", Precio = 9000, Stock = 10, ImagenUrl = "https://images.unsplash.com/photo-1573140247632-f8fd7de9d720?q=80&w=800", Categoria = "Panes", EsPersonalizable = false, Activo = true, FechaCreacion = staticDate },

                // --- POSTRES (Cat 3) ---
                new() { Id = 11, Nombre = "Caja de Macarons Joya (x12)", Descripcion = "Surtido gourmet: Pistacho, Lavanda, Sal de Mar, y Chocolate Belga.", Precio = 25000, Stock = 25, ImagenUrl = "https://images.unsplash.com/photo-1569864358642-9d1619702661?q=80&w=800", Categoria = "Postres", EsPersonalizable = true, Activo = true, FechaCreacion = staticDate },
                new() { Id = 12, Nombre = "Éclairs de Avellana y Caramelo", Descripcion = "Masa choux rellena de praliné y cubierta con toffee artesanal.", Precio = 6500, Stock = 20, ImagenUrl = "https://images.unsplash.com/photo-1511018556340-d16986a1c194?q=80&w=800", Categoria = "Postres", EsPersonalizable = false, Activo = true, FechaCreacion = staticDate },
                new() { Id = 13, Nombre = "Tiramisú de Autor", Descripcion = "Café de especialidad, mascarpone italiano y cacao amargo de origen.", Precio = 15000, Stock = 10, ImagenUrl = "https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?q=80&w=800", Categoria = "Postres", EsPersonalizable = false, Activo = true, FechaCreacion = staticDate },

                // --- GALLETAS (Cat 4) ---
                new() { Id = 14, Nombre = "Cookies Red Velvet y Queso", Descripcion = "Galletas rojas aterciopeladas rellenas de crema de queso dulce.", Precio = 3500, Stock = 40, ImagenUrl = "https://images.unsplash.com/photo-1558961363-fa8fdf82db35?q=80&w=800", Categoria = "Galletas", EsPersonalizable = false, Activo = true, FechaCreacion = staticDate },
                new() { Id = 15, Nombre = "Shortbread de Vainilla Francesa", Descripcion = "Galletas de mantequilla pura que se deshacen en la boca.", Precio = 1500, Stock = 100, ImagenUrl = "https://images.unsplash.com/photo-1499636136210-6f4ee915583e?q=80&w=800", Categoria = "Galletas", EsPersonalizable = false, Activo = true, FechaCreacion = staticDate },
                new() { Id = 16, Nombre = "Cookies Doble Chocolate XXL", Descripcion = "Cargadas con trozos de chocolate blanco, con leche y amargo.", Precio = 4000, Stock = 35, ImagenUrl = "https://images.unsplash.com/photo-1490265246297-3ca9da565b9b?q=80&w=800", Categoria = "Galletas", EsPersonalizable = false, Activo = true, FechaCreacion = staticDate },

                // --- BEBIDAS (Cat 5) ---
                new() { Id = 17, Nombre = "Café Latte de Especialidad", Descripcion = "Granos de origen único con leche emulsionada y un toque de vainilla.", Precio = 4500, Stock = 50, ImagenUrl = "https://images.unsplash.com/photo-1541167760496-162955ed8a9f?q=80&w=800", Categoria = "Bebidas", EsPersonalizable = false, Activo = true, FechaCreacion = staticDate },
                new() { Id = 18, Nombre = "Té Matcha Ceremonial", Descripcion = "Té verde japonés de grado premium, preparado tradicionalmente.", Precio = 5500, Stock = 30, ImagenUrl = "https://images.unsplash.com/photo-1515823064-d6e0c04616a7?q=80&w=800", Categoria = "Bebidas", EsPersonalizable = false, Activo = true, FechaCreacion = staticDate },

                // --- SALADOS (Cat 6) ---
                new() { Id = 19, Nombre = "Quiche de Espinacas y Brie", Descripcion = "Masa quebrada artesanal rellena de espinacas frescas y queso brie fundido.", Precio = 12500, Stock = 10, ImagenUrl = "https://images.unsplash.com/photo-1551404973-7bb6af157822?q=80&w=800", Categoria = "Salados", EsPersonalizable = false, Activo = true, FechaCreacion = staticDate },
                new() { Id = 20, Nombre = "Empanada de Carne Cortada a Cuchillo", Descripcion = "La tradicional empanada con carne premium y cocción perfecta.", Precio = 3500, Stock = 60, ImagenUrl = "https://images.unsplash.com/photo-1552332386-f8dd00dc2f85?q=80&w=800", Categoria = "Salados", EsPersonalizable = false, Activo = true, FechaCreacion = staticDate },

                // --- PROMOCIONES (Cat 7) ---
                new() { Id = 21, Nombre = "Combo Desayuno Dulce", Descripcion = "1 Cappuccino Grande + 2 Croissants Clásicos + 1 Macaron de regalo.", Precio = 18500, Stock = 100, ImagenUrl = "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?q=80&w=800", Categoria = "Promociones", EsPersonalizable = false, Activo = true, FechaCreacion = staticDate }
            };

            var strategy = context.Database.CreateExecutionStrategy();
            strategy.Execute(() =>
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Productos ON");
                    context.Productos.AddRange(productos);
                    context.SaveChanges();
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Productos OFF");
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    logger.LogError(ex, "Error seeding products");
                    throw;
                }
            });

            logger.LogInformation("🍰 Catálogo completo de productos restaurado.");
        }

        private static void SeedConfiguracionTienda(ApplicationDbContext context, ILogger logger)
        {
            var existing = context.ConfiguracionTienda.FirstOrDefault(c => c.Id == 1);

            if (existing != null)
            {
                // Actualización idempotente: solo rellenar campos nuevos si están vacíos
                bool changed = false;

                if (string.IsNullOrEmpty(existing.HorarioApertura)) { existing.HorarioApertura = "08:00"; changed = true; }
                if (string.IsNullOrEmpty(existing.HorarioCierre))  { existing.HorarioCierre = "18:00"; changed = true; }
                if (existing.InstagramUrl == null) { existing.InstagramUrl = "https://www.instagram.com/pastisseriedeluxe"; changed = true; }
                if (existing.FacebookUrl == null)  { existing.FacebookUrl  = "https://www.facebook.com/pastisseriedeluxe"; changed = true; }
                if (existing.WhatsappUrl == null)  { existing.WhatsappUrl  = "https://wa.me/573001234567"; changed = true; }

                if (changed) context.SaveChanges();
                logger.LogInformation("⚙️ Configuración de tienda actualizada (campos nuevos).");
                return;
            }

            var config = new ConfiguracionTienda
            {
                Id = 1,
                NombreTienda = "Pâtisserie Deluxe",
                Direccion = "Calle 123 # 45 - 67, Bogotá",
                Telefono = "300 123 4567",
                EmailContacto = "contacto@pastisseriedeluxe.com",
                CostoEnvio = 5000,
                Moneda = "COP",
                MensajeBienvenida = "Bienvenido a la mejor pastelería artesanal",
                HorarioActivo = true,
                HorarioApertura = "08:00",
                HorarioCierre = "18:00",
                InstagramUrl = "https://www.instagram.com/pastisseriedeluxe",
                FacebookUrl = "https://www.facebook.com/pastisseriedeluxe",
                WhatsappUrl = "https://wa.me/573001234567",
                FechaActualizacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            var strategy = context.Database.CreateExecutionStrategy();
            strategy.Execute(() =>
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT ConfiguracionTienda ON");
                    context.ConfiguracionTienda.Add(config);
                    context.SaveChanges();
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT ConfiguracionTienda OFF");
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    logger.LogError(ex, "Error seeding store configuration");
                    throw;
                }
            });

            logger.LogInformation("⚙️ Configuración de tienda inicializada.");
        }
    }
}
