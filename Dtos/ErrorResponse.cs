namespace AspNetMinimalCart.Dtos;

public record ErrorResponse(
    int StatusCode,
    string Message,
    string? Details = null
);