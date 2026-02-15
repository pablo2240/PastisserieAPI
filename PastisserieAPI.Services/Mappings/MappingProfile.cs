using AutoMapper;
using PastisserieAPI.Core.Entities;
using PastisserieAPI.Services.DTOs.Request;
using PastisserieAPI.Services.DTOs.Response;

namespace PastisserieAPI.Services.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ============ USER MAPPINGS ============
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                    src.UserRoles.Select(ur => ur.Rol.Nombre).ToList()));

            CreateMap<RegisterRequestDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FechaRegistro, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.EmailVerificado, opt => opt.MapFrom(src => false));

            CreateMap<UpdateUserRequestDto, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ============ PRODUCTO MAPPINGS ============
            CreateMap<Producto, ProductoResponseDto>()
                .ForMember(dest => dest.CategoriaNombre,
                           opt => opt.MapFrom(src => src.CategoriaProducto.Nombre));

            CreateMap<CreateProductoRequestDto, Producto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.FechaActualizacion, opt => opt.Ignore())
                .ForMember(dest => dest.CategoriaProducto, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.PedidoItems, opt => opt.Ignore())
                .ForMember(dest => dest.CarritoItems, opt => opt.Ignore());

            // ============ PEDIDO MAPPINGS ============
            CreateMap<Pedido, PedidoResponseDto>()
                .ForMember(dest => dest.NombreUsuario, opt => opt.MapFrom(src => src.Usuario.Nombre))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.DireccionEnvio, opt => opt.MapFrom(src => src.DireccionEnvio));

            CreateMap<PedidoItem, PedidoItemResponseDto>()
                .ForMember(dest => dest.NombreProducto, opt => opt.MapFrom(src => src.Producto.Nombre));

            CreateMap<CreatePedidoRequestDto, Pedido>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FechaPedido, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => "Pendiente"))
                .ForMember(dest => dest.Aprobado, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.Items, opt => opt.Ignore());

            CreateMap<PedidoItemRequestDto, PedidoItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PedidoId, opt => opt.Ignore())
                .ForMember(dest => dest.PrecioUnitario, opt => opt.Ignore())
                .ForMember(dest => dest.Subtotal, opt => opt.Ignore());

            // ============ CARRITO MAPPINGS ============
            CreateMap<CarritoCompra, CarritoResponseDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src =>
                    src.Items.Sum(i => i.Cantidad * i.Producto.Precio)))
                .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src =>
                    src.Items.Sum(i => i.Cantidad)));

            CreateMap<CarritoItem, CarritoItemResponseDto>()
                .ForMember(dest => dest.NombreProducto, opt => opt.MapFrom(src => src.Producto.Nombre))
                .ForMember(dest => dest.PrecioUnitario, opt => opt.MapFrom(src => src.Producto.Precio))
                .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Cantidad * src.Producto.Precio))
                .ForMember(dest => dest.ImagenUrl, opt => opt.MapFrom(src => src.Producto.ImagenUrl));

            CreateMap<AddToCarritoRequestDto, CarritoItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CarritoId, opt => opt.Ignore())
                .ForMember(dest => dest.FechaAgregado, opt => opt.MapFrom(src => DateTime.UtcNow));

            // ============ REVIEW MAPPINGS ============
            CreateMap<Review, ReviewResponseDto>()
                .ForMember(dest => dest.NombreUsuario, opt => opt.MapFrom(src => src.Usuario.Nombre))
                .ForMember(dest => dest.NombreProducto, opt => opt.MapFrom(src => src.Producto.Nombre));

            CreateMap<CreateReviewRequestDto, Review>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioId, opt => opt.Ignore())
                .ForMember(dest => dest.Fecha, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Aprobada, opt => opt.MapFrom(src => false));

            // ============ DIRECCION MAPPINGS ============
            CreateMap<DireccionEnvio, DireccionEnvioResponseDto>();

            CreateMap<CreateDireccionRequestDto, DireccionEnvio>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioId, opt => opt.Ignore())
                .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => DateTime.UtcNow));

            // ============ CATEGORIA MAPPINGS ============
            CreateMap<CategoriaProducto, CategoriaProducto>();
        }
    }
}