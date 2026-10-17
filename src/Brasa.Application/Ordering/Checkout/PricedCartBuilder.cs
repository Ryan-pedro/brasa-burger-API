using Brasa.Application.Catalog.Shared;
using Brasa.Application.Ordering.Shared;
using Brasa.Application.Shared;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Application.StoreSettings.Shared;
using Brasa.Domain.Catalog;
using Brasa.Domain.Ordering;
using Brasa.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Ordering.Checkout;

public sealed class PricedCartBuilder(IAppDbContext db, TimeProvider clock)
{
    private const int MaxOptionsPerItem = 30;

    public async Task<Result<PricedCart>> BuildAsync(string? typeText, IReadOnlyList<CartItemRequest>? items, CancellationToken cancellationToken)
    {
        var store = await db.CurrentStoreAsync(cancellationToken);
        if (store is null)
        {
            return StoreErrors.NotConfigured;
        }

        var errors = new FieldErrors();
        if (!EnumText.TryParse<OrderType>(typeText, out var type))
        {
            errors.Add("type", $"Escolha o tipo de pedido: {EnumText.Names<OrderType>()}.");
        }

        ValidateShape(items, errors);
        if (errors.Any)
        {
            return errors.ToError();
        }

        if (!store.IsOpen)
        {
            return OrderErrors.StoreClosed;
        }

        if (!store.Accepts(type))
        {
            return OrderErrors.OrderTypeUnavailable;
        }

        var lines = await PriceLinesAsync(items!, cancellationToken);
        if (!lines.IsSuccess)
        {
            return lines.Error!;
        }

        var now = clock.GetUtcNow();
        var offers = await db.ValidOffersAsync(now, cancellationToken);
        var price = CartPricing.Calculate(lines.Value!, offers, store.DeliveryFeeFor(type), now);

        return new PricedCart(store, type, lines.Value!, price);
    }

    private static void ValidateShape(IReadOnlyList<CartItemRequest>? items, FieldErrors errors)
    {
        if (items is null || items.Count is 0 or > Order.MaxItems)
        {
            errors.Add("items", $"O pedido precisa de 1 a {Order.MaxItems} itens.");
            return;
        }

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var field = $"items[{index}]";

            if (item.ProductId is null || item.ProductId == Guid.Empty)
            {
                errors.Add($"{field}.productId", "Informe o produto.");
            }

            if (item.Quantity is null or < 1 or > CartLine.MaxQuantity)
            {
                errors.Add($"{field}.quantity", $"A quantidade vai de 1 a {CartLine.MaxQuantity}.");
            }

            if (item.OptionIds is { } optionIds && (optionIds.Count > MaxOptionsPerItem || optionIds.Distinct().Count() != optionIds.Count))
            {
                errors.Add($"{field}.optionIds", "Opções repetidas ou em excesso.");
            }

            if (FreeText.Normalize(item.Notes)?.Length > CartLine.NotesMaxLength)
            {
                errors.Add($"{field}.notes", $"Use no máximo {CartLine.NotesMaxLength} caracteres.");
            }
        }
    }

    private async Task<Result<IReadOnlyList<CartLine>>> PriceLinesAsync(IReadOnlyList<CartItemRequest> items, CancellationToken cancellationToken)
    {
        var visibleCategoryIds = (await db.VisibleCategoriesAsync(cancellationToken)).Select(category => category.Id).ToHashSet();
        var productIds = items.Select(item => item.ProductId!.Value).Distinct().ToList();

        var products = await db.Products
            .AsNoTracking()
            .Where(product => productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, cancellationToken);

        var groups = await db.OptionGroupsForAsync(products.Values, cancellationToken);

        var invalid = new FieldErrors();
        var unavailable = new FieldErrors();
        var lines = new List<CartLine>(items.Count);

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var field = $"items[{index}]";

            if (!products.TryGetValue(item.ProductId!.Value, out var product) || !visibleCategoryIds.Contains(product.CategoryId))
            {
                unavailable.Add(field, "Produto não está mais no cardápio.");
                continue;
            }

            var productGroups = groups.GroupsOf(product);
            if (!product.IsOrderable(productGroups))
            {
                unavailable.Add(field, $"{product.Name} está indisponível.");
                continue;
            }

            var options = SelectOptions(product, productGroups, item.OptionIds ?? [], field, invalid, unavailable);
            lines.Add(new CartLine(
                product.Id,
                product.Name,
                product.Price,
                item.Quantity!.Value,
                options,
                FreeText.Clean(item.Notes, CartLine.NotesMaxLength, nameof(item.Notes))));
        }

        if (invalid.Any)
        {
            return invalid.ToError();
        }

        if (unavailable.Any)
        {
            return unavailable.ToConflict("item_unavailable", "Alguns itens do carrinho estão indisponíveis.");
        }

        return lines;
    }

    private static List<PricedOption> SelectOptions(
        Product product,
        IReadOnlyList<OptionGroup> groups,
        IReadOnlyList<Guid> optionIds,
        string field,
        FieldErrors invalid,
        FieldErrors unavailable)
    {
        var selected = new List<PricedOption>();
        var remaining = optionIds.ToHashSet();

        foreach (var group in groups)
        {
            var chosen = group.Options.Where(option => remaining.Remove(option.Id)).ToList();

            if (chosen.Count < group.MinSelections || chosen.Count > group.MaxSelections)
            {
                invalid.Add($"{field}.optionIds", Limits(group));
            }

            foreach (var option in chosen.Where(option => !option.IsAvailable))
            {
                unavailable.Add(field, $"{option.Name} está indisponível.");
            }

            selected.AddRange(chosen.Select(option => new PricedOption(option.Id, group.Name, option.Name, group.Kind, option.PriceDelta)));
        }

        if (remaining.Count > 0)
        {
            invalid.Add($"{field}.optionIds", $"Há opções que não pertencem a {product.Name}.");
        }

        return selected;
    }

    private static string Limits(OptionGroup group) =>
        group.MinSelections == group.MaxSelections
            ? $"Escolha {group.MinSelections} em {group.Name}."
            : $"Escolha de {group.MinSelections} a {group.MaxSelections} em {group.Name}.";
}
