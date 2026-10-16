namespace PaymentRequests.Api.Services;

public sealed class RequestValidationException(string message) : Exception(message);

public sealed class DuplicatePaymentReferenceException()
    : Exception("La referencia externa ya está registrada.");

public sealed class FeeCalculatorUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);