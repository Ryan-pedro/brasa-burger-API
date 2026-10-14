using Brasa.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Infrastructure.Persistence;

internal static class DemoOptions
{
    private static readonly Dictionary<string, string[]> Removable = new()
    {
        ["X-Bacon"] = ["Queijo prato", "Bacon", "Alface", "Tomate", "Cebola roxa", "Maionese da casa"],
        ["Clássico da Brasa"] = ["Cheddar", "Picles", "Cebola roxa", "Alface", "Molho da casa"],
        ["Duplo Smash"] = ["Cheddar duplo", "Cebola na chapa", "Molho especial"],
        ["Brasa BBQ"] = ["Provolone", "Onion rings", "Barbecue defumado"],
        ["Chicken Crispy"] = ["Coleslaw", "Alface", "Maionese de alho"],
        ["Veggie da Horta"] = ["Queijo coalho", "Tomate", "Rúcula"],
        ["Cheddar Melt"] = ["Cheddar cremoso", "Bacon"],
        ["Brasa Monstro"] = ["Queijos", "Bacon", "Alface"],
        ["Dog da Brasa"] = ["Mostarda", "Ketchup", "Batata palha"],
        ["Dog Cheddar Bacon"] = ["Cheddar cremoso", "Bacon"],
    };

    public static async Task AddToAsync(AppDbContext db, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var products = await db.Products.ToListAsync(cancellationToken);
        var categories = await db.Categories.AsNoTracking().ToDictionaryAsync(category => category.Slug, category => category.Id, cancellationToken);

        var burgerExtras = Group(db, "Adicionais", OptionKind.Addon, 0, 5, ("Bacon", 5m), ("Queijo", 3m), ("Ovo", 2.5m), ("Hambúrguer extra", 8m), ("Cebola caramelizada", 3m));
        var dogExtras = Group(db, "Adicionais", OptionKind.Addon, 0, 3, ("Bacon", 5m), ("Cheddar extra", 3m), ("Batata palha extra", 2m));
        var comboDrink = Group(db, "Bebida", OptionKind.Choice, 1, 1, ("Coca-Cola 350ml", 0m), ("Guaraná 350ml", 0m), ("Suco de laranja 500ml", 3.9m));

        foreach (var product in products)
        {
            var groups = new List<Guid>();

            if (product.CategoryId == categories.GetValueOrDefault("hamburgueres"))
            {
                groups.Add(burgerExtras.Id);
            }
            else if (product.CategoryId == categories.GetValueOrDefault("hot-dogs"))
            {
                groups.Add(dogExtras.Id);
            }
            else if (product.CategoryId == categories.GetValueOrDefault("combos"))
            {
                groups.Add(comboDrink.Id);
            }

            if (Removable.TryGetValue(product.Name, out var ingredients))
            {
                groups.Add(Group(db, "Retirar ingredientes", OptionKind.Removal, 0, ingredients.Length, [.. ingredients.Select(name => (name, 0m))]).Id);
            }

            product.SetOptionGroups(groups);
        }

        db.Offers.Add(Offer.ForOrderAmount("R$ 5 OFF no app", 30m, 5m, now, null));
    }

    private static OptionGroup Group(AppDbContext db, string name, OptionKind kind, int min, int max, params (string Name, decimal Price)[] options)
    {
        var group = new OptionGroup(name, kind, min, max);
        group.SetOptions(options.Select(option => new OptionDraft(null, option.Name, option.Price)).ToList());
        db.OptionGroups.Add(group);
        return group;
    }
}
