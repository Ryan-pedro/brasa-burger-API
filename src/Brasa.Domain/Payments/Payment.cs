namespace Brasa.Domain.Payments;

public sealed class Payment
{
    public const int ProviderMaxLength = 40;
    public const int ChargeIdMaxLength = 120;
    public const int PixCopyPasteMaxLength = 512;

    private Payment()
    {
    }

    public Payment(Guid orderId, PaymentMethod method, decimal amount, DateTimeOffset now)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("O pagamento precisa de um pedido.", nameof(orderId));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        Id = Guid.CreateVersion7();
        OrderId = orderId;
        Method = method;
        Amount = amount;
        Status = PaymentStatus.Pending;
        CreatedAt = now;
        ExpiresAt = method == PaymentMethod.PixOnline ? now + PixPaymentPolicy.PaymentWindow : null;
    }

    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public PaymentMethod Method { get; private set; }

    public decimal Amount { get; private set; }

    public PaymentStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? PaidAt { get; private set; }

    public DateTimeOffset? ExpiresAt { get; private set; }

    public string? Provider { get; private set; }

    public string? ChargeId { get; private set; }

    public string? PixCopyPaste { get; private set; }

    public bool IsOnline => Method == PaymentMethod.PixOnline;

    public bool HasCharge => ChargeId is not null;

    public bool IsDueAt(DateTimeOffset now) => IsOnline && Status == PaymentStatus.Pending && now >= ExpiresAt;

    public void AttachCharge(string provider, string chargeId, string pixCopyPaste)
    {
        if (!IsOnline || Status != PaymentStatus.Pending || HasCharge)
        {
            throw new InvalidOperationException("A cobrança só é criada uma vez, para um Pix online pendente.");
        }

        Provider = Required(provider, ProviderMaxLength, nameof(provider));
        ChargeId = Required(chargeId, ChargeIdMaxLength, nameof(chargeId));
        PixCopyPaste = Required(pixCopyPaste, PixCopyPasteMaxLength, nameof(pixCopyPaste));
    }

    public void MarkPaid(DateTimeOffset now)
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException("O pagamento já foi registrado.");
        }

        Status = PaymentStatus.Paid;
        PaidAt = now;
    }

    public void Expire()
    {
        if (!IsOnline || Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException("Só um Pix online pendente pode expirar.");
        }

        Status = PaymentStatus.Expired;
    }

    public void MarkPaidLate(DateTimeOffset now)
    {
        if (Status != PaymentStatus.Expired)
        {
            throw new InvalidOperationException("Pagamento tardio só vale para um Pix online expirado.");
        }

        Status = PaymentStatus.PaidLate;
        PaidAt = now;
    }

    public void MarkAmountMismatch()
    {
        if (!IsOnline || Status is not (PaymentStatus.Pending or PaymentStatus.Expired))
        {
            throw new InvalidOperationException("Valor divergente só vale para um Pix online pendente ou expirado.");
        }

        Status = PaymentStatus.AmountMismatch;
    }

    private static string Required(string value, int maxLength, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        if (value.Length > maxLength)
        {
            throw new ArgumentException($"O campo pode ter no máximo {maxLength} caracteres.", parameterName);
        }

        return value;
    }
}
