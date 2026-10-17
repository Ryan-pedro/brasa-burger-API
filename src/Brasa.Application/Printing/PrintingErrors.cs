using Brasa.Application.Shared.Results;

namespace Brasa.Application.Printing;

public static class PrintingErrors
{
    public static readonly Error PairingCodeInvalid = Error.Rule("pairing_code_invalid", "Código de pareamento inválido, já usado ou expirado.");

    public static readonly Error AgentNotFound = Error.NotFound("print_agent_not_found", "Agente de impressão não encontrado.");

    public static readonly Error AgentRevoked = Error.Conflict("print_agent_revoked", "Este agente foi revogado.");

    public static readonly Error PrinterNotFound = Error.NotFound("printer_not_found", "Impressora não encontrada.");

    public static readonly Error NoDefaultPrinter = Error.Conflict("no_default_printer", "Nenhuma impressora padrão ativa. Configure uma no painel.");

    public static readonly Error JobNotFound = Error.NotFound("print_job_not_found", "Tarefa de impressão não encontrada.");

    public static readonly Error AttemptNotCurrent = Error.Conflict("print_attempt_not_current", "Esta tentativa não é a reserva atual da tarefa.");
}
