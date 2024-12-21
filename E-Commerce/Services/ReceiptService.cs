using E_Commerce.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;

namespace E_Commerce.Services
{
    public class ReceiptService
    {
        public byte[] GenerateReceipt(User user, List<BasketItem> basketItems, decimal totalAmount, DateTime transactionDate)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);

                    page.Header().Text("Receipt for Transaction").FontSize(20).Bold().AlignCenter();

                    page.Content().Stack(stack =>
                    {
                        stack.Item().Text($"Date: {transactionDate:yyyy-MM-dd HH:mm}");
                        stack.Item().Text($"Customer: {user.UserName}");

                        stack.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Product").Bold();
                                header.Cell().Text("Price").Bold();
                                header.Cell().Text("Quantity").Bold();
                                header.Cell().Text("Total").Bold();
                            });

                            foreach (var item in basketItems)
                            {
                                table.Cell().Text(item.Product.ProductName);
                                table.Cell().Text(item.Product.ProductPrice.ToString("C"));
                                table.Cell().Text(item.Quantity.ToString());
                                table.Cell().Text((item.Product.ProductPrice * item.Quantity).ToString("C"));
                            }
                        });

                        stack.Item().Text($"Total Amount: {totalAmount:C}").FontSize(16).Bold();
                    });
                });
            });

            using var memoryStream = new MemoryStream();
            document.GeneratePdf(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
