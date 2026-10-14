using Brasa.Application.Shared.Results;

namespace Brasa.Application.Ordering.Shared;

public static class OrderErrors
{
    public static readonly Error NotFound = Error.NotFound("order_not_found", "Pedido não encontrado.");

    public static readonly Error StoreClosed = Error.Conflict("store_closed", "A loja está fechada no momento.");

    public static readonly Error OrderTypeUnavailable = Error.Conflict("order_type_unavailable", "A loja não aceita esse tipo de pedido agora.");

    public static readonly Error InvalidTransition = Error.Conflict("invalid_transition", "O pedido não está em um estado que permita essa ação.");

    public static readonly Error IdempotencyKeyReused = Error.Rule("idempotency_key_reused", "Essa chave já foi usada em outro pedido.");

    public static Error BelowMinimum(long minimumInCents) =>
        Error.Rule("below_minimum_order", $"O pedido mínimo é de R$ {minimumInCents / 100},{minimumInCents % 100:00}.");
}
