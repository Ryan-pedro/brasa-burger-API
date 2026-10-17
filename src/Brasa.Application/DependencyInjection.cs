using Brasa.Application.Catalog.Categories;
using Brasa.Application.Catalog.Images;
using Brasa.Application.Catalog.Menu;
using Brasa.Application.Catalog.Offers;
using Brasa.Application.Catalog.OptionGroups;
using Brasa.Application.Catalog.Products;
using Brasa.Application.Identity.Login;
using Brasa.Application.Identity.Session;
using Brasa.Application.Kitchen.Board;
using Brasa.Application.Kitchen.Tickets;
using Brasa.Application.Ordering.Checkout;
using Brasa.Application.Ordering.Management;
using Brasa.Application.Ordering.Reactions;
using Brasa.Application.Ordering.Tracking;
using Brasa.Application.Payments;
using Brasa.Application.Printing;
using Brasa.Application.Printing.Agents;
using Brasa.Application.Printing.Jobs;
using Brasa.Application.Printing.Printers;
using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Auth;
using Brasa.Application.Shared.Events;
using Brasa.Application.Shared.Jobs;
using Brasa.Application.Shared.Outbox;
using Brasa.Application.Shared.Realtime;
using Brasa.Application.StoreSettings.GetStoreProfile;
using Brasa.Application.StoreSettings.Settings;
using Brasa.Domain.Kitchen;
using Brasa.Domain.Ordering;
using Brasa.Domain.Printing;
using Microsoft.Extensions.DependencyInjection;

namespace Brasa.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CurrentUser>();
        services.AddScoped<AuditTrail>();

        services.AddScoped<GetStoreProfileHandler>();
        services.AddScoped<GetStoreSettingsHandler>();
        services.AddScoped<UpdateStoreSettingsHandler>();
        services.AddScoped<SetStoreStatusHandler>();

        services.AddScoped<LoginHandler>();
        services.AddScoped<GetSessionHandler>();

        services.AddScoped<GetMenuHandler>();
        services.AddScoped<ListCategoriesHandler>();
        services.AddScoped<CreateCategoryHandler>();
        services.AddScoped<UpdateCategoryHandler>();
        services.AddScoped<MoveCategoryHandler>();
        services.AddScoped<ListProductsHandler>();
        services.AddScoped<CreateProductHandler>();
        services.AddScoped<UpdateProductHandler>();
        services.AddScoped<SetProductAvailabilityHandler>();
        services.AddScoped<ListOptionGroupsHandler>();
        services.AddScoped<CreateOptionGroupHandler>();
        services.AddScoped<UpdateOptionGroupHandler>();
        services.AddScoped<SetOptionAvailabilityHandler>();
        services.AddScoped<ListOffersHandler>();
        services.AddScoped<ListActiveOffersHandler>();
        services.AddScoped<CreateOfferHandler>();
        services.AddScoped<UpdateOfferHandler>();
        services.AddScoped<UploadImageHandler>();

        services.AddScoped<PricedCartBuilder>();
        services.AddScoped<QuoteHandler>();
        services.AddScoped<CreateOrderHandler>();
        services.AddScoped<GetOrderTrackingHandler>();
        services.AddScoped<ListOrdersHandler>();
        services.AddScoped<GetOrderHandler>();
        services.AddScoped<ChangeOrderStatusHandler>();
        services.AddScoped<ReceivePaymentHandler>();

        services.AddScoped<GetBoardHandler>();
        services.AddScoped<GetTicketHandler>();
        services.AddScoped<MovePreparationHandler>();

        services.AddScoped<CurrentAgent>();
        services.AddScoped<ActivePrintAgents>();
        services.AddScoped<CreatePairingCodeHandler>();
        services.AddScoped<PairAgentHandler>();
        services.AddScoped<IssueAgentTokenHandler>();
        services.AddScoped<RecordHeartbeatHandler>();
        services.AddScoped<GetAgentConfigHandler>();
        services.AddScoped<ListPrintAgentsHandler>();
        services.AddScoped<RevokePrintAgentHandler>();
        services.AddScoped<CreatePrinterHandler>();
        services.AddScoped<UpdatePrinterHandler>();
        services.AddScoped<PrintTestPageHandler>();
        services.AddScoped<LeasePrintJobsHandler>();
        services.AddScoped<ReportPrintJobResultHandler>();
        services.AddScoped<ReprintTicketHandler>();
        services.AddScoped<ListPrintJobsHandler>();

        services.AddScoped<IDomainEventHandler<OrderPlaced>, CreatePaymentHandler>();
        services.AddScoped<IDomainEventHandler<OrderStatusChanged>, IssueTicketHandler>();
        services.AddScoped<IDomainEventHandler<PreparationChanged>, FollowPreparationHandler>();
        services.AddScoped<RealtimeNoticeReactions>();
        services.AddScoped<IDomainEventHandler<OrderPlaced>>(provider => provider.GetRequiredService<RealtimeNoticeReactions>());
        services.AddScoped<IDomainEventHandler<OrderStatusChanged>>(provider => provider.GetRequiredService<RealtimeNoticeReactions>());
        services.AddScoped<IDomainEventHandler<TicketChanged>>(provider => provider.GetRequiredService<RealtimeNoticeReactions>());
        services.AddScoped<IDomainEventHandler<TicketIssued>, QueuePrintForIssuedTicketHandler>();
        services.AddScoped<IDomainEventHandler<TicketVoided>, CancelPrintsForVoidedTicketHandler>();
        services.AddScoped<PrintingNoticeReactions>();
        services.AddScoped<IDomainEventHandler<PrintJobChanged>>(provider => provider.GetRequiredService<PrintingNoticeReactions>());
        services.AddScoped<IDomainEventHandler<PrinterStatusChanged>>(provider => provider.GetRequiredService<PrintingNoticeReactions>());
        services.AddScoped<IDomainEventHandler<PrintAgentPresenceChanged>>(provider => provider.GetRequiredService<PrintingNoticeReactions>());

        services.AddScoped<DispatchOutboxJob>();
        services.AddScoped<RejectStaleOrdersJob>();
        services.AddScoped<PurgeExpiredRecordsJob>();
        services.AddScoped<RecoverPrintJobsJob>();
        services.AddScoped<MarkSilentAgentsOfflineJob>();

        return services;
    }
}
