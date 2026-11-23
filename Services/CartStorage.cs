using AspNetMinimalCart.Extensions;
using AspNetMinimalCart.Models;

namespace AspNetMinimalCart.Services;

public class CartStorage
{
    private const string CartSessionKey = "UserCart";

    public List<CartItem> GetCart(ISession session)
    {
        return session.GetFromJson<List<CartItem>>(CartSessionKey) ?? [];
    }

    public void AddItem(ISession session, int productId, int quantity = 1)
    {
        var cart = GetCart(session);
        var existingItem = cart.FirstOrDefault(item => item.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            cart.Add(new CartItem { ProductId = productId, Quantity = quantity });
        }

        session.SetAsJson(CartSessionKey, cart);
    }

    public void Clear(ISession session)
    {
        session.Remove(CartSessionKey);
    }
}