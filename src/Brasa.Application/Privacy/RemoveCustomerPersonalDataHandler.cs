using System.Text.RegularExpressions;
using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Privacy;

public sealed class RemoveCustomerPersonalDataHandler(IAppDbContext db, PersonalDataRemoval removal, AuditTrail audit)
{
    public const int MinimumPhoneDigits = 8;
    public const int MaxOrderNumbers = 100;

    public async Task<Result<PersonalDataRemovalResponse>> HandleAsync(PersonalDataRemovalRequest request, CancellationToken cancellationToken)
    {
        var phoneDigits = request.Phone is null ? null : new string(request.Phone.Where(char.IsAsciiDigit).ToArray());
        var numbers = request.OrderNumbers ?? [];

        var errors = new FieldErrors();
        if (phoneDigits is not null && phoneDigits.Length < MinimumPhoneDigits)
        {
            errors.Add("phone", $"Informe o telefone com pelo menos {MinimumPhoneDigits} dígitos.");
        }

        if (numbers.Count > MaxOrderNumbers)
        {
            errors.Add("orderNumbers", $"Informe no máximo {MaxOrderNumbers} pedidos por vez.");
        }

        if (phoneDigits is null && numbers.Count == 0)
        {
            errors.Add("phone", "Informe o telefone do titular ou os números dos pedidos.");
        }

        if (errors.Any)
        {
            return errors.ToError();
        }

        var phonePattern = phoneDigits is null ? null : $@"^\D*{string.Join(@"\D*", phoneDigits.ToCharArray())}\D*$";
        var matches = await db.Orders
            .Where(order => order.PersonalDataRemovedAt == null
                && (numbers.Contains(order.Number)
                    || (phonePattern != null && order.Customer.Phone != null && Regex.IsMatch(order.Customer.Phone, phonePattern))))
            .ToListAsync(cancellationToken);

        var finished = matches.Where(order => order.IsFinished).ToList();
        var unfinished = matches.Where(order => !order.IsFinished).Select(order => order.Number).Order().ToList();

        audit.Record("privacy.personal_data_removed", null, new { removedOrders = finished.Count, unfinishedOrders = unfinished.Count });
        await removal.RemoveAsync(finished, cancellationToken);

        return new PersonalDataRemovalResponse(finished.Count, unfinished);
    }
}
