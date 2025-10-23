
using CashRegister.Data;
using Microsoft.EntityFrameworkCore;

namespace CashRegister.API;

public static class CashRegisterEndpoints
{
    private static async Task AddInitialData(ApplicationDataContext context)
    {
        if (!context.Products.Any())
        {
            var products = new List<Product>
            {
                new() { ProductName = "Bananen 1kg", UnitPrice = 1.99m },
                new() { ProductName = "Äpfel 1kg", UnitPrice = 2.99m },
                new() { ProductName = "Trauben Weiß 500g", UnitPrice = 3.49m },
                new() { ProductName = "Himbeeren 125g", UnitPrice = 2.99m },
                new() { ProductName = "Karotten 500g", UnitPrice = 1.29m },
                new() { ProductName = "Eissalat 1 Stück", UnitPrice = 1.98m },
                new() { ProductName = "Zucchini 1 Stück", UnitPrice = 0.99m },
                new() { ProductName = "Knoblauch 150g", UnitPrice = 1.49m },
                new() { ProductName = "Joghurt 200g", UnitPrice = 0.89m },
                new() { ProductName = "Butter", UnitPrice = 2.49m }
            };
            
            context.Products.AddRange(products);
            await context.SaveChangesAsync();    
        }
    }

    public static async Task<IResult> Checkout(ApplicationDataContext context, List<ReceiptLineDto> receiptLines)
    {
        var receipt = new Receipt
        {
            ReceiptLines = receiptLines.Select(rl => new ReceiptLine
            {
                ProductId = rl.ProductId,
                Quantity = rl.Quantity,
                TotalPrice = rl.TotalPrice
            }).ToList(),
            TotalPrice = receiptLines.Sum(rl => rl.TotalPrice)
        };

        await context.Receipts.AddAsync(receipt);
        await context.SaveChangesAsync();

        return Results.Ok(receipt.Id);
    }

    public record ReceiptLineDto(int ProductId, int Quantity, decimal TotalPrice);
    
    public static async Task<IResult> GetProducts(ApplicationDataContext context)
    {
        await AddInitialData(context);
        var products = await context.Products.ToListAsync();
        return Results.Ok(products);
    }
}
