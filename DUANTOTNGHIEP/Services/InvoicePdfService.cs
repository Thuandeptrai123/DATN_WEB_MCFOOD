using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using DUANTOTNGHIEP.Models;
using Microsoft.AspNetCore.Hosting;

namespace DUANTOTNGHIEP.Services
{
    public class InvoicePdfService
    {
        private readonly IWebHostEnvironment _env;

        public InvoicePdfService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string GeneratePdf(Invoice invoice, ApplicationUser customer, List<(string name, int qty, decimal unitPrice)> items)
        {
            var outputDir = Path.Combine(_env.WebRootPath, "invoices");
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var fileName = $"invoice_{invoice.Id}.pdf";
            var filePath = Path.Combine(outputDir, fileName);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);

                    // Header Section
                    page.Header().Column(header =>
                    {
                        header.Item().Background(Colors.Blue.Medium).Padding(20).Column(col =>
                        {
                            col.Item().Text("HÓA ĐƠN BÁN HÀNG")
                                .Bold()
                                .FontSize(24)
                                .FontColor(Colors.White)
                                .AlignCenter();

                            col.Item().PaddingTop(5).Text($"Số hóa đơn: #{invoice.Id}")
                                .FontSize(12)
                                .FontColor(Colors.White)
                                .AlignCenter();
                        });
                    });

                    // Content Section
                    page.Content().PaddingTop(20).Column(col =>
                    {
                        // Customer Information Section
                        col.Item().Background(Colors.Grey.Lighten4).Padding(15).Column(customerInfo =>
                        {
                            customerInfo.Item().PaddingBottom(10).Text("THÔNG TIN KHÁCH HÀNG")
                                .Bold()
                                .FontSize(14)
                                .FontColor(Colors.Blue.Medium);

                            customerInfo.Item().Row(row =>
                            {
                                row.RelativeItem().Column(leftCol =>
                                {
                                    leftCol.Item().PaddingBottom(3).Text($"Khách hàng: {customer.FirstName} {customer.LastName}")
                                        .FontSize(11);
                                    leftCol.Item().PaddingBottom(3).Text($"Email: {customer.Email}")
                                        .FontSize(11);
                                });

                                row.RelativeItem().Column(rightCol =>
                                {
                                    rightCol.Item().PaddingBottom(3).Text($"SĐT: {customer.PhoneNumbers}")
                                        .FontSize(11);
                                    rightCol.Item().PaddingBottom(3).Text($"Địa chỉ: {customer.Address}")
                                        .FontSize(11);
                                });
                            });
                        });

                        // Invoice Information Section
                        col.Item().PaddingTop(15).Background(Colors.Grey.Lighten5).Padding(15).Column(invoiceInfo =>
                        {
                            invoiceInfo.Item().PaddingBottom(10).Text("THÔNG TIN ĐƠN HÀNG")
                                .Bold()
                                .FontSize(14)
                                .FontColor(Colors.Blue.Medium);

                            invoiceInfo.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"Ngày tạo đơn: {invoice.CreatedDate:dd/MM/yyyy HH:mm}")
                                    .FontSize(11);
                                row.RelativeItem().Text($"Ngày xuất hóa đơn: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                    .FontSize(11);
                                row.RelativeItem().Text($"Trạng thái: {invoice.Status}")
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor(Colors.Green.Medium);
                            });
                        });

                        // Items Table Section
                        col.Item().PaddingTop(25).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4); // Tên sản phẩm
                                columns.RelativeColumn(2); // Số lượng
                                columns.RelativeColumn(3); // Đơn giá
                                columns.RelativeColumn(3); // Thành tiền
                            });

                            // Table Header
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Medium).Padding(10)
                                    .Text("Tên sản phẩm").Bold().FontColor(Colors.White).FontSize(12);
                                header.Cell().Background(Colors.Blue.Medium).Padding(10)
                                    .Text("Số lượng").Bold().FontColor(Colors.White).FontSize(12).AlignCenter();
                                header.Cell().Background(Colors.Blue.Medium).Padding(10)
                                    .Text("Đơn giá").Bold().FontColor(Colors.White).FontSize(12).AlignCenter();
                                header.Cell().Background(Colors.Blue.Medium).Padding(10)
                                    .Text("Thành tiền").Bold().FontColor(Colors.White).FontSize(12).AlignCenter();
                            });

                            // Table Rows
                            bool isEvenRow = false;
                            foreach (var item in items)
                            {
                                var bgColor = isEvenRow ? Colors.Grey.Lighten5 : Colors.White;

                                table.Cell().Background(bgColor).Padding(10).BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
                                    .Text(item.name).FontSize(11);
                                table.Cell().Background(bgColor).Padding(10).BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
                                    .Text(item.qty.ToString()).FontSize(11).AlignCenter();
                                table.Cell().Background(bgColor).Padding(10).BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
                                    .Text($"{item.unitPrice:N0}₫").FontSize(11).AlignCenter();
                                table.Cell().Background(bgColor).Padding(10).BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
                                    .Text($"{(item.qty * item.unitPrice):N0}₫").FontSize(11).AlignCenter().Bold();

                                isEvenRow = !isEvenRow;
                            }
                        });

                        // Total Section
                        col.Item().PaddingTop(20).AlignRight().Column(totalCol =>
                        {
                            totalCol.Item().Background(Colors.Blue.Medium).Padding(15).Width(200)
                                .Text($"TỔNG TIỀN: {invoice.TotalAmount:N0}₫")
                                .Bold()
                                .FontSize(16)
                                .FontColor(Colors.White)
                                .AlignCenter();
                        });

                        // Thank you message
                        col.Item().PaddingTop(30).AlignCenter().Column(thankYou =>
                        {
                            thankYou.Item().Text("Cảm ơn quý khách đã mua hàng!")
                                .FontSize(14)
                                .Bold()
                                .FontColor(Colors.Blue.Medium);
                            thankYou.Item().PaddingTop(5).Text("Chúc quý khách một ngày tốt lành!")
                                .FontSize(12)
                                .FontColor(Colors.Grey.Medium);
                        });
                    });

                    // Footer
                    page.Footer().AlignCenter().Column(footer =>
                    {
                        footer.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        footer.Item().PaddingTop(10).Text($"Xuất lúc: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Medium);
                    });
                });
            }).GeneratePdf(filePath);

            return $"/invoices/{fileName}";
        }
    }
}