using Brasa.Application.Shared.Persistence;
using Npgsql;

namespace Brasa.Infrastructure.Persistence;

internal sealed class DatabaseErrors : IDatabaseErrors
{
    public bool IsUniqueViolation(Exception exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
