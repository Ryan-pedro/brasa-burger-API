namespace Brasa.Domain.Identity;

/// <summary>Papel de uma pessoa da equipe da loja.</summary>
public enum StaffRole
{
    /// <summary>Proprietário: tudo do gerente, mais equipe e configurações sensíveis.</summary>
    Owner,

    /// <summary>Gerente: catálogo, pedidos, configurações e impressoras.</summary>
    Manager,

    /// <summary>Cozinha: comandas e preparo. Nada administrativo.</summary>
    Kitchen,
}
