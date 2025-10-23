using CashRegister.Data;

namespace CashRegister.API;

public static class CashRegisterApi
{
    public static void MapCashRegisterApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/cashregister");
        group.MapGet("/products", CashRegisterEndpoints.GetProducts)
            .WithName(nameof(CashRegisterEndpoints.GetProducts))
            .WithDescription("Get all products")
            .Produces<List<Product>>();

        group.MapPost("/checkout", CashRegisterEndpoints.Checkout)
            .AddEndpointFilter(CashRegisterApiEndpointFilter.CheckoutValidationAsync)
            .WithName(nameof(CashRegisterEndpoints.Checkout))
            .WithDescription("Checkout products")
            .Accepts<CashRegisterEndpoints.ReceiptLineDto[]>("application/json")
            .Produces<Receipt>(201);
    }
}