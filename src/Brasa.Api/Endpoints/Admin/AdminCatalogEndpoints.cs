using Brasa.Api.Auth;
using Brasa.Api.Problems;
using Brasa.Application.Catalog.Categories;
using Brasa.Application.Catalog.Products;
using Permissions = Brasa.Application.Identity.Permissions;

namespace Brasa.Api.Endpoints.Admin;

/// <summary>Cardápio no painel: categorias, produtos e disponibilidade.</summary>
internal static class AdminCatalogEndpoints
{
    /// <param name="secured">Grupo do painel já com sessão obrigatória e revalidada.</param>
    public static RouteGroupBuilder MapAdminCatalogEndpoints(this RouteGroupBuilder secured)
    {
        var catalog = secured.MapGroup("/catalog");

        catalog.MapGet("/categories", async (ListCategoriesHandler handler, CancellationToken cancellationToken) =>
                Results.Ok(await handler.HandleAsync(cancellationToken)))
            .RequirePermission(Permissions.CatalogRead)
            .WithName("AdminListCategories");

        catalog.MapPost("/categories", async (CategoryRequest request, CreateCategoryHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(request, cancellationToken))
                    .Match(category => Results.Created($"/api/v1/admin/catalog/categories/{category.Id}", category)))
            .RequirePermission(Permissions.CatalogWrite)
            .WithName("AdminCreateCategory");

        catalog.MapPut("/categories/{id:guid}", async (Guid id, CategoryRequest request, UpdateCategoryHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(id, request, cancellationToken)).Match(value => Results.Ok(value)))
            .RequirePermission(Permissions.CatalogWrite)
            .WithName("AdminUpdateCategory");

        catalog.MapGet("/products", async (Guid? categoryId, ListProductsHandler handler, CancellationToken cancellationToken) =>
                Results.Ok(await handler.HandleAsync(categoryId, cancellationToken)))
            .RequirePermission(Permissions.CatalogRead)
            .WithName("AdminListProducts");

        catalog.MapPost("/products", async (ProductRequest request, CreateProductHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(request, cancellationToken))
                    .Match(product => Results.Created($"/api/v1/admin/catalog/products/{product.Id}", product)))
            .RequirePermission(Permissions.CatalogWrite)
            .WithName("AdminCreateProduct");

        catalog.MapPut("/products/{id:guid}", async (Guid id, ProductRequest request, UpdateProductHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(id, request, cancellationToken)).Match(value => Results.Ok(value)))
            .RequirePermission(Permissions.CatalogWrite)
            .WithName("AdminUpdateProduct");

        catalog.MapPut("/products/{id:guid}/availability", async (Guid id, AvailabilityRequest request, SetProductAvailabilityHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(id, request, cancellationToken)).Match(value => Results.Ok(value)))
            .RequirePermission(Permissions.CatalogWrite)
            .WithName("AdminSetProductAvailability");

        return secured;
    }
}
