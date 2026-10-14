using Brasa.Application.Shared;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Catalog;

namespace Brasa.Application.Catalog.OptionGroups;

public sealed record OptionGroupRequest(string? Name, string? Kind, int? MinSelections, int? MaxSelections, IReadOnlyList<OptionRequest>? Options);

public sealed record OptionRequest(Guid? Id, string? Name, long? PriceDeltaInCents);

public sealed record OptionAvailabilityRequest(bool? IsAvailable);

public sealed record OptionGroupResponse(
    Guid Id,
    string Name,
    string Kind,
    int MinSelections,
    int MaxSelections,
    IReadOnlyList<OptionResponse> Options,
    int ProductCount)
{
    public static OptionGroupResponse From(OptionGroup group, int productCount) =>
        new(
            group.Id,
            group.Name,
            group.Kind.ToString(),
            group.MinSelections,
            group.MaxSelections,
            group.Options.Select(OptionResponse.From).ToList(),
            productCount);
}

public sealed record OptionResponse(Guid Id, string Name, long PriceDeltaInCents, bool IsAvailable)
{
    public static OptionResponse From(Option option) =>
        new(option.Id, option.Name, Money.ToCents(option.PriceDelta), option.IsAvailable);
}

public static class OptionGroupErrors
{
    public static readonly Error NotFound = Error.NotFound("option_group_not_found", "Grupo de opções não encontrado.");

    public static readonly Error OptionNotFound = Error.NotFound("option_not_found", "Opção não encontrada.");
}

internal sealed record ValidOptionGroup(string Name, OptionKind Kind, int MinSelections, int MaxSelections, IReadOnlyList<OptionDraft> Options);

internal static class OptionGroupRules
{
    private const long MaxPriceDeltaInCents = 99_999;

    public static ValidOptionGroup? Validate(OptionGroupRequest request, OptionGroup? existing, FieldErrors errors)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            errors.Add("name", "Informe o nome do grupo.");
        }
        else if (name.Length > OptionGroup.NameMaxLength)
        {
            errors.Add("name", $"Use no máximo {OptionGroup.NameMaxLength} caracteres.");
        }

        var kind = ValidateKind(request.Kind, existing, errors);
        var options = ValidateOptions(request.Options, kind, existing, errors);

        var min = request.MinSelections ?? (kind == OptionKind.Choice ? 1 : 0);
        var max = request.MaxSelections ?? (kind == OptionKind.Choice ? 1 : Math.Max(options.Count, 1));

        if (min < 0)
        {
            errors.Add("minSelections", "O mínimo não pode ser negativo.");
        }
        else if (kind == OptionKind.Removal && min > 0)
        {
            errors.Add("minSelections", "Remoção de ingrediente não pode ser obrigatória.");
        }

        if (max < 1)
        {
            errors.Add("maxSelections", "O máximo precisa ser pelo menos 1.");
        }
        else if (min > max)
        {
            errors.Add("maxSelections", "O máximo não pode ser menor que o mínimo.");
        }

        return errors.Any ? null : new ValidOptionGroup(name!, kind, min, max, options);
    }

    private static OptionKind ValidateKind(string? text, OptionGroup? existing, FieldErrors errors)
    {
        if (text is null && existing is not null)
        {
            return existing.Kind;
        }

        if (!EnumText.TryParse<OptionKind>(text, out var kind))
        {
            errors.Add("kind", $"Escolha o tipo: {EnumText.Names<OptionKind>()}.");
            return OptionKind.Addon;
        }

        if (existing is not null && existing.Kind != kind)
        {
            errors.Add("kind", "O tipo do grupo não pode mudar.");
        }

        return kind;
    }

    private static List<OptionDraft> ValidateOptions(IReadOnlyList<OptionRequest>? options, OptionKind kind, OptionGroup? existing, FieldErrors errors)
    {
        if (options is null || options.Count == 0 || options.Count > OptionGroup.MaxOptions)
        {
            errors.Add("options", $"Cadastre de 1 a {OptionGroup.MaxOptions} opções.");
            return [];
        }

        var drafts = new List<OptionDraft>(options.Count);
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < options.Count; index++)
        {
            var option = options[index];
            var field = $"options[{index}]";
            var name = option.Name?.Trim();

            if (string.IsNullOrEmpty(name))
            {
                errors.Add($"{field}.name", "Informe o nome da opção.");
            }
            else if (name.Length > Option.NameMaxLength)
            {
                errors.Add($"{field}.name", $"Use no máximo {Option.NameMaxLength} caracteres.");
            }
            else if (!names.Add(name))
            {
                errors.Add($"{field}.name", "Já existe uma opção com esse nome no grupo.");
            }

            var cents = option.PriceDeltaInCents ?? 0;
            if (cents < 0 || cents > MaxPriceDeltaInCents)
            {
                errors.Add($"{field}.priceDeltaInCents", "O acréscimo vai de R$ 0,00 a R$ 999,99.");
            }
            else if (kind == OptionKind.Removal && cents != 0)
            {
                errors.Add($"{field}.priceDeltaInCents", "Remoção de ingrediente não tem preço.");
            }

            if (option.Id is { } id && existing?.FindOption(id) is null)
            {
                errors.Add($"{field}.id", "Opção não pertence a este grupo.");
            }

            drafts.Add(new OptionDraft(option.Id, name ?? string.Empty, Money.FromCents(cents)));
        }

        return drafts;
    }
}
