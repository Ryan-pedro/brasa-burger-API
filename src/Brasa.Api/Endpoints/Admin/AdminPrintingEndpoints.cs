using Brasa.Api.Auth;
using Brasa.Api.Problems;
using Brasa.Application.Printing.Agents;
using Brasa.Application.Printing.Jobs;
using Brasa.Application.Printing.Printers;
using Permissions = Brasa.Application.Identity.Permissions;

namespace Brasa.Api.Endpoints.Admin;

internal static class AdminPrintingEndpoints
{
    public static RouteGroupBuilder MapAdminPrintingEndpoints(this RouteGroupBuilder secured)
    {
        var printing = secured.MapGroup("/printing").RequirePermission(Permissions.PrintingManage);

        printing.MapPost("/pairing-codes", async (CreatePairingCodeHandler handler, CancellationToken cancellationToken) =>
                Results.Ok(await handler.HandleAsync(cancellationToken)))
            .WithName("AdminCreatePairingCode");

        printing.MapGet("/agents", async (ListPrintAgentsHandler handler, CancellationToken cancellationToken) =>
                Results.Ok(await handler.HandleAsync(cancellationToken)))
            .WithName("AdminListPrintAgents");

        printing.MapPost("/agents/{id:guid}/revoke", async (Guid id, RevokePrintAgentHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(id, cancellationToken)).Match(value => Results.Ok(value)))
            .WithName("AdminRevokePrintAgent");

        printing.MapPost("/agents/{agentId:guid}/printers", async (Guid agentId, PrinterRequest request, CreatePrinterHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(agentId, request, cancellationToken))
                    .Match(printer => Results.Created($"/api/v1/admin/printing/printers/{printer.Id}", printer)))
            .WithName("AdminCreatePrinter");

        printing.MapPut("/printers/{id:guid}", async (Guid id, PrinterRequest request, UpdatePrinterHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(id, request, cancellationToken)).Match(value => Results.Ok(value)))
            .WithName("AdminUpdatePrinter");

        printing.MapPost("/printers/{id:guid}/test", async (Guid id, PrintTestPageHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(id, cancellationToken)).Match(value => Results.Ok(value)))
            .WithName("AdminPrintTestPage");

        printing.MapGet("/jobs", async (string? status, int? limit, Guid? cursor, ListPrintJobsHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(status, limit, cursor, cancellationToken)).Match(value => Results.Ok(value)))
            .WithName("AdminListPrintJobs");

        printing.MapPost("/tickets/{ticketId:guid}/reprint", async (Guid ticketId, ReprintTicketHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(ticketId, cancellationToken)).Match(value => Results.Ok(value)))
            .WithName("AdminReprintTicket");

        return secured;
    }
}
