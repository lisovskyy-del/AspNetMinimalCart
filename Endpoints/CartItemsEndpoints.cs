using AspNetMinimalCart.Dtos;
using AspNetMinimalCart.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMinimalCart.Endpoints;

public static class CartItemsEndpoints
{

    public static RouteGroupBuilder MapCartItemsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/cart")
                        .WithTags("Cart");

        group.MapGet("/", GetCartItems);
        group.MapPost("/add/{productId:int}", AddCartItem);
        group.MapDelete("/clear", ClearCartItems);

        return group;
    }

    private static IResult GetCartItems(
        HttpContext context,
        CartStorage cartStorage,
        ProductCatalog productCatalog,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("CartEndpoints");
        var userId = GetMaskedUserId(context);
        
        var cartItems = cartStorage.GetCart(context.Session);

        if (cartItems.Count == 0)
        {
            logger.LogWarning("User {UserId} requested cart, but session is empty or expired.", userId);
            return Results.BadRequest(new ErrorResponse(
                StatusCodes.Status400BadRequest,
                "Cart is empty or session has expired."));
        }

        var response = new List<CartItemResponse>();
        foreach (var item in cartItems)
        {
            var product = productCatalog.GetById(item.ProductId);
            if (product is not null)
            {
                response.Add(new CartItemResponse(
                    ProductId: product.Id,
                    ProductName: product.Name,
                    Price: product.Price,
                    Quantity: item.Quantity,
                    TotalPrice: product.Price * item.Quantity
                ));
            }
        }

        logger.LogInformation("User {UserId} retrieved cart items: {@CartItems}", userId, response);

        return Results.Ok(response);
    }

    private static IResult AddCartItem(
        int productId,
        [FromQuery] int? quantity,
        HttpContext context,
        CartStorage cartStorage,
        ProductCatalog productCatalog,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("CartEndpoints");
        var userId = GetMaskedUserId(context);

        int actualQuantity = quantity ?? 1;

        if (actualQuantity <= 0)
        {
            logger.LogWarning("User {UserId} attempted to add product {ProductId} with invalid quantity: {Quantity}",
                userId, productId, actualQuantity);

            return Results.BadRequest(new ErrorResponse(
                StatusCodes.Status400BadRequest,
                "Quantity must be greater than zero."));
        }

        var product = productCatalog.GetById(productId);
        if (product is null)
        {
            logger.LogWarning("User {UserId} attempted to add non-existent product with ID {ProductId}.",
                userId, productId);

            return Results.NotFound(new ErrorResponse(
                StatusCodes.Status404NotFound,
                $"Product with ID {productId} not found."));
        }

        cartStorage.AddItem(context.Session, productId, actualQuantity);
        logger.LogInformation("User {UserId} added product {ProductId} {ProductName} with quantity {Quantity} to  the cart.", userId, productId, product.Name, actualQuantity);
        
        return Results.Ok(new
        {
            message = $"Added {actualQuantity} of {product.Name} to the cart.",
            productId = product.Id,
            quantity = actualQuantity
        });   
    }

    private static IResult ClearCartItems(
        HttpContext context,
        CartStorage cartStorage,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("CartEndpoints");
        var userId = GetMaskedUserId(context);

        cartStorage.Clear(context.Session);
        logger.LogInformation("User {UserId} cleared the cart.", userId);

        return Results.Ok(new { message = "Cart cleared successfully." });
    }

    private static string GetMaskedUserId(HttpContext context)
    {
        var rawCookie = context.Request.Cookies["UserId"];

        if (string.IsNullOrEmpty(rawCookie))
            return "Anonymous";
        
        return rawCookie.Length > 8 ? $"{rawCookie[..8]}***" : "InvalidCookie";
    }
}