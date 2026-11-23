using AspNetMinimalCart.Dtos;

namespace AspNetMinimalCart.Endpoints;

public static class IdentifyEndpoints
{
    private const string UserIdCookieName = "UserId";

    public static RouteGroupBuilder MapIdentifyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/identify")
            .WithTags("Identification");
        
        group.MapGet("/", IdentifyUser);

        return group;
    }

    private static IResult IdentifyUser(HttpContext context, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("IdentifyEndpoints");

        if (context.Request.Cookies.TryGetValue(UserIdCookieName, out var existingCookieValue))
        {
            if (!Guid.TryParse(existingCookieValue, out var existingUserId))
            {
                logger.LogWarning("Invalid UserId cookie format received.");
                return Results.BadRequest(new ErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "Invalid UserId cookie format. Expected a valid GUID."));
            }

            var maskedExistingId = MaskGuid(existingUserId.ToString());
            logger.LogInformation("Existing user identified: {UserId}", maskedExistingId);

            return Results.Ok(new
            {
                userId = existingUserId,
                isNew = false,
                message = "Existing user identified."
            });
        }

        var newUserId = Guid.NewGuid();
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        };

        context.Response.Cookies.Append(UserIdCookieName, newUserId.ToString(), cookieOptions);

        var maskedNewId = MaskGuid(newUserId.ToString());
        logger.LogInformation("New user identified and cookie set: {UserId}", maskedNewId);

        return Results.Ok(new
        {
            userId = newUserId,
            isNew = true,
            message = "New user identified."
        });
    }

    private static string MaskGuid(string guid) => $"{guid[..8]}***";
}