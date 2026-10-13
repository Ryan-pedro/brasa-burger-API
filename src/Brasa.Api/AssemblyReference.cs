using System.Reflection;

namespace Brasa.Api;

/// <summary>
/// Ponto de referência para localizar este assembly. É usado pelos testes de arquitetura
/// e como ponto de entrada dos testes de integração.
/// </summary>
public sealed class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;

    private AssemblyReference()
    {
    }
}
