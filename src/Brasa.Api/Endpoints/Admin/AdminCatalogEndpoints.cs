using Brasa.Api.Auth;
using Brasa.Api.Problems;
using Brasa.Application.Catalog.Categories;
using Brasa.Application.Catalog.Images;
using Brasa.Application.Catalog.Offers;
using Brasa.Application.Catalog.OptionGroups;
using Brasa.Application.Catalog.Products;
using Microsoft.AspNetCore.Mvc;
using Permissions = Brasa.Application.Identity.Permissions;

namespace Brasa.Api.Endpoints.Admin;

/// <summary>Cardápio no painel: categorias, produtos, grupos de opções, ofertas e imagens.</summary>
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

        catalog.MapPut("/categories/{id:guid}/parent", async (Guid id, CategoryParentRequest request, MoveCategoryHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(id, request, cancellationToken)).Match(value => Results.Ok(value)))
            .RequirePermission(Permissions.CatalogWrite)
            .WithName("AdminMoveCategory");

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

        catalog.MapGet("/option-groups", async (ListOptionGroupsHandler handler, CancellationToken cancellationToken) =>
                Results.Ok(await handler.HandleAsync(cancellationToken)))
            .RequirePermission(Permissions.CatalogRead)
            .WithName("AdminListOptionGroups");

        catalog.MapPost("/option-groups", async (OptionGroupRequest request, CreateOptionGroupHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(request, cancellationToken))
                    .Match(group => Results.Created($"/api/v1/admin/catalog/option-groups/{group.Id}", group)))
            .RequirePermission(Permissions.CatalogWrite)
            .WithName("AdminCreateOptionGroup");

        catalog.MapPut("/option-groups/{id:guid}", async (Guid id, OptionGroupRequest request, UpdateOptionGroupHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(id, request, cancellationToken)).Match(value => Results.Ok(value)))
            .RequirePermission(Permissions.CatalogWrite)
            .WithName("AdminUpdateOptionGroup");

        catalog.MapPut("/option-groups/{id:guid}/options/{optionId:guid}/availability", async (Guid id, Guid optionId, OptionAvailabilityRequest request, SetOptionAvailabilityHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(id, optionId, request, cancellationToken)).Match(value => Results.Ok(value)))
            .RequirePermission(Permissions.CatalogWrite)
            .WithName("AdminSetOptionAvailability");

        catalog.MapGet("/offers", async (ListOffersHandler handler, CancellationToken cancellationToken) =>
                Results.Ok(await handler.HandleAsync(cancellationToken)))
            .RequirePermission(Permissions.CatalogRead)
            .WithName("AdminListOffers");

        catalog.MapPost("/offers", async (OfferRequest request, CreateOfferHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(request, cancellationToken))
                    .Match(offer => Results.Created($"/api/v1/admin/catalog/offers/{offer.Id}", offer)))
            .RequirePermission(Permissions.OffersWrite)
            .WithName("AdminCreateOffer");

        catalog.MapPut("/offers/{id:guid}", async (Guid id, OfferRequest request, UpdateOfferHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(id, request, cancellationToken)).Match(value => Results.Ok(value)))
            .RequirePermission(Permissions.OffersWrite)
            .WithName("AdminUpdateOffer");

        catalog.MapPost("/images", async (IFormFile file, UploadImageHandler handler, CancellationToken cancellationToken) =>
            {
                await using var content = file.OpenReadStream();
                return (await handler.HandleAsync(content, file.Length, cancellationToken))
                    .Match(image => Results.Created(image.Url, image));
            })
            .RequirePermission(Permissions.CatalogWrite)
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(UploadImageHandler.MaxBytes + 64 * 1024))
            .WithName("AdminUploadImage");

        return secured;
    }
}
