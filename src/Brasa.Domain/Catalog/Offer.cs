namespace Brasa.Domain.Catalog;

public sealed class Offer
{
    public const int LabelMaxLength = 60;

    private Offer()
    {
    }

    private Offer(string label, OfferKind kind, DateTimeOffset startsAt, DateTimeOffset? endsAt)
    {
        Id = Guid.CreateVersion7();
        Kind = kind;
        Relabel(label);
        Reschedule(startsAt, endsAt);
        IsActive = true;
    }

    public Guid Id { get; private set; }

    public string Label { get; private set; } = null!;

    public OfferKind Kind { get; private set; }

    public Guid? ProductId { get; private set; }

    public decimal? PromoPrice { get; private set; }

    public decimal? MinimumSubtotal { get; private set; }

    public decimal? DiscountAmount { get; private set; }

    public DateTimeOffset StartsAt { get; private set; }

    public DateTimeOffset? EndsAt { get; private set; }

    public bool IsActive { get; private set; }

    public static Offer ForProduct(string label, Guid productId, decimal promoPrice, DateTimeOffset startsAt, DateTimeOffset? endsAt)
    {
        var offer = new Offer(label, OfferKind.ProductPrice, startsAt, endsAt);
        offer.SetProductTerms(productId, promoPrice);
        return offer;
    }

    public static Offer ForOrderAmount(string label, decimal minimumSubtotal, decimal discountAmount, DateTimeOffset startsAt, DateTimeOffset? endsAt)
    {
        var offer = new Offer(label, OfferKind.OrderAmount, startsAt, endsAt);
        offer.SetOrderTerms(minimumSubtotal, discountAmount);
        return offer;
    }

    public void Relabel(string label)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        var trimmed = label.Trim();
        if (trimmed.Length > LabelMaxLength)
        {
            throw new ArgumentException($"O nome da oferta pode ter no máximo {LabelMaxLength} caracteres.", nameof(label));
        }

        Label = trimmed;
    }

    public void Reschedule(DateTimeOffset startsAt, DateTimeOffset? endsAt)
    {
        if (endsAt <= startsAt)
        {
            throw new ArgumentOutOfRangeException(nameof(endsAt), endsAt, "O fim da oferta precisa ser depois do início.");
        }

        StartsAt = startsAt;
        EndsAt = endsAt;
    }

    public void SetProductTerms(Guid productId, decimal promoPrice)
    {
        RequireKind(OfferKind.ProductPrice);

        if (productId == Guid.Empty)
        {
            throw new ArgumentException("A oferta precisa de um produto.", nameof(productId));
        }

        RequireAmount(promoPrice, nameof(promoPrice));

        ProductId = productId;
        PromoPrice = promoPrice;
    }

    public void SetOrderTerms(decimal minimumSubtotal, decimal discountAmount)
    {
        RequireKind(OfferKind.OrderAmount);
        RequireAmount(discountAmount, nameof(discountAmount));

        if (minimumSubtotal < discountAmount || minimumSubtotal > Product.MaxPrice || decimal.Round(minimumSubtotal, 2) != minimumSubtotal)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumSubtotal), minimumSubtotal, "O pedido mínimo precisa cobrir o desconto.");
        }

        MinimumSubtotal = minimumSubtotal;
        DiscountAmount = discountAmount;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public bool IsValidAt(DateTimeOffset moment) =>
        IsActive && StartsAt <= moment && (EndsAt is null || moment < EndsAt);

    private void RequireKind(OfferKind kind)
    {
        if (Kind != kind)
        {
            throw new InvalidOperationException("Os termos não correspondem ao tipo da oferta.");
        }
    }

    private static void RequireAmount(decimal amount, string parameterName)
    {
        if (amount <= 0 || amount > Product.MaxPrice || decimal.Round(amount, 2) != amount)
        {
            throw new ArgumentOutOfRangeException(parameterName, amount, $"O valor deve ser maior que zero, até {Product.MaxPrice}, com no máximo duas casas.");
        }
    }
}
