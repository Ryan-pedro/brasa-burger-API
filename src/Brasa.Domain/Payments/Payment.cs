namespace Brasa.Domain.Payments;

public sealed class Payment
{
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
    }

    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public PaymentMethod Method { get; private set; }

    public decimal Amount { get; private set; }

    public PaymentStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? PaidAt { get; private set; }

    public void MarkPaid(DateTimeOffset now)
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException("O pagamento já foi registrado.");
        }

        Status = PaymentStatus.Paid;
        PaidAt = now;
    }
}
