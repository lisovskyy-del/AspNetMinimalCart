using AspNetMinimalCart.Models;

namespace AspNetMinimalCart.Services;

public class ProductCatalog
{
    private readonly List<Product> _products =
    [
        new(1, "Product 1", 10.99m),
        new(2, "Product 2", 19.99m),
        new(3, "Product 3", 5.49m),
        new(4, "Product 4", 29.99m),
        new(5, "Product 5", 9.99m)
    ];

    public IReadOnlyList<Product> GetAll() => _products;

    public Product? GetById(int id) => _products.FirstOrDefault(p => p.Id == id);
}