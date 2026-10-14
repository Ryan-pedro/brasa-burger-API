namespace Brasa.Application.Shared.Persistence;

public interface IDatabaseErrors
{
    bool IsUniqueViolation(Exception exception);
}
