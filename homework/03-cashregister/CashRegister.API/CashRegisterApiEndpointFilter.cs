using CashRegister.Data;

namespace CashRegister.API;

public static class CashRegisterApiEndpointFilter
{
    public static async ValueTask<object?> CheckoutValidationAsync(EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var dbContext = context.GetArgument<ApplicationDataContext>(0);
        var receiptLines = context.GetArgument<List<CashRegisterEndpoints.ReceiptLineDto>>(1);
        
        if (receiptLines.Any(rl => rl.Quantity <= 0))
        {
            return Results.BadRequest("Quantity must be greater than zero for all receipt lines.");
        }
        
        if (receiptLines.Count == 0)
        {
            return Results.BadRequest("No receipt lines provided for checkout.");
        }

        if (receiptLines.Any(rl => !dbContext.Products.Any(p => p.Id == rl.ProductId)))
        {
            return Results.BadRequest("Invalid product ID provided for one or more receipt lines.");
        }
        
        return await next(context);
    }
}