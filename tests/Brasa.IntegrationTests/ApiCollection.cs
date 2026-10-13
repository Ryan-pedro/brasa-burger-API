namespace Brasa.IntegrationTests;

/// <summary>
/// Todas as classes de teste de integração compartilham uma única API e um único banco.
/// Isso também impede que rodem em paralelo e recriem o banco uma por cima da outra.
/// </summary>
[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<StoreApiFactory>
{
    public const string Name = "api";
}
