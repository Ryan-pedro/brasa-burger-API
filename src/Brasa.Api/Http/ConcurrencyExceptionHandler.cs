using Brasa.Api.Problems;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Api.Http;

internal sealed class ConcurrencyExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not DbUpdateConcurrencyException)
        {
            return false;
        }

        await ApiProblems
            .Create(StatusCodes.Status409Conflict, "Outra pessoa alterou este registro. Atualize e tente de novo.", "version_conflict")
            .ExecuteAsync(httpContext);

        return true;
    }
}
