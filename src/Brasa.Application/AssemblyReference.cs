using System.Reflection;

namespace Brasa.Application;

/// <summary>
/// Ponto de referência para localizar este assembly, usado pelos testes de arquitetura.
/// </summary>
public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
