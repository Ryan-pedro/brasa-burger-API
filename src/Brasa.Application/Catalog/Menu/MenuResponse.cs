namespace Brasa.Application.Catalog.Menu;

/// <summary>O cardápio público, numa resposta só: o Storefront monta todas as telas a partir dela.</summary>
public sealed record MenuResponse(IReadOnlyList<MenuCategory> Categories);
