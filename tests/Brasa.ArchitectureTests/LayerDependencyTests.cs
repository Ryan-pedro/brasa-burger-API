using System.Reflection;

namespace Brasa.ArchitectureTests;

/// <summary>
/// Garante as regras de dependência entre camadas descritas em
/// docs/architecture/backend-architecture.md (seção 4).
/// Se um destes testes falhar, alguma camada passou a usar o que não devia.
/// </summary>
public class LayerDependencyTests
{
    private const string DomainName = "Brasa.Domain";
    private const string ApplicationName = "Brasa.Application";
    private const string InfrastructureName = "Brasa.Infrastructure";
    private const string ApiName = "Brasa.Api";

    private static readonly Assembly Domain = Brasa.Domain.AssemblyReference.Assembly;
    private static readonly Assembly Application = Brasa.Application.AssemblyReference.Assembly;
    private static readonly Assembly Infrastructure = Brasa.Infrastructure.AssemblyReference.Assembly;

    [Fact]
    public void Domain_nao_depende_de_outras_camadas()
    {
        var references = ReferencesOf(Domain);

        Assert.DoesNotContain(ApplicationName, references);
        Assert.DoesNotContain(InfrastructureName, references);
        Assert.DoesNotContain(ApiName, references);
    }

    [Fact]
    public void Domain_nao_depende_de_infraestrutura_de_terceiros()
    {
        var references = ReferencesOf(Domain);

        Assert.DoesNotContain(references, name => name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
        Assert.DoesNotContain(references, name => name.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal));
        Assert.DoesNotContain(references, name => name.StartsWith("Npgsql", StringComparison.Ordinal));
    }

    [Fact]
    public void Application_nao_depende_de_Infrastructure_nem_de_Api()
    {
        var references = ReferencesOf(Application);

        Assert.DoesNotContain(InfrastructureName, references);
        Assert.DoesNotContain(ApiName, references);
    }

    [Fact]
    public void Application_nao_depende_de_AspNetCore()
    {
        var references = ReferencesOf(Application);

        Assert.DoesNotContain(references, name => name.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal));
    }

    [Fact]
    public void Infrastructure_nao_depende_de_Api()
    {
        var references = ReferencesOf(Infrastructure);

        Assert.DoesNotContain(ApiName, references);
    }

    private static string[] ReferencesOf(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();
}
