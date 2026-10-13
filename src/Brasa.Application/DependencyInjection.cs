using Brasa.Application.Catalog.Categories;
using Brasa.Application.Catalog.Menu;
using Brasa.Application.Catalog.Products;
using Brasa.Application.Identity.Login;
using Brasa.Application.Identity.Session;
using Brasa.Application.Shared.Auth;
using Brasa.Application.StoreSettings.GetStoreProfile;
using Microsoft.Extensions.DependencyInjection;

namespace Brasa.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Uma instância por requisição: guarda o usuário corrente.
        services.AddScoped<CurrentUser>();

        services.AddScoped<GetStoreProfileHandler>();

        services.AddScoped<LoginHandler>();
        services.AddScoped<GetSessionHandler>();

        services.AddScoped<GetMenuHandler>();
        services.AddScoped<ListCategoriesHandler>();
        services.AddScoped<CreateCategoryHandler>();
        services.AddScoped<UpdateCategoryHandler>();
        services.AddScoped<ListProductsHandler>();
        services.AddScoped<CreateProductHandler>();
        services.AddScoped<UpdateProductHandler>();
        services.AddScoped<SetProductAvailabilityHandler>();

        return services;
    }
}
