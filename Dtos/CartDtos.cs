namespace AspNetMinimalCart.Dtos;

public record CartItemResponse(
    int ProductId,
    string ProductName,
    decimal Price,
    int Quantity,
    decimal TotalPrice
);

public record CartResponse(
    List<CartItemResponse> Items,
    decimal GrandTotal
);