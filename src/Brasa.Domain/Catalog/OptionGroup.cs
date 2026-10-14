namespace Brasa.Domain.Catalog;

public sealed class OptionGroup
{
    public const int NameMaxLength = 60;
    public const int MaxOptions = 30;

    private readonly List<Option> _options = [];

    private OptionGroup()
    {
    }

    public OptionGroup(string name, OptionKind kind, int minSelections, int maxSelections)
    {
        Id = Guid.CreateVersion7();
        Kind = kind;
        Rename(name);
        SetLimits(minSelections, maxSelections);
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public OptionKind Kind { get; private set; }

    public int MinSelections { get; private set; }

    public int MaxSelections { get; private set; }

    public IReadOnlyList<Option> Options => _options.OrderBy(option => option.SortOrder).ToList();

    public bool CanBeFulfilled => _options.Count(option => option.IsAvailable) >= MinSelections;

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var trimmed = name.Trim();
        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException($"O nome do grupo pode ter no máximo {NameMaxLength} caracteres.", nameof(name));
        }

        Name = trimmed;
    }

    public void SetLimits(int minSelections, int maxSelections)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minSelections);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxSelections, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minSelections, maxSelections);

        if (Kind == OptionKind.Removal && minSelections > 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minSelections), minSelections, "Remoção de ingrediente não pode ser obrigatória.");
        }

        MinSelections = minSelections;
        MaxSelections = maxSelections;
    }

    public void SetOptions(IReadOnlyList<OptionDraft> drafts)
    {
        ArgumentNullException.ThrowIfNull(drafts);

        if (drafts.Count == 0 || drafts.Count > MaxOptions)
        {
            throw new ArgumentException($"O grupo precisa de 1 a {MaxOptions} opções.", nameof(drafts));
        }

        if (drafts.Select(draft => draft.Name.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() != drafts.Count)
        {
            throw new ArgumentException("Há opções com o mesmo nome.", nameof(drafts));
        }

        if (Kind == OptionKind.Removal && drafts.Any(draft => draft.PriceDelta != 0))
        {
            throw new ArgumentException("Remoção de ingrediente não tem preço.", nameof(drafts));
        }

        var keptIds = drafts.Where(draft => draft.Id is not null).Select(draft => draft.Id!.Value).ToHashSet();
        if (keptIds.Any(id => _options.All(option => option.Id != id)))
        {
            throw new ArgumentException("Opção não pertence a este grupo.", nameof(drafts));
        }

        _options.RemoveAll(option => !keptIds.Contains(option.Id));

        for (var index = 0; index < drafts.Count; index++)
        {
            var draft = drafts[index];
            if (draft.Id is { } id)
            {
                _options.Single(option => option.Id == id).Change(draft.Name, draft.PriceDelta, index);
            }
            else
            {
                _options.Add(new Option(draft.Name, draft.PriceDelta, index));
            }
        }
    }

    public Option? FindOption(Guid optionId) => _options.FirstOrDefault(option => option.Id == optionId);

    public void SetOptionAvailability(Guid optionId, bool isAvailable)
    {
        var option = FindOption(optionId) ?? throw new ArgumentException("Opção não pertence a este grupo.", nameof(optionId));
        option.SetAvailability(isAvailable);
    }
}
