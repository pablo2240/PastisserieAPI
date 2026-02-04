PastisserieAPI/
├── PastisserieAPI.sln                    # Solución principal
├── .gitignore                            # Archivos ignorados por Git
├── README.md                             # Este archivo
│
├── PastisserieAPI.API/                   # 🌐 Capa de Presentación
│   ├── Controllers/                      # Controladores REST
│   │   ├── AuthController.cs            # Autenticación (login, register)
│   │   ├── ProductosController.cs       # CRUD de productos
│   │   └── CarritoController.cs         # Carrito de compras
│   ├── Database/                         # Scripts de base de datos
│   │   └── Scripts/
│   │       ├── 01_CreateAdminUser.sql   # Crear administrador
│   │       ├── BCryptHashGenerator.cs   # Generador de hash
│   │       └── README.md                # Instrucciones detalladas
│   ├── Properties/
│   │   └── launchSettings.json          # Configuración de ejecución
│   ├── appsettings.json                 # Configuración (NO SUBIR A GIT)
│   ├── appsettings.Example.json         # Plantilla de configuración
│   ├── Program.cs                       # Punto de entrada
│   └── PastisserieAPI.API.csproj        # Archivo de proyecto
│
├── PastisserieAPI.Core/                  # 🎯 Capa de Dominio
│   ├── Entities/                         # Entidades del modelo
│   │   ├── User.cs                      # Usuario
│   │   ├── Producto.cs                  # Producto
│   │   ├── Pedido.cs                    # Pedido
│   │   ├── CarritoCompra.cs             # Carrito
│   │   ├── Review.cs                    # Reseña
│   │   └── ... (20 entidades total)
│   ├── Enums/                            # Enumeraciones
│   │   ├── EstadoPedido.cs
│   │   ├── EstadoEnvio.cs
│   │   ├── TipoNotificacion.cs
│   │   └── TipoRol.cs
│   ├── Interfaces/                       # Contratos
│   │   ├── IUnitOfWork.cs               # Unit of Work
│   │   └── Repositories/                 # Interfaces de repositorios
│   │       ├── IRepository.cs           # Repositorio genérico
│   │       ├── IUserRepository.cs
│   │       ├── IProductoRepository.cs
│   │       └── ... (7 interfaces)
│   └── PastisserieAPI.Core.csproj
│
├── PastisserieAPI.Infrastructure/        # 🗄️ Capa de Infraestructura
│   ├── Data/                             # Contexto de base de datos
│   │   ├── ApplicationDbContext.cs      # DbContext principal
│   │   ├── ApplicationDbContextFactory.cs # Factory para migrations
│   │   └── Configurations/               # Configuraciones Fluent API
│   │       ├── UserConfiguration.cs
│   │       ├── ProductoConfiguration.cs
│   │       └── PedidoConfiguration.cs
│   ├── Migrations/                       # Migraciones de EF Core
│   │   ├── 20260201XXXXXX_InitialCreate.cs
│   │   └── ApplicationDbContextModelSnapshot.cs
│   ├── Repositorie/                      # Implementaciones
│   │   ├── Repository.cs                # Repositorio genérico
│   │   ├── UnitOfWork.cs                # Unit of Work
│   │   ├── UserRepository.cs
│   │   ├── ProductoRepository.cs
│   │   └── ... (7 repositorios)
│   └── PastisserieAPI.Infrastructure.csproj
│
└── PastisserieAPI.Services/              # 💼 Capa de Aplicación
    ├── DTOs/                             # Data Transfer Objects
    │   ├── Common/                       # DTOs compartidos
    │   │   ├── ApiResponse.cs           # Respuesta estándar
    │   │   └── PaginationDto.cs         # Paginación
    │   ├── Request/                      # DTOs de entrada
    │   │   ├── RegisterRequestDto.cs
    │   │   ├── LoginRequestDto.cs
    │   │   ├── CreateProductoRequestDto.cs
    │   │   ├── CreatePedidoRequestDto.cs
    │   │   └── ... (15+ DTOs)
    │   └── Response/                     # DTOs de salida
    │       ├── UserResponseDto.cs
    │       ├── LoginResponseDto.cs
    │       ├── ProductoResponseDto.cs
    │       └── ... (10+ DTOs)
    ├── Validators/                       # Validadores FluentValidation
    │   ├── RegisterRequestValidator.cs
    │   ├── LoginRequestValidator.cs
    │   ├── CreateProductoRequestValidator.cs
    │   └── ... (8+ validadores)
    ├── Mappings/                         # Perfiles de AutoMapper
    │   └── MappingProfile.cs            # Mapeos entre entidades y DTOs
    ├── Services/                         # Servicios de negocio
    │   ├── Interfaces/
    │   │   ├── IAuthService.cs
    │   │   ├── IProductoService.cs
    │   │   ├── IPedidoService.cs
    │   │   └── ICarritoService.cs
    │   ├── AuthService.cs               # Lógica de autenticación
    │   ├── ProductoService.cs           # Lógica de productos
    │   ├── PedidoService.cs             # Lógica de pedidos
    │   └── CarritoService.cs            # Lógica de carrito
    ├── Helpers/
    │   └── JwtHelper.cs                 # Generación de tokens JWT
    └── PastisserieAPI.Services.csproj
