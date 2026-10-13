using Brasa.Domain.StoreSettings;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Shared.Persistence;

/// <summary>
/// Acesso a dados usado pelos casos de uso. A implementação fica em Infrastructure.
/// </summary>
public interface IAppDbContext
{
    DbSet<Store> Stores { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
