using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CashRegister.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace CashRegister.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IDbContextFactory<ApplicationDataContext> contextFactory;

    public ObservableCollection<Product> Products { get; } = [];
    public ObservableCollection<ReceiptLineViewModel> ReceiptLines { get; } = [];
    
    [ObservableProperty]
    private decimal totalPrice;

    public MainWindowViewModel(IDbContextFactory<ApplicationDataContext> contextFactory)
    {
        this.contextFactory = contextFactory;
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        await SeedDatabaseAsync();
        await LoadProductsAsync();
    }
    
    private async Task SeedDatabaseAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        if (!await context.Products.AnyAsync())
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
            
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }

    private async Task LoadProductsAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        
        var products = await context.Products.ToListAsync();
        
        Products.Clear();
        foreach (var product in products)
        {
            Products.Add(product);
        }
    }

    [RelayCommand]
    private void AddProduct(Product product)
    {
        var existingLine = ReceiptLines.FirstOrDefault(rl => rl.ProductId == product.Id);
        if (existingLine is not null)
        {
            existingLine.Quantity++;
            existingLine.TotalPrice = existingLine.Quantity * existingLine.UnitPrice;
        }
        else
        {
            var newLine = new ReceiptLineViewModel
            {
                ProductId = product.Id,
                ProductName = product.ProductName,
                UnitPrice = product.UnitPrice,
                Quantity = 1,
                TotalPrice = product.UnitPrice
            };
            ReceiptLines.Add(newLine);
        }
        
        CalculateTotal();
    }

    private void CalculateTotal()
    {
        TotalPrice = ReceiptLines.Sum(rl => rl.TotalPrice);
    }

    [RelayCommand]
    private async Task Checkout()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        if (!ReceiptLines.Any())
        {
            var emptyBox = MessageBoxManager.GetMessageBoxStandard("Information",
                "No items in receipt to checkout.", ButtonEnum.Ok);
            await emptyBox.ShowAsync();
        }
        else
        {
            var receiptLines = ReceiptLines.Select(rl => new ReceiptLine
            {
                ProductId = rl.ProductId,
                Quantity = rl.Quantity,
                TotalPrice = rl.TotalPrice
            }).ToList();
            var receipt = new Receipt
            {
                ReceiptLines = receiptLines,
                TotalPrice = TotalPrice
            };
            
            await context.Receipts.AddAsync(receipt);
            await context.SaveChangesAsync();
            
            ReceiptLines.Clear();
            CalculateTotal();
        }
    }
    
}

public partial class ReceiptLineViewModel : ObservableObject
{
    [ObservableProperty]
    private int productId;

    [ObservableProperty]
    private string productName = string.Empty;

    [ObservableProperty]
    private int quantity;

    [ObservableProperty]
    private decimal unitPrice;

    [ObservableProperty]
    private decimal totalPrice;
}