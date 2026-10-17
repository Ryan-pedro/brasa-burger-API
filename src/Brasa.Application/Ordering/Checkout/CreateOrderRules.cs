using Brasa.Application.Shared;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Ordering;
using Brasa.Domain.Payments;
using Brasa.Domain.Shared;

namespace Brasa.Application.Ordering.Checkout;

internal static class CreateOrderRules
{
    public static ValidCheckoutDetails? Validate(CreateOrderRequest request, OrderType type, FieldErrors errors)
    {
        var customer = request.Customer ?? new CustomerRequest(null, null, null);

        Text(customer.Name, OrderCustomer.NameMaxLength, "customer.name", type != OrderType.DineIn, errors);
        Text(customer.Phone, OrderCustomer.PhoneMaxLength, "customer.phone", type == OrderType.Delivery, errors);
        Text(customer.Table, OrderCustomer.TableMaxLength, "customer.table", type == OrderType.DineIn, errors);
        Text(request.Notes, Order.NotesMaxLength, "notes", required: false, errors);

        var address = type == OrderType.Delivery ? ValidateAddress(request.Address, errors) : null;

        if (!EnumText.TryParse<PaymentMethod>(request.PaymentMethod, out var method))
        {
            errors.Add("paymentMethod", $"Escolha a forma de pagamento: {EnumText.Names<PaymentMethod>()}.");
        }

        if (request.ChangeForInCents is not null && method != PaymentMethod.Cash)
        {
            errors.Add("changeForInCents", "Troco só para pagamento em dinheiro.");
        }

        if (request.ExpectedTotalInCents is null or < 0)
        {
            errors.Add("expectedTotalInCents", "Informe o total que você viu na cotação.");
        }

        if (errors.Any)
        {
            return null;
        }

        return new ValidCheckoutDetails(
            new OrderCustomer(customer.Name, customer.Phone, type == OrderType.DineIn ? customer.Table : null),
            address,
            request.Notes,
            method,
            request.ChangeForInCents is { } change ? Money.FromCents(change) : null);
    }

    private static DeliveryAddress? ValidateAddress(AddressRequest? address, FieldErrors errors)
    {
        Text(address?.Street, DeliveryAddress.StreetMaxLength, "address.street", required: true, errors);
        Text(address?.Number, DeliveryAddress.NumberMaxLength, "address.number", required: true, errors);
        Text(address?.Complement, DeliveryAddress.ComplementMaxLength, "address.complement", required: false, errors);
        Text(address?.District, DeliveryAddress.DistrictMaxLength, "address.district", required: true, errors);

        return errors.Any ? null : new DeliveryAddress(address!.Street!, address.Number!, address.Complement, address.District!);
    }

    private static void Text(string? value, int maxLength, string field, bool required, FieldErrors errors)
    {
        var trimmed = FreeText.Normalize(value);
        if (trimmed is null)
        {
            if (required)
            {
                errors.Add(field, "Campo obrigatório.");
            }
        }
        else if (trimmed.Length > maxLength)
        {
            errors.Add(field, $"Use no máximo {maxLength} caracteres.");
        }
    }
}
