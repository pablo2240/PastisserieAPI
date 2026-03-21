using PastisserieAPI.Core.Entities;
using PastisserieAPI.Services.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PastisserieAPI.Services.Services
{
    public class InvoiceService : IInvoiceService
    {
        public InvoiceService()
        {
            // QuestPDF License config (Community)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateInvoicePdf(Pedido pedido, User usuario)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(x => ComposeContent(x, pedido, usuario));
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        void ComposeHeader(IContainer container)
        {
            var titleStyle = TextStyle.Default.FontSize(28).SemiBold().FontColor("#7D2121");

            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Pâtisserie Deluxe").Style(titleStyle);
                    column.Item().Text("Avenida Principal 123, Medellín, Colombia").FontSize(10).FontColor(Colors.Grey.Medium);
                    column.Item().Text("Tel: +57 300 123 4567").FontSize(10).FontColor(Colors.Grey.Medium);
                    column.Item().Text("Email: contacto@patisseriedeluxe.com").FontSize(10).FontColor(Colors.Grey.Medium);
                });

                // Intentar cargar el logo si existe en la ruta relativa esperada
                try 
                {
                    var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "pastisserie-front", "public", "logo.png");
                    if (File.Exists(logoPath))
                    {
                        row.ConstantItem(80).Image(logoPath);
                    }
                    else 
                    {
                        row.ConstantItem(80).Height(60).Placeholder(); 
                    }
                }
                catch { row.ConstantItem(80).Height(60).Placeholder(); }
            });
        }

        void ComposeContent(IContainer container, Pedido pedido, User usuario)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                column.Spacing(20);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("FACTURADO A:").SemiBold().FontColor("#7D2121");
                        col.Item().Text(usuario.Nombre);
                        col.Item().Text(usuario.Email);
                        if (!string.IsNullOrEmpty(usuario.Telefono)) col.Item().Text($"Tel: {usuario.Telefono}");
                    });

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text($"FACTURA N°: #{pedido.Id}").SemiBold().FontColor("#7D2121");
                        col.Item().Text($"Fecha Compra: {pedido.FechaPedido.AddHours(-5):dd/MM/yyyy HH:mm}");
                        col.Item().Text("Sincronizado con Medellín (UTC-5)");
                    });
                });

                column.Item().Element(col => ComposeTable(col, pedido));

                column.Item().AlignRight().PaddingRight(5).Column(col =>
                {
                    col.Spacing(5);
                    col.Item().Text($"Subtotal: ${pedido.Subtotal:N0}").FontSize(12);
                    col.Item().Text($"IVA (0%): $0").FontSize(11).FontColor(Colors.Grey.Medium);
                    
                    if (pedido.CostoEnvio > 0)
                        col.Item().Text($"Domicilio: ${pedido.CostoEnvio:N0}").FontSize(12);
                    
                    col.Item().PaddingTop(5).Text($"Total: ${pedido.Total:N0}").FontSize(18).SemiBold().FontColor("#7D2121");
                });
            });
        }

        void ComposeTable(IContainer container, Pedido pedido)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(50); // Cantidad
                    columns.RelativeColumn(3);  // Producto
                    columns.RelativeColumn();   // Precio Unit
                    columns.RelativeColumn();   // Subtotal
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Cant.").SemiBold();
                    header.Cell().Element(CellStyle).Text("Producto").SemiBold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Unitario").SemiBold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Subtotal").SemiBold();

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                    }
                });

                foreach (var item in pedido.Items)
                {
                    table.Cell().Element(CellStyle).Text(item.Cantidad.ToString());
                    
                    // Asumiendo que el item tiene NombreProducto cargado. Si no, QuestPDF fallará.
                    // Para mayor seguridad en la generación, en el PedidoService asegurar Include(Producto)
                    var nombre = item.Producto?.Nombre ?? "Producto Patisserie";
                    
                    table.Cell().Element(CellStyle).Text(nombre);
                    table.Cell().Element(CellStyle).AlignRight().Text($"${item.PrecioUnitario:N2}");
                    table.Cell().Element(CellStyle).AlignRight().Text($"${item.Subtotal:N2}");

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                    }
                }
            });
        }

        void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Gracias por preferir Pâtisserie Deluxe. ");
                x.Span("Página ").FontSize(10);
                x.CurrentPageNumber().FontSize(10);
                x.Span(" de ").FontSize(10);
                x.TotalPages().FontSize(10);
            });
        }
    }
}
