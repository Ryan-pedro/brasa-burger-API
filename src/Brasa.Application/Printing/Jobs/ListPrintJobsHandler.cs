using Brasa.Application.Shared;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Printing;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Printing.Jobs;

public sealed class ListPrintJobsHandler(IAppDbContext db)
{
    public const int DefaultLimit = 50;
    public const int MaxLimit = 100;

    public async Task<Result<PrintJobsPage>> HandleAsync(string? status, int? limit, Guid? cursor, CancellationToken cancellationToken)
    {
        var errors = new FieldErrors();
        PrintJobStatus? statusFilter = null;

        if (status is not null)
        {
            if (EnumText.TryParse<PrintJobStatus>(status, out var parsed))
            {
                statusFilter = parsed;
            }
            else
            {
                errors.Add("status", $"Situações aceitas: {EnumText.Names<PrintJobStatus>()}.");
            }
        }

        if (limit is < 1 or > MaxLimit)
        {
            errors.Add("limit", $"O limite vai de 1 a {MaxLimit}.");
        }

        if (errors.Any)
        {
            return errors.ToError();
        }

        var pageSize = limit ?? DefaultLimit;
        var query = db.PrintJobs.AsNoTracking();

        if (statusFilter is { } filter)
        {
            query = query.Where(job => job.Status == filter);
        }

        if (cursor is { } before)
        {
            query = query.Where(job => job.Id.CompareTo(before) < 0);
        }

        var jobs = await query
            .OrderByDescending(job => job.Id)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        var page = jobs.Take(pageSize).Select(PrintJobResponse.From).ToList();
        var nextCursor = jobs.Count > pageSize ? page[^1].Id.ToString() : null;

        return new PrintJobsPage(page, nextCursor);
    }
}
