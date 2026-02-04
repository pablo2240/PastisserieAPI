PastisserieAPI/
├── PastisserieAPI.sln                    # Solución principal
├── .gitignore                            # Archivos ignorados por Git
├── README.md                             # Documentación principal
│
├── PastisserieAPI.API/                   # 🌐 Capa de Presentación (API)
│   ├── Controllers/                      # Controladores REST
│   │   ├── AuthController.cs             # Autenticación (login, register)
│   │   ├── ProductosController.cs        # CRUD de productos
│   │   └── CarritoController.cs          # Carrito de compras
│   ├── Database/                         # Scripts de base de datos
│   │   └── Scripts/
│   │       ├── 01_CreateAdminUser.sql    # Crear usuario administrador
│   │       ├── BCryptHashGenerator.cs    # Generador de hash BCrypt
│   │       └── README.md                 # Instrucciones de scripts
│   ├── Properties/
│   │   └── launchSettings.json           # Configuración de ejecución
│   ├── appsettings.json                  # Configuración (NO SUBIR A GIT)
│   ├── appsettings.Example.json          # Plantilla de configuración
│   ├── Program.cs                        # Punto de entrada de la API
│   └── PastisserieAPI.API.csproj         # Proyecto API
│
├── PastisserieAPI.Core/                  # 🎯 Capa de Dominio
│   ├── Entities/                         # Entidades del modelo
│   │   ├── User.cs                      # Usuario
│   │   ├── Producto.cs                  # Producto
│   │   ├── Pedido.cs                    # Pedido
│   │   ├── CarritoCompra.cs             # Carrito de compra
│   │   ├── Review.cs                    # Reseña
│   │   └── ...                           # (+20 entidades)
│   ├── Enums/                            # Enumeraciones
│   │   ├── EstadoPedido.cs
│   │   ├── EstadoEnvio.cs
│   │   ├── TipoNotificacion.cs
│   │   └── TipoRol.cs
│   ├── Interfaces/                       # Contratos
│   │   ├── IUnitOfWork.cs               # Unit of Work
│   │   └── Repositories/
│   │       ├── IRepository.cs           # Repositorio genérico
│   │       ├── IUserRepository.cs
│   │       ├── IProductoRepository.cs
│   │       └── ...                       # (+7 interfaces)
│   └── PastisserieAPI.Core.csproj
│
├── PastisserieAPI.Infrastructure/        # 🗄️ Capa de Infraestructura
│   ├── Data/
│   │   ├── ApplicationDbContext.cs       # DbContext principal
│   │   ├── ApplicationDbContextFactory.cs# Factory para migraciones
│   │   └── Configurations/               # Fluent API
│   │       ├── UserConfiguration.cs
│   │       ├── ProductoConfiguration.cs
│   │       └── PedidoConfiguration.cs
│   ├── Migrations/                       # Migraciones EF Core
│   │   ├── 20260201XXXXXX_InitialCreate.cs
│   │   └── ApplicationDbContextModelSnapshot.cs
│   ├── Repositorie/                      # Implementaciones
│   │   ├── Repository.cs                # Repositorio genérico
│   │   ├── UnitOfWork.cs                # Unit of Work
│   │   ├── UserRepository.cs
│   │   ├── ProductoRepository.cs
│   │   └── ...                           # (+7 repositorios)
│   └── PastisserieAPI.Infrastructure.csproj
│
└── PastisserieAPI.Services/              # 💼 Capa de Aplicación
    ├── DTOs/                             # Data Transfer Objects
    │   ├── Common/
    │   │   ├── ApiResponse.cs            # Respuesta estándar
    │   │   └── PaginationDto.cs          # Paginación
    │   ├── Request/
    │   │   ├── RegisterRequestDto.cs
    │   │   ├── LoginRequestDto.cs
    │   │   ├── CreateProductoRequestDto.cs
    │   │   ├── CreatePedidoRequestDto.cs
    │   │   └── ...                       # (+15 DTOs)
    │   └── Response/
    │       ├── UserResponseDto.cs
    │       ├── LoginResponseDto.cs
    │       ├── ProductoResponseDto.cs
    │       └── ...                       # (+10 DTOs)
    ├── Validators/                       # FluentValidation
    │   ├── RegisterRequestValidator.cs
    │   ├── LoginRequestValidator.cs
    │   ├── CreateProductoRequestValidator.cs
    │   └── ...                           # (+8 validadores)
    ├── Mappings/
    │   └── MappingProfile.cs             # AutoMapper
    ├── Services/
    │   ├── Interfaces/
    │   │   ├── IAuthService.cs
    │   │   ├── IProductoService.cs
    │   │   ├── IPedidoService.cs
    │   │   └── ICarritoService.cs
    │   ├── AuthService.cs                # Autenticación
    │   ├── ProductoService.cs            # Productos
    │   ├── PedidoService.cs              # Pedidos
    │   └── CarritoService.cs             # Carrito
    ├── Helpers/
    │   └── JwtHelper.cs                  # JWT
    └── PastisserieAPI.Services.csproj
