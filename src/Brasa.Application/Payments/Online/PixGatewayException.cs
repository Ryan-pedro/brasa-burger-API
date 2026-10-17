namespace Brasa.Application.Payments.Online;

public sealed class PixGatewayException(string message, Exception? innerException = null) : Exception(message, innerException);
