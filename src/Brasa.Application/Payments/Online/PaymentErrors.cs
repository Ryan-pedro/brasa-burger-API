using Brasa.Application.Shared.Results;

namespace Brasa.Application.Payments.Online;

public static class PaymentErrors
{
    public static readonly Error PixPaymentNotFound = Error.NotFound("pix_payment_not_found", "Este pedido não tem pagamento por Pix online.");

    public static readonly Error PixUnavailable = Error.Unavailable("pix_unavailable", "Não foi possível gerar o Pix agora. Tente de novo em instantes.");
}
